import { useCallback, useEffect, useMemo, useState } from 'react'
import { Link, useSearchParams } from 'react-router-dom'
import Button from '../../components/common/Button'
import EmptyState from '../../components/common/EmptyState'
import ErrorMessage from '../../components/common/ErrorMessage'
import PageHeader from '../../components/common/PageHeader'
import RefreshButton from '../../components/common/RefreshButton'
import SearchBar from '../../components/common/SearchBar'
import StatusBadge from '../../components/common/StatusBadge'
import FormInput from '../../components/forms/FormInput'
import DataTable from '../../components/tables/DataTable'
import Pagination from '../../components/tables/Pagination'
import AdminEntityManager from '../../components/admin/AdminEntityManager'
import { routesConfig } from '../admin/adminResourceConfigs'
import { useDashboardData } from '../../hooks/useDashboardData'
import { getHubs } from '../../services/hubService'
import { getTrackingHistory } from '../../services/trackingService'
import { getDispatcherCouriers } from '../../services/dispatcherService'
import '../../style/dispatcher/dispatcher-pages.css'

const PAGE_SIZE = 10

function formatDate(value) {
  return value ? new Date(value).toLocaleString() : '—'
}

function getCourierAvailability(courier) {
  if (courier.hasActiveAssignment) return 'Busy'
  return courier.isAvailable ? 'Available' : 'Unavailable'
}

function DispatcherCouriersPage() {
  const [searchParams, setSearchParams] = useSearchParams()
  const [search, setSearch] = useState('')
  const [currentPage, setCurrentPage] = useState(1)
  const availabilityFilter = searchParams.get('availability') ?? ''
  const loadCouriers = useCallback((signal) => getDispatcherCouriers(signal), [])
  const couriers = useDashboardData(loadCouriers)
  const records = useMemo(
    () => (Array.isArray(couriers.data) ? couriers.data : []),
    [couriers.data],
  )

  const filteredCouriers = useMemo(() => {
    const query = search.trim().toLowerCase()
    return records.filter((courier) => {
      const availability = getCourierAvailability(courier).toLowerCase()
      const matchesAvailability = !availabilityFilter || availability === availabilityFilter
      const searchable = [
        courier.employeeCode,
        courier.fullName,
        courier.vehicleType,
      ].filter(Boolean).join(' ').toLowerCase()

      return matchesAvailability && (!query || searchable.includes(query))
    })
  }, [availabilityFilter, records, search])

  const pageCount = Math.ceil(filteredCouriers.length / PAGE_SIZE)
  const pageRows = filteredCouriers.slice((currentPage - 1) * PAGE_SIZE, currentPage * PAGE_SIZE)

  return (
    <div className="role-dashboard role-dashboard--dispatcher">
      <PageHeader
        actions={<RefreshButton isLoading={couriers.isLoading} onRefresh={couriers.refresh} />}
        description="Review active couriers and their current assignment availability."
        eyebrow="Dispatch resources"
        title="Couriers"
      />
      {couriers.isLoading && !couriers.data && (
        <div className="dispatcher-inline-loading" role="status">Loading couriers…</div>
      )}
      {couriers.error && <ErrorMessage error={couriers.error} onRetry={couriers.refresh} />}
      {couriers.data && !couriers.error && (
        <>
          <div className="dispatcher-list-toolbar dispatcher-list-toolbar--couriers">
            <SearchBar
              label="Search couriers"
              onChange={(value) => {
                setSearch(value)
                setCurrentPage(1)
              }}
              placeholder="Name, employee code, or vehicle type"
              value={search}
            />
            <label className="dispatcher-filter-field">
              <span>Availability</span>
              <select
                className="ui-form-field__control"
                onChange={(event) => {
                  const nextSearchParams = new URLSearchParams(searchParams)
                  if (event.target.value) nextSearchParams.set('availability', event.target.value)
                  else nextSearchParams.delete('availability')
                  setSearchParams(nextSearchParams, { replace: true })
                  setCurrentPage(1)
                }}
                value={availabilityFilter}
              >
                <option value="">All couriers</option>
                <option value="available">Available</option>
                <option value="busy">Busy</option>
                <option value="unavailable">Unavailable</option>
              </select>
            </label>
          </div>
          <section className="dispatcher-list-card">
            <DataTable
              caption="Courier directory"
              columns={[
                { key: 'employeeCode', header: 'Employee code' },
                { key: 'fullName', header: 'Name' },
                { key: 'vehicleType', header: 'Vehicle', render: (row) => row.vehicleType || '—' },
                {
                  key: 'availability',
                  header: 'Availability',
                  render: (row) => <StatusBadge status={getCourierAvailability(row)} />,
                },
              ]}
              emptyState={(
                <EmptyState
                  description="Try another search or availability filter."
                  title="No couriers found"
                />
              )}
              getRowKey={(row) => row.courierId}
              rows={pageRows}
            />
            <Pagination
              currentPage={currentPage}
              onPageChange={setCurrentPage}
              pageCount={pageCount}
              pageSize={PAGE_SIZE}
              totalRecords={filteredCouriers.length}
            />
          </section>
        </>
      )}
    </div>
  )
}

