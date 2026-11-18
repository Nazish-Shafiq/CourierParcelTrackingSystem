import { useCallback, useMemo, useState } from 'react'
import Button from '../../components/common/Button'
import EmptyState from '../../components/common/EmptyState'
import ErrorMessage from '../../components/common/ErrorMessage'
import LoadingSpinner from '../../components/common/LoadingSpinner'
import PageHeader from '../../components/common/PageHeader'
import RefreshButton from '../../components/common/RefreshButton'
import SearchBar from '../../components/common/SearchBar'
import StatusBadge from '../../components/common/StatusBadge'
import FormSelect from '../../components/forms/FormSelect'
import FormTextArea from '../../components/forms/FormTextArea'
import DataTable from '../../components/tables/DataTable'
import Pagination from '../../components/tables/Pagination'
import { useDashboardData } from '../../hooks/useDashboardData'
import {
  confirmHubTransferArrival,
  createHubTransfer,
  getHubTransferOptions,
  getHubTransfers,
} from '../../services/hubTransferService'

const PAGE_SIZE = 10

function formatDate(value) {
  return value ? new Date(value).toLocaleString() : '—'
}

function getErrorMessage(error) {
  return error?.response?.data?.message
    || error?.message
    || 'The hub transfer request could not be completed.'
}

