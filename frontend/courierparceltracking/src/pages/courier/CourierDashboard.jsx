import { useCallback } from 'react'
import { Clock3, MapPin, Package, Truck } from 'lucide-react'
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
import { useDashboardData } from '../../hooks/useDashboardData'
import { useAuth } from '../../hooks/useAuth'
import { getCourierDashboard } from '../../services/dashboardService'
import '../../style/courier/courier-dashboard.css'

function formatDate(value) {
  return value ? new Date(value).toLocaleString() : '—'
}

function CourierDashboard() {
  const { user } = useAuth()
  const navigate = useNavigate()
  const loadDashboard = useCallback((signal) => getCourierDashboard(signal), [])
  const { data, error, isLoading, refresh } = useDashboardData(loadDashboard)
  const assignedParcels = Array.isArray(data?.assignedParcels) ? data.assignedParcels : []

  return (
    <div className="role-dashboard role-dashboard--courier">
      <PageHeader
        actions={<RefreshButton isLoading={isLoading} onRefresh={refresh} />}
        description={data?.employeeCode ? `Courier · ${data.employeeCode}` : 'Your assigned parcel workload.'}
        eyebrow="My work"
        title={`Courier Dashboard${data?.courierName ? ` · ${data.courierName}` : user?.fullName ? ` · ${user.fullName}` : ''}`}
      />

      {isLoading && !data && <LoadingSpinner label="Loading your courier dashboard" />}
      {error && <ErrorMessage error={error} onRetry={refresh} />}

      {data && (
        <>
          <div className="dashboard-stat-grid">
            <DashboardStatCard
              icon={Package}
              label="Total assigned parcels"
              onClick={() => navigate('/courier/assigned-parcels')}
              value={data.totalAssignedParcels}
            />
            <DashboardStatCard
              icon={Truck}
              label="Parcels in transit"
              onClick={() => navigate('/courier/assigned-parcels?status=In%20Transit')}
              value={data.parcelsInTransit}
              accent="amber"
            />
            <DashboardStatCard
              icon={MapPin}
              label="Parcels delivered"
              onClick={() => navigate('/courier/delivery-history')}
              value={data.parcelsDelivered}
              accent="green"
            />
          </div>

          <DashboardSection
            description="Active assignments associated with your authenticated courier account."
            title="My assigned parcels"
          >
            <DataTable
              caption="Parcels assigned to the signed-in courier"
              columns={[
                { key: 'trackingNumber', header: 'Tracking number' },
                { key: 'currentStatus', header: 'Status', render: (row) => <StatusBadge status={row.currentStatus} /> },
                { key: 'recipientName', header: 'Recipient' },
                { key: 'recipientCity', header: 'Destination' },
                { key: 'assignedAt', header: 'Assigned', render: (row) => formatDate(row.assignedAt) },
              ]}
              emptyState={(
                <EmptyState
                  description="No active parcel assignments are currently linked to your courier account."
                  title="No assigned parcels"
                />
              )}
              rows={assignedParcels.slice(0, 5)}
              getRowKey={(row) => row.parcelAssignmentId}
            />
          </DashboardSection>

          <DashboardSection title="Quick action">
            <div className="courier-quick-links">
              <Link className="courier-quick-link" to="/courier/assigned-parcels">
                <span className="courier-quick-link__icon">
                  <Package aria-hidden="true" size={18} />
                </span>
                <span>Review my assigned parcels</span>
              </Link>
              <Link className="courier-quick-link" to="/courier/delivery-history">
                <span className="courier-quick-link__icon">
                  <Clock3 aria-hidden="true" size={18} />
                </span>
                <span>View completed deliveries</span>
              </Link>
            </div>
          </DashboardSection>
        </>
      )}
    </div>
  )
}

export default CourierDashboard
