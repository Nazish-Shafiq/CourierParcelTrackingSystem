import { useCallback, useMemo, useState } from 'react'
import { Link, useParams, useSearchParams } from 'react-router-dom'
import EmptyState from '../../components/common/EmptyState'
import ErrorMessage from '../../components/common/ErrorMessage'
import LoadingSpinner from '../../components/common/LoadingSpinner'
import PageHeader from '../../components/common/PageHeader'
import RefreshButton from '../../components/common/RefreshButton'
import SearchBar from '../../components/common/SearchBar'
import StatusBadge from '../../components/common/StatusBadge'
import FormSelect from '../../components/forms/FormSelect'
import DataTable from '../../components/tables/DataTable'
import Pagination from '../../components/tables/Pagination'
import { useDashboardData } from '../../hooks/useDashboardData'
import { getCourierDashboard } from '../../services/dashboardService'
import { getTrackingHistory } from '../../services/trackingService'
import '../../style/courier/courier-pages.css'

const PAGE_SIZE = 10

function getOwnAssignedParcel(dashboard, parcelId) {
  const assignedParcels = Array.isArray(dashboard?.assignedParcels)
    ? dashboard.assignedParcels
    : []
  const parcel = assignedParcels.find((item) => String(item.parcelId) === String(parcelId))

  if (!parcel) {
    throw new Error('This parcel is not currently assigned to your courier account.')
  }

  return parcel
}

function formatDate(value) {
  if (!value) return '—'
  const date = new Date(value)
  return Number.isNaN(date.getTime()) ? '—' : date.toLocaleString()
}

function CourierAssignedParcelsPage() {
  const [searchParams, setSearchParams] = useSearchParams()
  const [search, setSearch] = useState('')
  const [currentPage, setCurrentPage] = useState(1)
  const statusFilter = searchParams.get('status') ?? ''
  const loadDashboard = useCallback((signal) => getCourierDashboard(signal), [])
  const dashboard = useDashboardData(loadDashboard)
  const assignedParcels = useMemo(
    () => (Array.isArray(dashboard.data?.assignedParcels) ? dashboard.data.assignedParcels : []),
    [dashboard.data],
  )

  const statusOptions = useMemo(() => (
    [...new Set(assignedParcels.map((parcel) => parcel.currentStatus).filter(Boolean))]
      .sort((left, right) => left.localeCompare(right))
      .map((status) => ({ value: status, label: status }))
  ), [assignedParcels])

  const filteredParcels = useMemo(() => {
    const query = search.trim().toLowerCase()
    return assignedParcels.filter((parcel) => {
      const matchesStatus = !statusFilter || parcel.currentStatus === statusFilter
      const searchable = [
        parcel.trackingNumber,
        parcel.senderName,
        parcel.recipientName,
        parcel.recipientCity,
        parcel.currentStatus,
      ].filter(Boolean).join(' ').toLowerCase()

      return matchesStatus && (!query || searchable.includes(query))
    })
  }, [assignedParcels, search, statusFilter])

  const pageCount = Math.ceil(filteredParcels.length / PAGE_SIZE)
  const pageRows = filteredParcels.slice((currentPage - 1) * PAGE_SIZE, currentPage * PAGE_SIZE)

  function updateStatus(event) {
    const nextSearchParams = new URLSearchParams(searchParams)
    if (event.target.value) nextSearchParams.set('status', event.target.value)
    else nextSearchParams.delete('status')
    setSearchParams(nextSearchParams, { replace: true })
    setCurrentPage(1)
  }

  return (
    <div className="role-dashboard role-dashboard--courier">
      <PageHeader
        actions={<RefreshButton isLoading={dashboard.isLoading} onRefresh={dashboard.refresh} />}
        description="Only active assignments returned for your authenticated courier account are shown."
        eyebrow="My work"
        title="My Assigned Parcels"
      />
      {dashboard.isLoading && !dashboard.data && (
        <LoadingSpinner label="Loading your assigned parcels" />
      )}
      {dashboard.error && <ErrorMessage error={dashboard.error} onRetry={dashboard.refresh} />}
      {dashboard.data && !dashboard.error && (
        <>
          <div className="courier-parcel-toolbar">
            <SearchBar
              label="Search assigned parcels"
              onChange={(value) => {
                setSearch(value)
                setCurrentPage(1)
              }}
              placeholder="Tracking number, sender, recipient, city, or status"
              value={search}
            />
            <FormSelect
              id="courier-assigned-parcel-status"
              label="Filter by status"
              onChange={updateStatus}
              options={statusOptions}
              placeholder="All statuses"
              value={statusFilter}
            />
          </div>
          <section className="courier-parcel-list-card">
            <DataTable
              caption="Parcels assigned to the signed-in courier"
              columns={[
                { key: 'trackingNumber', header: 'Tracking number' },
                { key: 'senderName', header: 'Sender' },
                { key: 'recipientName', header: 'Recipient', render: (row) => row.recipientName || '—' },
                { key: 'recipientCity', header: 'Destination', render: (row) => row.recipientCity || '—' },
                { key: 'currentStatus', header: 'Status', render: (row) => <StatusBadge status={row.currentStatus} /> },
                { key: 'assignedAt', header: 'Assigned', render: (row) => formatDate(row.assignedAt) },
              ]}
              emptyState={(
                <EmptyState
                  description={search || statusFilter
                    ? 'Adjust the search or status filter and try again.'
                    : 'There are no active parcel assignments linked to your courier account.'}
                  title="No assigned parcels found"
                />
              )}
              getRowKey={(row) => row.parcelAssignmentId}
              rowActions={(row) => (
                <Link
                  aria-label={`View details for ${row.trackingNumber}`}
                  className="ui-button ui-button--outline"
                  to={`/courier/assigned-parcels/${row.parcelId}`}
                >
                  View details
                </Link>
              )}
              rows={pageRows}
            />
            <Pagination
              currentPage={currentPage}
              onPageChange={setCurrentPage}
              pageCount={pageCount}
              pageSize={PAGE_SIZE}
              totalRecords={filteredParcels.length}
            />
          </section>
          <p className="courier-api-note">
            Weight, route, and pickup details are not included in the courier-scoped dashboard response.
          </p>
        </>
      )}
    </div>
  )
}