export default function DispatcherHubTransfersPage() {
  const [search, setSearch] = useState('')
  const [status, setStatus] = useState('')
  const [currentPage, setCurrentPage] = useState(1)
  const [selectedTransfer, setSelectedTransfer] = useState(null)
  const [isCreateOpen, setIsCreateOpen] = useState(false)
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [actionError, setActionError] = useState('')
  const [successMessage, setSuccessMessage] = useState('')

  const loadTransfers = useCallback((signal) => getHubTransfers({
    page: currentPage,
    pageSize: PAGE_SIZE,
    search: search.trim() || undefined,
    status: status || undefined,
    signal,
  }), [currentPage, search, status])

  const transfers = useDashboardData(loadTransfers)
  const rows = transfers.data?.items ?? []

  const loadOptions = useCallback((signal) => getHubTransferOptions(signal), [])
  const options = useDashboardData(loadOptions)

  async function handleCreate(request) {
    setActionError('')
    setSuccessMessage('')
    setIsSubmitting(true)
    try {
      await createHubTransfer(request)
      setIsCreateOpen(false)
      setSuccessMessage('Hub transfer created and parcel marked IN_TRANSIT.')
      transfers.refresh()
      options.refresh()
    } catch (error) {
      setActionError(getErrorMessage(error))
    } finally {
      setIsSubmitting(false)
    }
  }

  async function handleConfirmArrival(transferId, notes) {
    setActionError('')
    setSuccessMessage('')
    setIsSubmitting(true)
    try {
      await confirmHubTransferArrival(transferId, notes ? { notes } : {})
      setSelectedTransfer(null)
      setSuccessMessage('Parcel arrival confirmed at the destination hub.')
      transfers.refresh()
      options.refresh()
    } catch (error) {
      setActionError(getErrorMessage(error))
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <div className="role-dashboard role-dashboard--dispatcher">
      <PageHeader
        actions={
          <>
            <Button onClick={() => setIsCreateOpen(true)} variant="primary">Create transfer</Button>
            <RefreshButton isLoading={transfers.isLoading} onRefresh={transfers.refresh} />
          </>
        }
        description="Create hub transfers and confirm parcel arrival at destination hubs."
        eyebrow="Parcel movement"
        title="Hub Transfers"
      />

      {successMessage && <p className="dispatcher-success" role="status">{successMessage}</p>}
      {actionError && <p className="dispatcher-form-error" role="alert">{actionError}</p>}

      <div className="dispatcher-list-toolbar">
        <SearchBar
          label="Search hub transfers"
          onChange={(value) => { setSearch(value); setCurrentPage(1) }}
          placeholder="Tracking number or hub name"
          value={search}
        />
        <FormSelect
          id="hub-transfer-status"
          label="Transfer status"
          onChange={(event) => { setStatus(event.target.value); setCurrentPage(1) }}
          options={[
            { value: 'IN_TRANSIT', label: 'In transit' },
            { value: 'ARRIVED', label: 'Arrived' },
            { value: 'CANCELLED', label: 'Cancelled' },
          ]}
          placeholder="All statuses"
          value={status}
        />
      </div>

      {transfers.isLoading && !transfers.data && (
        <LoadingSpinner label="Loading hub transfers" />
      )}
      {transfers.error && <ErrorMessage error={transfers.error} onRetry={transfers.refresh} />}

      {transfers.data && !transfers.error && (
        <section className="dispatcher-list-card">
          <DataTable
            caption="Hub transfer history"
            columns={[
              { key: 'trackingNumber', header: 'Tracking number' },
              { key: 'sourceHubName', header: 'Source hub' },
              { key: 'destinationHubName', header: 'Destination hub' },
              { key: 'status', header: 'Status', render: (row) => <StatusBadge status={row.status.replaceAll('_', ' ')} /> },
              { key: 'transferredAt', header: 'Transferred', render: (row) => formatDate(row.transferredAt) },
              { key: 'arrivedAt', header: 'Arrived', render: (row) => formatDate(row.arrivedAt) },
            ]}
            emptyState={
              <EmptyState
                action={
                  <Button onClick={() => setIsCreateOpen(true)} variant="primary">Create transfer</Button>
                }
                description="No hub transfers match the current search or status."
                title="No hub transfers found"
              />
            }
            getRowKey={(row) => row.hubTransferId}
            rowActions={(row) => (
              <div className="dispatcher-row-actions">
                <Button onClick={() => setSelectedTransfer(row)} variant="outline">Details</Button>
                {row.status === 'IN_TRANSIT' && (
                  <Button onClick={() => setSelectedTransfer(row)} variant="primary">Confirm arrival</Button>
                )}
              </div>
            )}
            rows={rows}
          />
          <Pagination
            currentPage={transfers.data.page}
            onPageChange={setCurrentPage}
            pageCount={transfers.data.totalPages}
            pageSize={transfers.data.pageSize}
            totalRecords={transfers.data.totalRecords}
          />
        </section>
      )}

      {isCreateOpen && (
        <CreateHubTransferDialog
          isSubmitting={isSubmitting}
          options={options.data}
          optionsError={options.error}
          onClose={() => !isSubmitting && setIsCreateOpen(false)}
          onSubmit={handleCreate}
        />
      )}

      {selectedTransfer && (
        <HubTransferDetailsDialog
          isSubmitting={isSubmitting}
          transfer={selectedTransfer}
          onClose={() => !isSubmitting && setSelectedTransfer(null)}
          onConfirm={handleConfirmArrival}
        />
      )}
    </div>
  )
}

function CreateHubTransferDialog({ isSubmitting, options, optionsError, onClose, onSubmit }) {
  const [parcelId, setParcelId] = useState('')
  const [destinationHubId, setDestinationHubId] = useState('')
  const [notes, setNotes] = useState('')
  const [validationError, setValidationError] = useState('')
  const selectedParcel = useMemo(
    () => options?.parcels?.find((parcel) => String(parcel.parcelId) === parcelId),
    [options, parcelId],
  )

  function handleSubmit(event) {
    event.preventDefault()
    setValidationError('')
    if (!parcelId || !destinationHubId) {
      setValidationError('Select an eligible parcel and destination hub.')
      return
    }
    if (selectedParcel && String(selectedParcel.currentHubId) === destinationHubId) {
      setValidationError('The destination hub must be different from the parcel current hub.')
      return
    }
    onSubmit({
      parcelId: Number(parcelId),
      destinationHubId: Number(destinationHubId),
      ...(notes.trim() ? { notes: notes.trim() } : {}),
    })
  }

  return (
    <div className="dispatcher-dialog-backdrop">
      <section aria-modal="true" className="dispatcher-dialog" role="dialog">
        <header className="dispatcher-dialog__header">
          <div>
            <p className="dispatcher-dialog__eyebrow">New movement</p>
            <h2 className="dispatcher-dialog__title">Create Hub Transfer</h2>
          </div>
          <Button disabled={isSubmitting} onClick={onClose} variant="outline">Close</Button>
        </header>
        {optionsError && <ErrorMessage error={optionsError} />}
        {validationError && <p className="dispatcher-form-error" role="alert">{validationError}</p>}
        {options && (
          <form className="dispatcher-form" onSubmit={handleSubmit}>
            <FormSelect
              id="hub-transfer-parcel"
              label="Eligible parcel"
              onChange={(event) => setParcelId(event.target.value)}
              options={(options.parcels ?? []).map((parcel) => ({
                value: String(parcel.parcelId),
                label: `${parcel.trackingNumber} — ${parcel.currentHubName} (${parcel.currentStatus})`,
              }))}
              placeholder="Select a parcel"
              required
              value={parcelId}
            />
            {selectedParcel && (
              <p className="dispatcher-muted">
                Current hub: <strong>{selectedParcel.currentHubName}</strong>
              </p>
            )}
            <FormSelect
              id="hub-transfer-destination"
              label="Destination hub"
              onChange={(event) => setDestinationHubId(event.target.value)}
              options={(options.hubs ?? []).map((hub) => ({
                value: String(hub.hubId),
                label: `${hub.hubName}${hub.city ? ` — ${hub.city}` : ''}`,
              }))}
              placeholder="Select destination hub"
              required
              value={destinationHubId}
            />
            <FormTextArea
              id="hub-transfer-notes"
              label="Transfer notes"
              maxLength={500}
              onChange={(event) => setNotes(event.target.value)}
              rows={3}
              value={notes}
            />
            <div className="dispatcher-form__actions">
              <Button disabled={isSubmitting} onClick={onClose} variant="outline">Cancel</Button>
              <Button isLoading={isSubmitting} type="submit">Create transfer</Button>
            </div>
          </form>
        )}
      </section>
    </div>
  )
}

function HubTransferDetailsDialog({ isSubmitting, transfer, onClose, onConfirm }) {
  const [notes, setNotes] = useState('')
  return (
    <div className="dispatcher-dialog-backdrop">
      <section aria-modal="true" className="dispatcher-dialog dispatcher-dialog--compact" role="dialog">
        <header className="dispatcher-dialog__header">
          <div>
            <p className="dispatcher-dialog__eyebrow">Transfer details</p>
            <h2 className="dispatcher-dialog__title">{transfer.trackingNumber}</h2>
          </div>
          <Button disabled={isSubmitting} onClick={onClose} variant="outline">Close</Button>
        </header>
        <dl className="dispatcher-details-grid">
          <div><dt>Source hub</dt><dd>{transfer.sourceHubName}</dd></div>
          <div><dt>Destination hub</dt><dd>{transfer.destinationHubName}</dd></div>
          <div><dt>Status</dt><dd>{transfer.status.replaceAll('_', ' ')}</dd></div>
          <div><dt>Transferred</dt><dd>{formatDate(transfer.transferredAt)}</dd></div>
          <div><dt>Arrived</dt><dd>{formatDate(transfer.arrivedAt)}</dd></div>
          <div className="dispatcher-details-grid__full"><dt>Notes</dt><dd>{transfer.notes || '—'}</dd></div>
        </dl>
        {transfer.status === 'IN_TRANSIT' && (
          <div className="dispatcher-form" style={{ marginTop: '1rem' }}>
            <FormTextArea
              id="hub-transfer-arrival-notes"
              label="Arrival notes"
              maxLength={500}
              onChange={(event) => setNotes(event.target.value)}
              rows={3}
              value={notes}
            />
            <div className="dispatcher-form__actions">
              <Button disabled={isSubmitting} onClick={onClose} variant="outline">Cancel</Button>
              <Button
                isLoading={isSubmitting}
                onClick={() => onConfirm(transfer.hubTransferId, notes)}
              >
                Confirm arrival
              </Button>
            </div>
          </div>
        )}
      </section>
    </div>
  )
}
