import { useCallback } from 'react'
import { Building2, Package, Route, Users } from 'lucide-react'
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
import {
  getAdminDashboard,
  getCourierPerformanceReport,
} from '../../services/dashboardService'
import { getUsers } from '../../services/userService'
import '../../style/admin/admin-dashboard.css'

function formatDate(value) {
  return value ? new Date(value).toLocaleDateString() : '—'
}

function AdminDashboard() {
  const navigate = useNavigate()
  const loadDashboard = useCallback((signal) => getAdminDashboard(signal), [])
  const loadPerformanceReport = useCallback(
    (signal) => getCourierPerformanceReport({ signal }),
    [],
  )
  const loadUsers = useCallback((signal) => getUsers(signal), [])
  const dashboard = useDashboardData(loadDashboard)
  const performanceReport = useDashboardData(loadPerformanceReport)
  const users = useDashboardData(loadUsers)

  function refreshAll() {
    dashboard.refresh()
    performanceReport.refresh()
    users.refresh()
  }

  function countUsersForRole(roleName) {
    if (!Array.isArray(users.data)) return '—'
    return users.data.filter((user) => (
      user.roles.some((role) => role.toLowerCase() === roleName.toLowerCase())
    )).length
  }

  return (
    <div className="role-dashboard role-dashboard--admin">
      <PageHeader
        actions={(
          <RefreshButton
            isLoading={dashboard.isLoading || performanceReport.isLoading || users.isLoading}
            onRefresh={refreshAll}
          />
        )}
        description="Operational metrics, parcel activity, and courier performance from the backend."
        eyebrow="Administration"
        title="Admin Dashboard"
      />

      {dashboard.isLoading && !dashboard.data && <LoadingSpinner label="Loading admin dashboard" />}
      {dashboard.error && <ErrorMessage error={dashboard.error} onRetry={dashboard.refresh} />}
      {users.error && (
        <ErrorMessage error={users.error} onRetry={users.refresh} />
      )}

      {dashboard.data && (
        <>
          <div className="dashboard-stat-grid">
            <DashboardStatCard
              icon={Package}
              label="Total parcels"
              onClick={() => navigate('/admin/parcels')}
              value={dashboard.data.totalParcels}
            />
            <DashboardStatCard
              accent="green"
              icon={Users}
              label="Total customers"
              onClick={() => navigate('/admin/customers')}
              value={dashboard.data.totalCustomers}
            />
            <DashboardStatCard
              accent="violet"
              icon={Users}
              label="Total couriers"
              onClick={() => navigate('/admin/couriers')}
              value={dashboard.data.totalCouriers}
            />
            <DashboardStatCard
              accent="amber"
              icon={Building2}
              label="Total hubs"
              onClick={() => navigate('/admin/hubs')}
              value={dashboard.data.totalHubs}
            />
            <DashboardStatCard
              icon={Route}
              label="Total routes"
              onClick={() => navigate('/admin/routes')}
              value={dashboard.data.totalRoutes}
            />
            <DashboardStatCard
              icon={Users}
              label="Active booking agents"
              onClick={() => navigate('/admin/users')}
              value={countUsersForRole(ROLE_NAMES.BOOKING_AGENT)}
            />
            <DashboardStatCard
              accent="violet"
              icon={Users}
              label="Active dispatchers"
              onClick={() => navigate('/admin/users')}
              value={countUsersForRole(ROLE_NAMES.DISPATCHER)}
            />
            <DashboardStatCard
              accent="green"
              label="Available couriers"
              onClick={() => navigate('/admin/couriers')}
              value={dashboard.data.availableCouriers}
            />
            <DashboardStatCard
              accent="amber"
              label="Busy couriers"
              onClick={() => navigate('/admin/couriers')}
              value={dashboard.data.busyCouriers}
            />
          </div>

          <DashboardSection
            description="Counts for each active parcel status returned by the API."
            title="Operational summary"
          >
            {dashboard.data.parcelStatusSummary.length === 0 ? (
              <p className="dashboard-inline-empty">No parcel status data is available.</p>
            ) : (
              <div className="dashboard-status-grid">
                {dashboard.data.parcelStatusSummary.map((status) => (
                  <article className="dashboard-status-card" key={status.parcelStatusId}>
                    <StatusBadge status={status.statusName} />
                    <strong>{status.totalParcels}</strong>
                  </article>
                ))}
              </div>
            )}
          </DashboardSection>

          <DashboardSection title="Recent parcel bookings">
            {dashboard.data.recentBookings.length === 0
              ? <EmptyState description="No recent parcel bookings were returned." title="No recent bookings" />
              : (
                <DataTable
                  caption="Recent parcel bookings"
                  columns={[
                    { key: 'trackingNumber', header: 'Tracking number' },
                    { key: 'senderName', header: 'Sender' },
                    { key: 'statusName', header: 'Status', render: (row) => <StatusBadge status={row.statusName} /> },
                    { key: 'bookingDate', header: 'Booked', render: (row) => formatDate(row.bookingDate) },
                    { key: 'expectedDeliveryDate', header: 'Expected delivery', render: (row) => formatDate(row.expectedDeliveryDate) },
                  ]}
                  rows={dashboard.data.recentBookings}
                  getRowKey={(row) => row.parcelId}
                />
              )}
          </DashboardSection>
        </>
      )}

      <DashboardSection
        description="Assignment and delivery totals from the courier-performance report."
        title="Courier performance overview"
      >
        {performanceReport.isLoading && !performanceReport.data && (
          <LoadingSpinner label="Loading courier performance" size="small" />
        )}
        {performanceReport.error && (
          <ErrorMessage error={performanceReport.error} onRetry={performanceReport.refresh} />
        )}
        {performanceReport.data && (
          performanceReport.data.length === 0
            ? <EmptyState description="No courier performance records were returned." title="No performance data" />
            : (
              <DataTable
                caption="Courier assignment and delivery totals"
                columns={[
                  { key: 'courierName', header: 'Courier' },
                  { key: 'employeeCode', header: 'Employee code' },
                  { key: 'totalAssignedParcels', header: 'Assigned parcels' },
                  { key: 'deliveredParcels', header: 'Delivered parcels' },
                ]}
                rows={performanceReport.data}
                getRowKey={(row) => row.courierId}
              />
            )
        )}
      </DashboardSection>

      <DashboardSection title="Quick navigation">
        <div className="admin-quick-links">
          {ROLE_NAVIGATION[ROLE_NAMES.ADMIN].map(({ icon: Icon, label, path }) => (
            <Link className="admin-quick-link" key={path} to={path}>
              <span className="admin-quick-link__icon">
                <Icon aria-hidden="true" size={19} />
              </span>
              <span>{label}</span>
            </Link>
          ))}
        </div>
      </DashboardSection>
    </div>
  )
}

export default AdminDashboard
