import { useCallback, useEffect, useState } from 'react'
import { Link, useSearchParams } from 'react-router-dom'
import DataTable from '../../components/tables/DataTable'
import EmptyState from '../../components/common/EmptyState'
import ErrorMessage from '../../components/common/ErrorMessage'
import LoadingSpinner from '../../components/common/LoadingSpinner'
import PageHeader from '../../components/common/PageHeader'
import RefreshButton from '../../components/common/RefreshButton'
import StatusBadge from '../../components/common/StatusBadge'
import Pagination from '../../components/tables/Pagination'
import { useDashboardData } from '../../hooks/useDashboardData'
import { getParcelBookings } from '../../services/bookingService'

const PAGE_SIZE = 10

function formatDate(value) {
  return value ? new Date(value).toLocaleDateString() : '—'
}

function getTodayBookingRange() {
  const start = new Date()
  start.setHours(0, 0, 0, 0)
  const end = new Date(start)
  end.setDate(end.getDate() + 1)
  return {
    fromBookingDate: start.toISOString(),
    beforeBookingDate: end.toISOString(),
  }
}

function BookingAgentBookingsPage() {
  const [searchParams] = useSearchParams()
  const filter = searchParams.get('filter') ?? 'all'
  const [pageState, setPageState] = useState(() => ({ filter, page: 1 }))
  const [search, setSearch] = useState('')
  const [debouncedSearch, setDebouncedSearch] = useState('')
  const page = pageState.filter === filter ? pageState.page : 1

  useEffect(() => {
    const timeout = window.setTimeout(() => {
      setDebouncedSearch(search.trim())
    }, 250)
    return () => window.clearTimeout(timeout)
  }, [search])

  const loadBookings = useCallback((signal) => {
    const dateRange = filter === 'today' ? getTodayBookingRange() : {}
    return getParcelBookings({
      page,
      pageSize: PAGE_SIZE,
      search: debouncedSearch || undefined,
      filter,
      ...dateRange,
      signal,
    })
  }, [debouncedSearch, filter, page])
  const bookings = useDashboardData(loadBookings)
  const rows = Array.isArray(bookings.data?.items) ? bookings.data.items : []

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
              onChange={(event) => {
                setPageState({ filter, page: 1 })
                setSearch(event.target.value)
              }}
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
          <Pagination
            currentPage={page}
            onPageChange={(nextPage) => setPageState({ filter, page: nextPage })}
            pageCount={bookings.data?.totalPages}
            pageSize={bookings.data?.pageSize ?? PAGE_SIZE}
            totalRecords={bookings.data?.totalRecords}
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