function DispatcherHubsPage() {
  const [search, setSearch] = useState('')
  const [debouncedSearch, setDebouncedSearch] = useState('')
  const [currentPage, setCurrentPage] = useState(1)

  useEffect(() => {
    const timeout = window.setTimeout(() => {
      setDebouncedSearch(search.trim())
      setCurrentPage(1)
    }, 250)
    return () => window.clearTimeout(timeout)
  }, [search])

  const loadHubs = useCallback(
    (signal) => getHubs({
      page: currentPage,
      pageSize: PAGE_SIZE,
      search: debouncedSearch || undefined,
      signal,
    }),
    [currentPage, debouncedSearch],
  )
  const hubs = useDashboardData(loadHubs)
  const rows = hubs.data?.items ?? []

  return (
    <div className="role-dashboard role-dashboard--dispatcher">
      <PageHeader
        actions={<RefreshButton isLoading={hubs.isLoading} onRefresh={hubs.refresh} />}
        description="Review active and inactive hubs. Parcel transfers between hubs are not supported by the current API."
        eyebrow="Dispatch resources"
        title="Hubs"
      />
      {hubs.isLoading && !hubs.data && (
        <div className="dispatcher-inline-loading" role="status">Loading hubs…</div>
      )}
      {hubs.error && <ErrorMessage error={hubs.error} onRetry={hubs.refresh} />}
      {hubs.data && !hubs.error && (
        <>
          <SearchBar
            label="Search hubs"
            onChange={setSearch}
            placeholder="Search hub name, code, or location"
            value={search}
          />
          <section className="dispatcher-list-card">
            <DataTable
              caption="Hubs"
              columns={[
                { key: 'hubCode', header: 'Hub code' },
                { key: 'hubName', header: 'Hub name' },
                { key: 'city', header: 'City' },
                { key: 'province', header: 'Province', render: (row) => row.province || '—' },
                { key: 'country', header: 'Country' },
                { key: 'contactPhone', header: 'Contact phone', render: (row) => row.contactPhone || '—' },
                {
                  key: 'isActive',
                  header: 'Status',
                  render: (row) => <StatusBadge status={row.isActive ? 'Active' : 'Inactive'} />,
                },
              ]}
              emptyState={(
                <EmptyState
                  description={debouncedSearch ? 'Try another search term.' : 'No hubs were returned by the API.'}
                  title="No hubs found"
                />
              )}
              getRowKey={(row) => row.hubId}
              rows={rows}
            />
            <Pagination
              currentPage={currentPage}
              onPageChange={setCurrentPage}
              pageCount={hubs.data.totalPages}
              pageSize={hubs.data.pageSize}
              totalRecords={hubs.data.totalRecords}
            />
          </section>
        </>
      )}
    </div>
  )
}

