import { useCallback, useMemo, useState } from 'react'
import { Eye, PackagePlus } from 'lucide-react'
import { Link, useSearchParams } from 'react-router-dom'
import Button from '../../components/common/Button'
import EmptyState from '../../components/common/EmptyState'
import ErrorMessage from '../../components/common/ErrorMessage'
import PageHeader from '../../components/common/PageHeader'
import RefreshButton from '../../components/common/RefreshButton'
import SearchBar from '../../components/common/SearchBar'
import StatusBadge from '../../components/common/StatusBadge'
import FormSelect from '../../components/forms/FormSelect'
import DataTable from '../../components/tables/DataTable'
import Pagination from '../../components/tables/Pagination'
import { useDashboardData } from '../../hooks/useDashboardData'
import { getParcels } from '../../services/parcelService'
import { getDispatcherUnassignedParcels } from '../../services/dispatcherService'
import '../../style/dispatcher/dispatcher-pages.css'

const PAGE_SIZE = 10

function formatDate(value) {
  return value ? new Date(value).toLocaleDateString() : '—'
}

function formatValue(value) {
  return value == null || value === '' ? '—' : value
}

function DispatcherParcelListPage({ unassignedOnly = false }) {
  const [searchParams, setSearchParams] = useSearchParams()
  const [search, setSearch] = useState('')
  const [currentPage, setCurrentPage] = useState(1)
  const [selectedParcel, setSelectedParcel] = useState(null)
  const statusFilter = searchParams.get('status') ?? ''

  const loadParcels = useCallback(
    (signal) => (unassignedOnly
      ? getDispatcherUnassignedParcels(signal)
      : getParcels(signal)),
    [unassignedOnly],
  )
  const parcels = useDashboardData(loadParcels)
  const records = useMemo(
    () => (Array.isArray(parcels.data) ? parcels.data : []),
    [parcels.data],
  )

  const statusOptions = useMemo(() => (
    [...new Set(records.map((parcel) => parcel.currentStatus).filter(Boolean))]
      .sort((left, right) => left.localeCompare(right))
      .map((status) => ({ value: status, label: status }))
  ), [records])

  const filteredParcels = useMemo(() => {
    const query = search.trim().toLowerCase()
    return records.filter((parcel) => {
      const matchesStatus = !statusFilter || parcel.currentStatus === statusFilter
      const searchable = [
        parcel.trackingNumber,
        parcel.senderName,
        parcel.currentStatus,
        parcel.routeName,
      ].filter(Boolean).join(' ').toLowerCase()

      return matchesStatus && (!query || searchable.includes(query))
    })
  }, [records, search, statusFilter])

  const pageCount = Math.ceil(filteredParcels.length / PAGE_SIZE)
  const pageRows = filteredParcels.slice((currentPage - 1) * PAGE_SIZE, currentPage * PAGE_SIZE)

  function updateSearch(value) {
    setSearch(value)
    setCurrentPage(1)
  }

  function updateStatus(event) {
    const nextSearchParams = new URLSearchParams(searchParams)
    if (event.target.value) nextSearchParams.set('status', event.target.value)
    else nextSearchParams.delete('status')
    setSearchParams(nextSearchParams, { replace: true })
    setCurrentPage(1)
  }

  const title = unassignedOnly ? 'Unassigned Parcels' : 'Parcels'

  return (
    <div className="role-dashboard role-dashboard--dispatcher">
      <PageHeader
        actions={(
          <RefreshButton isLoading={parcels.isLoading} onRefresh={parcels.refresh} />
        )}
        description={unassignedOnly
          ? 'Active parcels without a current courier assignment. Courier availability is checked again by the assignment API.'
          : 'Search and filter active parcels using the parcel records returned by the backend.'}
        eyebrow="Parcel movement"
        title={title}
      />

      {parcels.isLoading && !parcels.data && (
        <div className="dispatcher-inline-loading" role="status">Loading parcels…</div>
      )}
      {parcels.error && <ErrorMessage error={parcels.error} onRetry={parcels.refresh} />}

      {parcels.data && !parcels.error && (
        <>
          <div className="dispatcher-list-toolbar">
            <SearchBar
              label={`Search ${title.toLowerCase()}`}
              onChange={updateSearch}
              placeholder="Tracking number, sender, status, or route"
              value={search}
            />
            <FormSelect
              id="dispatcher-parcel-status"
              label="Filter by status"
              onChange={updateStatus}
              options={statusOptions}
              placeholder="All statuses"
              value={statusFilter}
            />
          </div>

          <section className="dispatcher-list-card">
            <DataTable
              caption={title}
              columns={[
                { key: 'trackingNumber', header: 'Tracking number' },
                { key: 'senderName', header: 'Sender' },
                { key: 'currentStatus', header: 'Status', render: (row) => <StatusBadge status={row.currentStatus} /> },
                { key: 'routeName', header: 'Route', render: (row) => formatValue(row.routeName) },
                { key: 'bookingDate', header: 'Booked', render: (row) => formatDate(row.bookingDate) },
                { key: 'totalWeightKg', header: 'Weight', render: (row) => `${row.totalWeightKg} kg` },
              ]}
              emptyState={(
                <EmptyState
                  description={search || statusFilter
                    ? 'Adjust the search or status filter and try again.'
                    : `There are no ${title.toLowerCase()} to display.`}
                  title={`No ${title.toLowerCase()} found`}
                  action={unassignedOnly && (
                    <Link className="ui-button ui-button--primary" to="/dispatch/assign">
                      <PackagePlus aria-hidden="true" size={16} />
                      Assign a parcel
                    </Link>
                  )}
                />
              )}
              getRowKey={(row) => row.parcelId}
              rowActions={(row) => (
                <div className="dispatcher-row-actions">
                  <Button
                    aria-label={`View parcel ${row.trackingNumber}`}
                    onClick={() => setSelectedParcel(row)}
                    title="View parcel details"
                    variant="outline"
                  >
                    <Eye aria-hidden="true" size={16} />
                  </Button>
                  {unassignedOnly && (
                    <Link
                      aria-label={`Assign courier to ${row.trackingNumber}`}
                      className="ui-button ui-button--outline"
                      title="Assign courier"
                      to={`/dispatch/assign?parcelId=${row.parcelId}`}
                    >
                      <PackagePlus aria-hidden="true" size={16} />
                    </Link>
                  )}
                </div>
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
        </>
      )}

      {selectedParcel && (
        <div className="dispatcher-dialog-backdrop">
          <section
            aria-labelledby="dispatcher-parcel-details-title"
            aria-modal="true"
            className="dispatcher-dialog"
            role="dialog"
          >
            <header className="dispatcher-dialog__header">
              <div>
                <p className="dispatcher-dialog__eyebrow">Parcel details</p>
                <h2 className="dispatcher-dialog__title" id="dispatcher-parcel-details-title">
                  {selectedParcel.trackingNumber}
                </h2>
              </div>
              <Button onClick={() => setSelectedParcel(null)} variant="outline">Close</Button>
            </header>
            <dl className="dispatcher-details-grid">
              <div><dt>Parcel ID</dt><dd>{selectedParcel.parcelId}</dd></div>
              <div><dt>Sender</dt><dd>{selectedParcel.senderName}</dd></div>
              <div><dt>Status</dt><dd>{selectedParcel.currentStatus}</dd></div>
              <div><dt>Route</dt><dd>{formatValue(selectedParcel.routeName)}</dd></div>
              <div><dt>Weight</dt><dd>{selectedParcel.totalWeightKg} kg</dd></div>
              <div><dt>Booked</dt><dd>{formatDate(selectedParcel.bookingDate)}</dd></div>
              <div><dt>Expected delivery</dt><dd>{formatDate(selectedParcel.expectedDeliveryDate)}</dd></div>
              <div><dt>Declared value</dt><dd>{formatValue(selectedParcel.declaredValue)}</dd></div>
              <div className="dispatcher-details-grid__full"><dt>Instructions</dt><dd>{formatValue(selectedParcel.specialInstructions)}</dd></div>
            </dl>
          </section>
        </div>
      )}
    </div>
  )
}

export function DispatcherParcelsPage() {
  return <DispatcherParcelListPage />
}

export function DispatcherUnassignedParcelsPage() {
  return <DispatcherParcelListPage unassignedOnly />
}
