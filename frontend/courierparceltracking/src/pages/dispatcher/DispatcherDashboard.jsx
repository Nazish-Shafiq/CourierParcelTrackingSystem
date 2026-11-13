import { useCallback } from 'react'
import { CircleDashed, Package, Truck } from 'lucide-react'
import DashboardStatCard from '../../components/cards/DashboardStatCard'
import DashboardSection from '../../components/common/DashboardSection'
import ErrorMessage from '../../components/common/ErrorMessage'
import LoadingSpinner from '../../components/common/LoadingSpinner'
import PageHeader from '../../components/common/PageHeader'
import RefreshButton from '../../components/common/RefreshButton'
import StatusBadge from '../../components/common/StatusBadge'
import DataTable from '../../components/tables/DataTable'
import { useDashboardData } from '../../hooks/useDashboardData'
import { getDispatcherDashboard } from '../../services/dashboardService'
import '../../style/dispatcher/dispatcher-dashboard.css'

function formatDate(value) {
  return value ? new Date(value).toLocaleString() : '—'
}

function DispatcherDashboard() {
  const loadDashboard = useCallback((signal) => getDispatcherDashboard(signal), [])
  const { data, error, isLoading, refresh } = useDashboardData(loadDashboard)

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
            <DashboardStatCard icon={Package} label="Total parcels" value={data.totalParcels} />
            <DashboardStatCard icon={CircleDashed} label="Unassigned parcels" value={data.unassignedParcels} accent="amber" />
            <DashboardStatCard label="Active assignments" value={data.activeAssignments} accent="violet" />
            <DashboardStatCard label="Parcels in transit" value={data.parcelsInTransit} />
            <DashboardStatCard icon={Truck} label="Available couriers" value={data.availableCouriers} accent="green" />
            <DashboardStatCard icon={Truck} label="Busy couriers" value={data.busyCouriers} accent="amber" />
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
                rows={data.recentUnassignedParcels}
                getRowKey={(row) => row.parcelId}
              />
            </DashboardSection>
          </div>
        </>
      )}
    </div>
  )
}

export default DispatcherDashboard
