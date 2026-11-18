import { useCallback, useMemo, useState } from 'react'
import { Link, useSearchParams } from 'react-router-dom'
import DataTable from '../../components/tables/DataTable'
import EmptyState from '../../components/common/EmptyState'
import ErrorMessage from '../../components/common/ErrorMessage'
import LoadingSpinner from '../../components/common/LoadingSpinner'
import PageHeader from '../../components/common/PageHeader'
import RefreshButton from '../../components/common/RefreshButton'
import StatusBadge from '../../components/common/StatusBadge'
import { useDashboardData } from '../../hooks/useDashboardData'
import { getParcelBookings } from '../../services/bookingService'

function formatDate(value) {
  return value ? new Date(value).toLocaleDateString() : '—'
}

function getItems(payload) {
  if (Array.isArray(payload)) return payload
  if (payload && Array.isArray(payload.items)) return payload.items
  return []
}

function BookingAgentBookingsPage() {
  const [searchParams] = useSearchParams()
  const filter = searchParams.get('filter') ?? 'all'
  const [search, setSearch] = useState('')
  const loadBookings = useCallback((signal) => getParcelBookings({ page: 1, pageSize: 100, signal }), [])
  const bookings = useDashboardData(loadBookings)

  const rows = useMemo(() => {
    const items = getItems(bookings.data)
    const query = search.trim().toLowerCase()
    const today = new Date()
    return items.filter((row) => {
      const status = String(row.currentStatusName ?? '').trim().toLowerCase()
      const bookingDate = row.bookingDate ? new Date(row.bookingDate) : null
      const matchesFilter =
        filter === 'today'
          ? bookingDate && bookingDate.getFullYear() === today.getFullYear()
            && bookingDate.getMonth() === today.getMonth()
            && bookingDate.getDate() === today.getDate()
          : filter === 'pending'
            ? status === 'booked' || status === 'pending'
            : filter === 'transit'
              ? status === 'in transit' || status === 'out for delivery'
              : filter === 'delivered'
                ? status === 'delivered'
                : filter === 'failed'
                  ? status === 'delivery attempted' || status.includes('failed')
                  : true
      if (!matchesFilter) return false
      if (!query) return true
      return [
        row.trackingNumber,
        row.senderCustomerName,
        row.currentStatusName,
        row.routeName,
      ].filter(Boolean).join(' ').toLowerCase().includes(query)
    })
  }, [bookings.data, filter, search])

  return (
    <div className="role-dashboard role-dashboard--booking-agent">
      <PageHeader
        actions={(
          <RefreshButton isLoading={bookings.isLoading} onRefresh={bookings.refresh} />
        )}
        description="Parcel booking records captured by the backend and available to the Booking Agent role."
        eyebrow="Booking records"
        title="Parcel Bookings"
      />

      {bookings.isLoading && !bookings.data && <LoadingSpinner label="Loading bookings" />}
      {bookings.error && <ErrorMessage error={bookings.error} onRetry={bookings.refresh} />}

      {!bookings.error && (
        <section className="booking-agent-list-card">
          <div className="booking-agent-list-card__header">
            <input
              aria-label="Search parcel bookings"
              className="ui-form-field__control"
              onChange={(event) => setSearch(event.target.value)}
              placeholder="Search tracking number, sender, status, or route"
              value={search}
            />
          </div>
          <DataTable
            caption="Booking records"
            columns={[
              { key: 'trackingNumber', header: 'Tracking number' },
              { key: 'senderCustomerName', header: 'Sender' },
              { key: 'currentStatusName', header: 'Status', render: (row) => <StatusBadge status={row.currentStatusName} /> },
              { key: 'bookingDate', header: 'Booked', render: (row) => formatDate(row.bookingDate) },
              { key: 'totalWeightKg', header: 'Weight', render: (row) => `${row.totalWeightKg} kg` },
            ]}
            emptyState={(
              <EmptyState
                description="No booking records were returned by the parcel booking API."
                title="No parcel bookings"
              />
            )}
            getRowKey={(row) => row.parcelId}
            rows={rows}
          />
          <div className="booking-agent-list-card__footer my-2">
            <Link className="ui-button ui-button--outline" to="/booking/book-parcel">Create another booking</Link>
          </div>
        </section>
      )}
    </div>
  )
}

export default BookingAgentBookingsPage