function CourierParcelDetailsPage() {
  const { parcelId } = useParams()
  const loadParcel = useCallback(
    async (signal) => {
      const dashboard = await getCourierDashboard(signal)
      return getOwnAssignedParcel(dashboard, parcelId)
    },
    [parcelId],
  )
  const parcelState = useDashboardData(loadParcel)
  const parcel = parcelState.data

  return (
    <div className="role-dashboard role-dashboard--courier">
      <PageHeader
        actions={(
          <>
            <Link className="ui-button ui-button--outline" to="/courier/assigned-parcels">
              Back to my parcels
            </Link>
            {parcel && (
              <Link
                className="ui-button ui-button--primary"
                to={`/courier/assigned-parcels/${parcel.parcelId}/tracking`}
              >
                Tracking history
              </Link>
            )}
          </>
        )}
        description="Details are limited to the information returned for your active courier assignments."
        eyebrow="My work"
        title={parcel ? `Parcel ${parcel.trackingNumber}` : 'Parcel Details'}
      />
      {parcelState.isLoading && !parcel && <LoadingSpinner label="Loading parcel details" />}
      {parcelState.error && <ErrorMessage error={parcelState.error} onRetry={parcelState.refresh} />}
      {parcel && (
        <section className="courier-parcel-details">
          <div className="courier-parcel-details__status">
            <span>Current status</span>
            <StatusBadge status={parcel.currentStatus} />
          </div>
          <dl className="courier-parcel-details__grid">
            <div><dt>Tracking number</dt><dd>{parcel.trackingNumber}</dd></div>
            <div><dt>Parcel ID</dt><dd>{parcel.parcelId}</dd></div>
            <div><dt>Sender</dt><dd>{parcel.senderName || '—'}</dd></div>
            <div><dt>Recipient</dt><dd>{parcel.recipientName || '—'}</dd></div>
            <div><dt>Destination city</dt><dd>{parcel.recipientCity || '—'}</dd></div>
            <div><dt>Assigned</dt><dd>{formatDate(parcel.assignedAt)}</dd></div>
            <div><dt>Assignment ID</dt><dd>{parcel.parcelAssignmentId}</dd></div>
          </dl>
          <p className="courier-api-note">
            The courier dashboard does not return parcel weight, route, hub, or additional contact details.
          </p>
        </section>
      )}
    </div>
  )
}

function CourierParcelTrackingPage() {
  const { parcelId } = useParams()
  const loadTracking = useCallback(async (signal) => {
    const dashboard = await getCourierDashboard(signal)
    const parcel = getOwnAssignedParcel(dashboard, parcelId)
    const events = await getTrackingHistory(parcel.trackingNumber, signal)

    if (!Array.isArray(events)) {
      throw new Error('The tracking history response was not a list.')
    }

    return {
      parcel,
      events: events.slice().sort((left, right) => new Date(left.eventAt) - new Date(right.eventAt)),
    }
  }, [parcelId])
  const tracking = useDashboardData(loadTracking)

  return (
    <div className="role-dashboard role-dashboard--courier">
      <PageHeader
        actions={(
          <>
            <Link
              className="ui-button ui-button--outline"
              to={`/courier/assigned-parcels/${parcelId}`}
            >
              Parcel details
            </Link>
            <RefreshButton isLoading={tracking.isLoading} onRefresh={tracking.refresh} />
          </>
        )}
        description="Read-only tracking events for a parcel currently assigned to your courier account."
        eyebrow="My work"
        title={tracking.data ? `Tracking · ${tracking.data.parcel.trackingNumber}` : 'Tracking History'}
      />
      {tracking.isLoading && !tracking.data && (
        <LoadingSpinner label="Loading parcel tracking history" />
      )}
      {tracking.error && <ErrorMessage error={tracking.error} onRetry={tracking.refresh} />}
      {tracking.data && tracking.data.events.length === 0 && (
        <EmptyState
          description="No tracking events were returned for this assigned parcel."
          title="No tracking history"
        />
      )}
      {tracking.data?.events.length > 0 && (
        <section aria-label="Parcel tracking events" className="courier-tracking-timeline">
          {tracking.data.events.map((event) => (
            <article className="courier-tracking-event" key={event.statusEventId}>
              <span aria-hidden="true" className="courier-tracking-event__marker" />
              <div className="courier-tracking-event__content">
                <div className="courier-tracking-event__heading">
                  <StatusBadge status={event.statusName} />
                  <time dateTime={event.eventAt}>{formatDate(event.eventAt)}</time>
                </div>
                <h2>{event.statusName} <span>({event.statusCode})</span></h2>
                {event.hubName && <p>Hub: {event.hubName}</p>}
                {event.courierName && <p>Courier: {event.courierName}</p>}
                {event.notes && <p>{event.notes}</p>}
              </div>
            </article>
          ))}
        </section>
      )}
    </div>
  )
}

export {
  CourierAssignedParcelsPage,
  CourierParcelDetailsPage,
  CourierParcelTrackingPage,
}
