import { useCallback } from 'react'
import { CircleDashed, ClipboardList, Package, PackageCheck, Truck } from 'lucide-react'
import { Link, useNavigate } from 'react-router-dom'
import DashboardStatCard from '../../components/cards/DashboardStatCard'
import DashboardSection from '../../components/common/DashboardSection'
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
import '../../style/dispatcher/dispatcher-dashboard.css'

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
              onClick={() => navigate('/dispatch/assignments')}
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

          <DashboardSection
            description="The current API does not expose hub-transfer endpoints or transfer history."
            title="Hub transfers unavailable"
          >
            <p className="dispatcher-dashboard__notice">
              Hub transfer management is not available in this phase because the backend has no
              supported transfer operations.
            </p>
          </DashboardSection>
        </>
      )}
    </div>
  )
}

export default DispatcherDashboard
