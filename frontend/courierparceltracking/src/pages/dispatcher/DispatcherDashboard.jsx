import { useCallback, useMemo, useState } from 'react'
import { CircleDashed, ClipboardList, Package, PackageCheck, Truck } from 'lucide-react'
import { Link, useNavigate, useSearchParams } from 'react-router-dom'
import DashboardStatCard from '../../components/cards/DashboardStatCard'
import DashboardSection from '../../components/common/DashboardSection'
import DashboardViewAll, { DASHBOARD_PREVIEW_LIMIT } from '../../components/common/DashboardViewAll'
import EmptyState from '../../components/common/EmptyState'
import ErrorMessage from '../../components/common/ErrorMessage'
import LoadingSpinner from '../../components/common/LoadingSpinner'
import PageHeader from '../../components/common/PageHeader'
import RefreshButton from '../../components/common/RefreshButton'
import StatusBadge from '../../components/common/StatusBadge'
import DataTable from '../../components/tables/DataTable'
import { ROLE_NAMES, ROLE_NAVIGATION } from '../../constants/navigation'
import { useDashboardData } from '../../hooks/useDashboardData'
import { getDispatcherDashboard } from '../../services/dashboardService'
import { getDispatcherDelayPredictions } from '../../services/parcelDelayPredictionService'
import '../../style/dispatcher/dispatcher-dashboard.css'
import '../../style/dispatcher/parcel-delay-predictions.css'

function formatDate(value) {
  return value ? new Date(value).toLocaleString() : '—'
}

function DispatcherDashboard() {
  const navigate = useNavigate()
  const loadDashboard = useCallback((signal) => getDispatcherDashboard(signal), [])
  const { data, error, isLoading, refresh } = useDashboardData(loadDashboard)
  const quickActions = ROLE_NAVIGATION[ROLE_NAMES.DISPATCHER]

  return (
    <div className="role-dashboard role-dashboard--dispatcher">
      <PageHeader
        actions={<RefreshButton isLoading={isLoading} onRefresh={refresh} />}
        description="Parcel assignments, unassigned work, and courier availability."
        eyebrow="Operations"
        title="Dispatcher Dashboard"
      />

      {isLoading && !data && <LoadingSpinner label="Loading dispatcher dashboard" />}
      {error && <ErrorMessage error={error} onRetry={refresh} />}

      {data && (
        <>
          <div className="dashboard-stat-grid">
            <DashboardStatCard
              icon={Package}
              label="Total parcels"
              onClick={() => navigate('/dispatch/parcels')}
              value={data.totalParcels}
            />
            <DashboardStatCard
              accent="amber"
              icon={CircleDashed}
              label="Unassigned parcels"
              onClick={() => navigate('/dispatch/unassigned-parcels')}
              value={data.unassignedParcels}
            />
            <DashboardStatCard
              accent="violet"
              icon={ClipboardList}
              label="Active assignments"
              onClick={() => navigate('/dispatch/assignments?active=true')}
              value={data.activeAssignments}
            />
            <DashboardStatCard
              icon={PackageCheck}
              label="Parcels in transit"
              onClick={() => navigate('/dispatch/parcels?status=In%20Transit')}
              value={data.parcelsInTransit}
            />
            <DashboardStatCard
              accent="green"
              icon={Truck}
              label="Available couriers"
              onClick={() => navigate('/dispatch/couriers?availability=available')}
              value={data.availableCouriers}
            />
            <DashboardStatCard
              accent="amber"
              icon={Truck}
              label="Busy couriers"
              onClick={() => navigate('/dispatch/couriers?availability=busy')}
              value={data.busyCouriers}
            />
          </div>

          <div className="dashboard-section-grid">
            <DashboardSection title="Recent active assignments">
              <DataTable
                caption="Five most recently assigned parcels"
                columns={[
                  { key: 'trackingNumber', header: 'Tracking number' },
                  { key: 'courierName', header: 'Courier' },
                  { key: 'assignedAt', header: 'Assigned', render: (row) => formatDate(row.assignedAt) },
                ]}
                emptyState={(
                  <EmptyState
                    description="There are no active courier assignments to display."
                    title="No active assignments"
                  />
                )}
                rows={data.recentAssignments}
                getRowKey={(row) => row.parcelAssignmentId}
              />
              <DashboardViewAll
                count={data.activeAssignments}
                label="View all active assignments"
                to="/dispatch/assignments?active=true"
              />
            </DashboardSection>

            <DashboardSection title="Parcels awaiting assignment">
              <DataTable
                caption="Five most recently booked unassigned parcels"
                columns={[
                  { key: 'trackingNumber', header: 'Tracking number' },
                  { key: 'senderName', header: 'Sender' },
                  { key: 'statusName', header: 'Status', render: (row) => <StatusBadge status={row.statusName} /> },
                  { key: 'bookingDate', header: 'Booked', render: (row) => formatDate(row.bookingDate) },
                ]}
                emptyState={(
                  <EmptyState
                    description="There are no parcels awaiting assignment."
                    title="No unassigned parcels"
                  />
                )}
                rows={data.recentUnassignedParcels}
                getRowKey={(row) => row.parcelId}
              />
              <DashboardViewAll
                count={data.unassignedParcels}
                label="View all unassigned parcels"
                to="/dispatch/unassigned-parcels"
              />
            </DashboardSection>
          </div>

          <DashboardSection
            description="Dispatcher screens supported by the current backend APIs."
            title="Quick navigation"
          >
            <div className="dispatcher-quick-links">
              {quickActions.map(({ icon: Icon, label, path }) => (
                <Link className="dispatcher-quick-link" key={path} to={path}>
                  <span className="dispatcher-quick-link__icon">
                    <Icon aria-hidden="true" size={18} />
                  </span>
                  <span>{label}</span>
                </Link>
              ))}
            </div>
          </DashboardSection>

        </>
      )}

      <ParcelDelayPredictionSection />
    </div>
  )
}

