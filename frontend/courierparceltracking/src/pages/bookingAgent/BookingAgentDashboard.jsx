import { useCallback, useMemo } from 'react'
import { ClipboardList, Package, PackageCheck, PackageSearch, Truck, Users } from 'lucide-react'
import { Link, useNavigate } from 'react-router-dom'
import DashboardSection from '../../components/common/DashboardSection'
import DashboardStatCard from '../../components/cards/DashboardStatCard'
import EmptyState from '../../components/common/EmptyState'
import ErrorMessage from '../../components/common/ErrorMessage'
import LoadingSpinner from '../../components/common/LoadingSpinner'
import PageHeader from '../../components/common/PageHeader'
import RefreshButton from '../../components/common/RefreshButton'
import StatusBadge from '../../components/common/StatusBadge'
import DataTable from '../../components/tables/DataTable'
import { useDashboardData } from '../../hooks/useDashboardData'
import { getCustomers } from '../../services/customerService'
import { getParcelBookings } from '../../services/bookingService'
import { getParcels } from '../../services/parcelService'
import '../../style/bookingAgent/booking-agent-dashboard.css'

function normalizeList(data) {
  if (Array.isArray(data)) return data
  if (data && Array.isArray(data.items)) return data.items
  return []
}

function getStatusKey(value = '') {
  return String(value).trim().toLowerCase()
}

function formatDate(value) {
  return value ? new Date(value).toLocaleDateString() : '—'
}

function isSameDay(dateValue, reference = new Date()) {
  if (!dateValue) return false
  const date = new Date(dateValue)
  return date.getFullYear() === reference.getFullYear()
    && date.getMonth() === reference.getMonth()
    && date.getDate() === reference.getDate()
}