function DispatcherTrackingPage() {
  const [trackingNumber, setTrackingNumber] = useState('')
  const [history, setHistory] = useState(null)
  const [error, setError] = useState(null)
  const [isLoading, setIsLoading] = useState(false)

  async function handleSubmit(event) {
    event.preventDefault()
    const query = trackingNumber.trim()
    if (!query) {
      setError(new Error('Enter a tracking number to view parcel history.'))
      setHistory(null)
      return
    }

    setIsLoading(true)
    setError(null)
    setHistory(null)
    try {
      const result = await getTrackingHistory(query)
      if (!Array.isArray(result)) {
        throw new Error('The tracking history response was not a list.')
      }
      setHistory(result.slice().sort((left, right) => (
        new Date(left.eventAt) - new Date(right.eventAt)
      )))
    } catch (requestError) {
      setError(requestError)
    } finally {
      setIsLoading(false)
    }
  }

  return (
    <div className="role-dashboard role-dashboard--dispatcher">
      <PageHeader
        description="Look up tracking events by tracking number. This screen is read-only."
        eyebrow="Parcel movement"
        title="Tracking"
      />
      <section className="dispatcher-form-card dispatcher-tracking-card">
        <form className="dispatcher-tracking-form" onSubmit={handleSubmit}>
          <FormInput
            id="dispatcher-tracking-number"
            label="Tracking number"
            onChange={(event) => setTrackingNumber(event.target.value)}
            placeholder="Enter tracking number"
            value={trackingNumber}
          />
          <Button isLoading={isLoading} type="submit">Search</Button>
        </form>
      </section>
      {error && <ErrorMessage error={error} />}
      {history && history.length === 0 && (
        <EmptyState
          description="No tracking events were returned for this tracking number."
          title="No tracking history"
        />
      )}
      {history?.some((event) => event.statusCode?.toUpperCase() === 'DELIVERY_ATTEMPTED') && (
        <p className="dispatcher-failed-attempt" role="status">
          This parcel has a recorded unsuccessful delivery attempt. Review or reassign its active assignment in{' '}
          <Link to="/dispatch/assignments">Parcel Assignments</Link>.
        </p>
      )}
      {history?.length > 0 && (
        <section className="dispatcher-list-card">
          <div className="dispatcher-list-card__header">
            <div>
              <h2 className="dispatcher-list-card__title">Tracking history</h2>
              <p className="dispatcher-list-card__description">{history[0].trackingNumber}</p>
            </div>
          </div>
          <div className="dispatcher-tracking-timeline">
            {history.map((event) => (
              <article className="dispatcher-tracking-event" key={event.statusEventId}>
                <div className="dispatcher-tracking-event__marker" />
                <div className="dispatcher-tracking-event__content">
                  <div className="dispatcher-tracking-event__topline">
                    <StatusBadge status={event.statusName} />
                    <time dateTime={event.eventAt}>{formatDate(event.eventAt)}</time>
                  </div>
                  <p><strong>{event.statusName}</strong> ({event.statusCode})</p>
                  {event.notes && <p>{event.notes}</p>}
                  {event.hubName && <p>Hub: {event.hubName}</p>}
                  {event.courierName && <p>Courier: {event.courierName}</p>}
                </div>
              </article>
            ))}
          </div>
        </section>
      )}
    </div>
  )
}

function DispatcherRoutesPage() {
  return (
    <AdminEntityManager
      config={{
        ...routesConfig,
        eyebrow: 'Dispatch operations',
        description: 'Manage routes between active hubs using the existing route API.',
      }}
    />
  )
}

export {
  DispatcherCouriersPage,
  DispatcherHubsPage,
  DispatcherRoutesPage,
  DispatcherTrackingPage,
}