function ParcelDelayPredictionSection() {
  const [searchParams] = useSearchParams()
  const showAll = searchParams.get('predictions') === 'all'
  const [search, setSearch] = useState('')
  const [riskFilter, setRiskFilter] = useState('All')
  const loadPredictions = useCallback(
    (signal) => getDispatcherDelayPredictions(signal),
    [],
  )
  const predictions = useDashboardData(loadPredictions)
  const filteredPredictions = useMemo(() => {
    const rows = predictions.data?.predictions ?? []
    const normalizedSearch = search.trim().toLowerCase()
    return rows.filter((row) => (
      (riskFilter === 'All' || row.riskLevel === riskFilter)
      && (!normalizedSearch || row.trackingNumber.toLowerCase().includes(normalizedSearch))
    ))
  }, [predictions.data, riskFilter, search])

  return (
    <div id="parcel-delay-predictions">
      <DashboardSection
        description="Use evaluated delivery history to prioritize at-risk parcels. Predictions never change assignments or parcel status."
        title="Parcel delay predictions"
      >
      {predictions.isLoading && !predictions.data && (
        <LoadingSpinner label="Loading parcel delay predictions" size="small" />
      )}
      {predictions.error && (
        <ErrorMessage error={predictions.error} onRetry={predictions.refresh} />
      )}
      {predictions.data && !predictions.data.isModelAvailable && (
        <EmptyState
          description={predictions.data.message}
          title="Predictions unavailable"
        />
      )}
      {predictions.data?.isModelAvailable && (
        <>
          <p className="delay-predictions__notice" role="status">
            {predictions.data.message}
          </p>
          <div className="delay-predictions__filters">
            <label>
              <span>Search tracking number</span>
              <input
                aria-label="Search predictions by tracking number"
                onChange={(event) => setSearch(event.target.value)}
                type="search"
                value={search}
              />
            </label>
            <label>
              <span>Risk level</span>
              <select
                aria-label="Filter predictions by risk"
                onChange={(event) => setRiskFilter(event.target.value)}
                value={riskFilter}
              >
                <option>All</option>
                <option>High</option>
                <option>Medium</option>
                <option>Low</option>
              </select>
            </label>
          </div>
          {filteredPredictions.length === 0 ? (
            <EmptyState
              description={predictions.data.predictions.length === 0
                ? 'There are no undelivered parcels with complete route and schedule data.'
                : 'No parcels match the selected search and risk filters.'}
              title="No matching predictions"
            />
          ) : (
            <>
              <DataTable
                caption="Parcel delay risk predictions"
                columns={[
                  { key: 'trackingNumber', header: 'Tracking number' },
                  { key: 'routeName', header: 'Route' },
                  {
                    key: 'hubs',
                    header: 'Origin → destination',
                    render: (row) => `${row.originHubName} → ${row.destinationHubName}`,
                  },
                  {
                    key: 'expectedDeliveryDate',
                    header: 'Expected delivery',
                    render: (row) => formatDate(row.expectedDeliveryDate),
                  },
                  {
                    key: 'riskLevel',
                    header: 'Delay risk',
                    render: (row) => (
                      <span className={`delay-risk-badge delay-risk-badge--${row.riskLevel.toLowerCase()}`}>
                        {row.riskLevel}
                      </span>
                    ),
                  },
                  {
                    key: 'delayProbability',
                    header: 'Probability',
                    render: (row) => `${(row.delayProbability * 100).toFixed(1)}%`,
                  },
                ]}
                emptyState={<EmptyState title="No eligible parcels" />}
                getRowKey={(row) => row.parcelId}
                rows={showAll
                  ? filteredPredictions
                  : filteredPredictions.slice(0, DASHBOARD_PREVIEW_LIMIT)}
              />
              <DashboardViewAll
                count={filteredPredictions.length}
                label="View all delay predictions"
                to="/dashboard/dispatcher?predictions=all#parcel-delay-predictions"
              />
            </>
          )}
        </>
      )}
      </DashboardSection>
    </div>
  )
}

export default DispatcherDashboard