function BookingAgentDashboard() {
  const navigate = useNavigate()

  const loadDashboard = useCallback(async (signal) => {
    const [customersResult, bookingsResult, parcelsResult] = await Promise.all([
      getCustomers({ page: 1, pageSize: 100, signal }),
      getParcelBookings({ page: 1, pageSize: 100, signal }),
      getParcels(signal),
    ])

    return {
      customers: normalizeList(customersResult),
      bookings: normalizeList(bookingsResult),
      parcels: normalizeList(parcelsResult),
    }
  }, [])

  const dashboard = useDashboardData(loadDashboard)

  const summary = useMemo(() => {
    const parcels = dashboard.data?.parcels ?? []
    const customers = dashboard.data?.customers ?? []
    const bookings = dashboard.data?.bookings ?? []

    const statusCounts = parcels.reduce((accumulator, parcel) => {
      const statusName = parcel.currentStatus ?? parcel.currentStatusName ?? parcel.statusName ?? ''
      const key = getStatusKey(statusName)

      if (!key) return accumulator

      accumulator[key] = (accumulator[key] ?? 0) + 1
      return accumulator
    }, {})

    return {
      totalParcels: parcels.length,
      totalCustomers: customers.length,
      todaysBookings: parcels.filter((parcel) => isSameDay(parcel.bookingDate)).length,
      pendingParcels: (statusCounts.booked ?? 0) + (statusCounts.pending ?? 0) + (statusCounts['pending pickup'] ?? 0),
      inTransitParcels: (statusCounts['in transit'] ?? 0) + (statusCounts.in_transit ?? 0) + (statusCounts['out for delivery'] ?? 0),
      deliveredParcels: (statusCounts.delivered ?? 0) + (statusCounts['delivery complete'] ?? 0),
      failedDeliveries: (statusCounts.failed ?? 0) + (statusCounts['failed delivery'] ?? 0) + (statusCounts['delivery failed'] ?? 0),
      recentBookings: bookings.slice(0, 5),
      recentCustomers: customers
        .slice()
        .sort((a, b) => new Date(b.createdAt) - new Date(a.createdAt))
        .slice(0, 5),
      statusOverview: Object.entries(statusCounts).map(([name, total]) => ({ statusName: name, total })),
    }
  }, [dashboard.data])

  const quickActions = [
    { label: 'Register customer', path: '/booking/register-customer', icon: Users },
    { label: 'Book parcel', path: '/booking/book-parcel', icon: Package },
    { label: 'Parcel bookings', path: '/booking/bookings', icon: ClipboardList },
    { label: 'Track parcel', path: '/booking/track-parcel', icon: PackageSearch },
  ]

  return (
    <div className="role-dashboard role-dashboard--booking-agent">
      <PageHeader
        actions={(
          <RefreshButton isLoading={dashboard.isLoading} onRefresh={dashboard.refresh} />
        )}
        description="Operational metrics for customer registration and parcel booking activity."
        eyebrow="Operations"
        title="Booking Agent Dashboard"
      />

      {dashboard.isLoading && !dashboard.data && <LoadingSpinner label="Loading booking activity" />}
      {dashboard.error && <ErrorMessage error={dashboard.error} onRetry={dashboard.refresh} />}

      {dashboard.data && (
        <>
          <div className="dashboard-stat-grid">
            <DashboardStatCard icon={Package} label="Total parcels booked" onClick={() => navigate('/booking/bookings?filter=all')} value={summary.totalParcels} />
            <DashboardStatCard accent="green" icon={Users} label="Total customers registered" onClick={() => navigate('/booking/customers')} value={summary.totalCustomers} />
            <DashboardStatCard accent="violet" icon={ClipboardList} label="Today's bookings" onClick={() => navigate('/booking/bookings?filter=today')} value={summary.todaysBookings} />
            <DashboardStatCard accent="amber" icon={PackageSearch} label="Pending parcels" onClick={() => navigate('/booking/bookings?filter=pending')} value={summary.pendingParcels} />
            <DashboardStatCard icon={Truck} label="In transit parcels" onClick={() => navigate('/booking/bookings?filter=transit')} value={summary.inTransitParcels} />
            <DashboardStatCard accent="green" icon={PackageCheck} label="Delivered parcels" onClick={() => navigate('/booking/bookings?filter=delivered')} value={summary.deliveredParcels} />
            <DashboardStatCard accent="red" icon={PackageSearch} label="Failed deliveries" onClick={() => navigate('/booking/bookings?filter=failed')} value={summary.failedDeliveries} />
          </div>

          <DashboardSection description="Live counts derived from the valid parcel and customer data returned by the backend." title="Parcel booking overview">
            <div className="dashboard-status-grid">
              {summary.statusOverview.length === 0 ? (
                <p className="dashboard-inline-empty">No parcel status data is available yet.</p>
              ) : (
                summary.statusOverview.map((status) => (
                  <article className="dashboard-status-card" key={status.statusName}>
                    <StatusBadge status={status.statusName.replace(/_/g, ' ')} />
                    <strong>{status.total}</strong>
                  </article>
                ))
              )}
            </div>
          </DashboardSection>

          <DashboardSection title="Recent parcel bookings">
            {summary.recentBookings.length === 0
              ? <EmptyState description="No recent bookings were returned by the booking API." title="No recent bookings" />
              : (
                <DataTable
                  caption="Recent parcel bookings"
                  columns={[
                    { key: 'trackingNumber', header: 'Tracking number' },
                    { key: 'senderCustomerName', header: 'Sender' },
                    { key: 'currentStatusName', header: 'Status', render: (row) => <StatusBadge status={row.currentStatusName} /> },
                    { key: 'bookingDate', header: 'Booked', render: (row) => formatDate(row.bookingDate) },
                    { key: 'totalWeightKg', header: 'Weight', render: (row) => `${row.totalWeightKg} kg` },
                  ]}
                  rows={summary.recentBookings}
                  getRowKey={(row) => row.parcelId}
                />
              )}
          </DashboardSection>

          <DashboardSection title="Recent customer registrations">
            {summary.recentCustomers.length === 0
              ? <EmptyState description="No customer registrations were returned by the customer API." title="No recent registrations" />
              : (
                <DataTable
                  caption="Recent customer registrations"
                  columns={[
                    { key: 'fullName', header: 'Customer' },
                    { key: 'phone', header: 'Phone' },
                    { key: 'city', header: 'City' },
                    { key: 'createdAt', header: 'Registered', render: (row) => formatDate(row.createdAt) },
                  ]}
                  rows={summary.recentCustomers}
                  getRowKey={(row) => row.customerId}
                />
              )}
          </DashboardSection>

          <DashboardSection description="Common actions available to the booking agent." title="Quick actions">
            <div className="booking-agent-quick-links">
              {quickActions.map(({ icon: Icon, label, path }) => (
                <Link className="booking-agent-quick-link" key={path} to={path}>
                  <span className="booking-agent-quick-link__icon"><Icon aria-hidden="true" size={18} /></span>
                  <span>{label}</span>
                </Link>
              ))}
            </div>
          </DashboardSection>

          <DashboardSection title="Booking activity summary">
            <div className="booking-agent-summary-grid">
              <div><span>Bookings loaded</span><strong>{summary.recentBookings.length}</strong></div>
              <div><span>Registered customers</span><strong>{summary.totalCustomers}</strong></div>
              <div><span>Current active parcel states</span><strong>{summary.statusOverview.length}</strong></div>
            </div>
          </DashboardSection>
        </>
      )}
    </div>
  )
}

export default BookingAgentDashboard
