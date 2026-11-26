
import { useCallback, useEffect, useState } from 'react'
import { Link, useSearchParams } from 'react-router-dom'
import Button from '../../components/common/Button'
import EmptyState from '../../components/common/EmptyState'
import ErrorMessage from '../../components/common/ErrorMessage'
import PageHeader from '../../components/common/PageHeader'
import RefreshButton from '../../components/common/RefreshButton'
import FormSelect from '../../components/forms/FormSelect'
import FormTextArea from '../../components/forms/FormTextArea'
import DataTable from '../../components/tables/DataTable'
import Pagination from '../../components/tables/Pagination'
import { useDashboardData } from '../../hooks/useDashboardData'
import {
  assignParcel,
  getParcelAssignments,
  reassignParcel,
  unassignParcel,
} from '../../services/parcelAssignmentService'
import {
  getDispatcherAssignmentOptions,
  getDispatcherAvailableCouriers,
} from '../../services/dispatcherService'
import '../../style/dispatcher/dispatcher-pages.css'

const PAGE_SIZE = 10

function formatDate(value) {
  return value ? new Date(value).toLocaleString() : '—'
}

function getErrorMessage(error) {
  return error?.response?.data?.message
    || error?.message
    || 'The request could not be completed.'
}

// ============================================================
// DISPATCHER ASSIGN PARCEL PAGE
// ============================================================

function DispatcherAssignParcelPage() {
  // Keep URL search parameters editable so the old parcelId
  // can be removed after a successful assignment.
  const [searchParams, setSearchParams] = useSearchParams()

  const [parcelId, setParcelId] = useState(
    () => searchParams.get('parcelId') ?? ''
  )
  const [courierId, setCourierId] = useState('')
  const [assignmentNotes, setAssignmentNotes] = useState('')
  const [submitError, setSubmitError] = useState('')
  const [successMessage, setSuccessMessage] = useState('')
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [courierRefreshKey, setCourierRefreshKey] = useState(0)
  const [courierLookup, setCourierLookup] = useState({
    parcelId: '',
    refreshKey: 0,
    couriers: [],
    isLoading: false,
    error: null,
  })

  const loadOptions = useCallback(
    (signal) => getDispatcherAssignmentOptions(signal),
    [],
  )

  const options = useDashboardData(loadOptions)

  const parcelOptions = options.data?.parcels ?? []
  const currentCourierLookup = courierLookup.parcelId === parcelId
    && courierLookup.refreshKey === courierRefreshKey
    ? courierLookup
    : { couriers: [], isLoading: Boolean(parcelId), error: null }
  const courierOptions = currentCourierLookup.couriers

  useEffect(() => {
    const controller = new AbortController()
    const selectedParcelId = parcelId
    const selectedRefreshKey = courierRefreshKey
    const lookup = selectedParcelId
      ? getDispatcherAvailableCouriers(Number(selectedParcelId), controller.signal)
      : Promise.resolve([])

    lookup
      .then((couriers) => {
        if (!controller.signal.aborted) {
          setCourierLookup({
            parcelId: selectedParcelId,
            refreshKey: selectedRefreshKey,
            couriers,
            isLoading: false,
            error: null,
          })
        }
      })
      .catch((error) => {
        if (!controller.signal.aborted) {
          setCourierLookup({
            parcelId: selectedParcelId,
            refreshKey: selectedRefreshKey,
            couriers: [],
            isLoading: false,
            error,
          })
        }
      })

    return () => controller.abort()
  }, [parcelId, courierRefreshKey])

  const requestedParcelId = searchParams.get('parcelId') ?? ''

  // Show this warning only when a parcel was requested through
  // the URL but is no longer available in the unassigned list.
  const requestedParcelUnavailable = Boolean(
    requestedParcelId
    && options.data
    && !parcelOptions.some(
      (parcel) => String(parcel.parcelId) === requestedParcelId
    ),
  )

  // ----------------------------------------------------------
  // Assign parcel to Courier
  // ----------------------------------------------------------

  async function handleSubmit(event) {
    event.preventDefault()

    if (isSubmitting) return

    setSubmitError('')
    setSuccessMessage('')

    const parcel = parcelOptions.find(
      (item) => String(item.parcelId) === parcelId
    )

    const courier = courierOptions.find(
      (item) => String(item.courierId) === courierId
    )

    if (!parcel || !courier) {
      setSubmitError(
        'Select an available parcel and courier before assigning.'
      )
      return
    }

    const request = {
      parcelId: parcel.parcelId,
      courierId: courier.courierId,
    }

    if (assignmentNotes.trim()) {
      request.assignmentNotes = assignmentNotes.trim()
    }

    setIsSubmitting(true)

    try {
      // Send assignment request to the backend.
      await assignParcel(request)

      // Display successful assignment message.
      setSuccessMessage(
        `Courier ${courier.fullName} was assigned to ${parcel.trackingNumber}.`
      )

      // Clear previous error messages.
      setSubmitError('')

      // Clear selected form values.
      setParcelId('')
      setCourierId('')
      setAssignmentNotes('')

      // Remove the old parcelId from the URL.
      // Otherwise, after refreshing the parcel list, the page
      // incorrectly reports that the successfully assigned
      // parcel is no longer available.
      const updatedParams = new URLSearchParams(searchParams)
      updatedParams.delete('parcelId')

      setSearchParams(updatedParams, { replace: true })

      // Refresh unassigned parcels and available Couriers.
      options.refresh()

    } catch (error) {
      setSuccessMessage('')
      setSubmitError(getErrorMessage(error))

    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <div className="role-dashboard role-dashboard--dispatcher">

      <PageHeader
        actions={
          <RefreshButton
            isLoading={options.isLoading || currentCourierLookup.isLoading}
            onRefresh={() => {
              options.refresh()
              setCourierRefreshKey((key) => key + 1)
            }}
          />
        }
        description="Assign an unassigned parcel to a courier currently reported as active and available."
        eyebrow="Parcel movement"
        title="Assign Courier"
      />

      {options.isLoading && !options.data && (
        <div className="dispatcher-inline-loading" role="status">
          Loading assignment options…
        </div>
      )}

      {options.error && (
        <ErrorMessage
          error={options.error}
          onRetry={options.refresh}
        />
      )}

      {successMessage && (
        <p className="dispatcher-success" role="status">
          {successMessage}
        </p>
      )}

      {submitError && (
        <p className="dispatcher-form-error" role="alert">
          {submitError}
        </p>
      )}

      {options.data && !options.error && (
        <section className="dispatcher-form-card">

          {requestedParcelUnavailable && (
            <p className="dispatcher-form-error" role="status">
              The requested parcel is no longer available for assignment.
              Select another parcel.
            </p>
          )}

          {parcelOptions.length === 0 && (
            <p className="dispatcher-inline-empty">
              There are no unassigned parcels available right now.
            </p>
          )}

          {parcelId && currentCourierLookup.isLoading && (
            <div className="dispatcher-inline-loading" role="status">
              Loading couriers assigned to the parcel's operational hub…
            </div>
          )}

          {parcelId && currentCourierLookup.error && (
            <ErrorMessage
              error={currentCourierLookup.error}
              onRetry={() => setCourierRefreshKey((key) => key + 1)}
            />
          )}

          {parcelId &&
            !currentCourierLookup.isLoading &&
            !currentCourierLookup.error &&
            courierOptions.length === 0 && (
            <p className="dispatcher-inline-empty">
              No active, available couriers are assigned to this parcel's
              operational hub.
            </p>
          )}

          {parcelOptions.length > 0 ? (
            <form
              className="dispatcher-form"
              onSubmit={handleSubmit}
            >

              <FormSelect
                id="dispatcher-assign-parcel"
                label="Unassigned parcel"
                onChange={(event) => {
                  setParcelId(event.target.value)
                  setCourierId('')
                  setSubmitError('')
                  setSuccessMessage('')
                }}
                options={parcelOptions.map((parcel) => ({
                  value: String(parcel.parcelId),
                  label: `${parcel.trackingNumber} — ${parcel.senderName}`,
                }))}
                placeholder="Select a parcel"
                required
                value={parcelId}
              />

              <FormSelect
                id="dispatcher-assign-courier"
                label="Available courier"
                onChange={(event) => {
                  setCourierId(event.target.value)
                  setSubmitError('')
                  setSuccessMessage('')
                }}
                disabled={!parcelId || currentCourierLookup.isLoading || courierOptions.length === 0}
                options={courierOptions.map((courier) => ({
                  value: String(courier.courierId),
                  label: `${courier.fullName} (${courier.employeeCode}) — ${courier.hubName}`,
                }))}
                placeholder={parcelId ? 'Select a courier' : 'Select a parcel first'}
                required
                value={courierId}
              />

              <FormTextArea
                id="dispatcher-assignment-notes"
                label="Assignment notes"
                onChange={(event) => setAssignmentNotes(event.target.value)}
                placeholder="Optional notes for this assignment"
                rows={3}
                value={assignmentNotes}
              />

              <div className="dispatcher-form__actions">

                <Button
                  disabled={
                    isSubmitting ||
                    options.isLoading ||
                    currentCourierLookup.isLoading ||
                    !courierOptions.length
                  }
                  isLoading={isSubmitting}
                  type="submit"
                >
                  Assign courier
                </Button>

                <Link
                  className="ui-button ui-button--outline"
                  to="/dispatch/unassigned-parcels"
                >
                  View unassigned parcels
                </Link>

              </div>
            </form>
          ) : (
            <div className="dispatcher-form__empty-action">
              <Link
                className="ui-button ui-button--outline"
                to="/dispatch/unassigned-parcels"
              >
                Review unassigned parcels
              </Link>
            </div>
          )}

        </section>
      )}

    </div>
  )
}

// ============================================================
// REASSIGN COURIER DIALOG
// ============================================================

function ReassignDialog({ assignment, onClose, onSubmit }) {
  const [courierId, setCourierId] = useState('')
  const [assignmentNotes, setAssignmentNotes] = useState('')
  const [submitError, setSubmitError] = useState('')
  const [isSubmitting, setIsSubmitting] = useState(false)

  const loadCouriers = useCallback(
    (signal) => getDispatcherAvailableCouriers(assignment.parcelId, signal),
    [assignment.parcelId],
  )

  const couriers = useDashboardData(loadCouriers)

  async function handleSubmit(event) {
    event.preventDefault()

    setSubmitError('')

    const courier = couriers.data?.find(
      (item) => String(item.courierId) === courierId
    )

    if (!courier) {
      setSubmitError(
        'Select an available courier before reassigning.'
      )
      return
    }

    const request = {
      newCourierId: courier.courierId,
    }

    if (assignmentNotes.trim()) {
      request.assignmentNotes = assignmentNotes.trim()
    }

    setIsSubmitting(true)

    try {
      await onSubmit(assignment.parcelAssignmentId, request)

    } catch (error) {
      setSubmitError(getErrorMessage(error))

    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <div className="dispatcher-dialog-backdrop">

      <section
        aria-labelledby="dispatcher-reassign-title"
        aria-modal="true"
        className="dispatcher-dialog"
        role="dialog"
      >

        <header className="dispatcher-dialog__header">

          <div>
            <p className="dispatcher-dialog__eyebrow">
              Change courier
            </p>

            <h2
              className="dispatcher-dialog__title"
              id="dispatcher-reassign-title"
            >
              {assignment.trackingNumber}
            </h2>
          </div>

          <Button onClick={onClose} variant="outline">
            Close
          </Button>

        </header>

        {couriers.isLoading && !couriers.data && (
          <div className="dispatcher-inline-loading" role="status">
            Loading available couriers…
          </div>
        )}

        {couriers.error && (
          <ErrorMessage
            error={couriers.error}
            onRetry={couriers.refresh}
          />
        )}

        {submitError && (
          <p className="dispatcher-form-error" role="alert">
            {submitError}
          </p>
        )}

        {couriers.data && !couriers.error && (
          couriers.data.length > 0 ? (

            <form
              className="dispatcher-form"
              onSubmit={handleSubmit}
            >

              <FormSelect
                id="dispatcher-reassign-courier"
                label="New courier"
                onChange={(event) => setCourierId(event.target.value)}
                options={couriers.data.map((courier) => ({
                  value: String(courier.courierId),
                  label: `${courier.fullName} (${courier.employeeCode}) — ${courier.hubName}`,
                }))}
                placeholder="Select a courier"
                required
                value={courierId}
              />

              <FormTextArea
                id="dispatcher-reassign-notes"
                label="Assignment notes"
                onChange={(event) => setAssignmentNotes(event.target.value)}
                placeholder="Optional notes for this assignment"
                rows={3}
                value={assignmentNotes}
              />

              <div className="dispatcher-form__actions">

                <Button
                  disabled={isSubmitting}
                  isLoading={isSubmitting}
                  type="submit"
                >
                  Reassign courier
                </Button>

                <Button
                  disabled={isSubmitting}
                  onClick={onClose}
                  variant="outline"
                >
                  Cancel
                </Button>

              </div>

            </form>

          ) : (
            <EmptyState
              description="No other active, available couriers can take this assignment."
              title="No couriers available"
            />
          )
        )}

      </section>

    </div>
  )
}

// ============================================================
// DISPATCHER ASSIGNMENTS PAGE
// ============================================================

function DispatcherAssignmentsPage() {
  const [searchParams] = useSearchParams()
  const activeOnly = searchParams.get('active') === 'true'
  const [currentPage, setCurrentPage] = useState(1)
  const [selectedAssignment, setSelectedAssignment] = useState(null)
  const [pendingUnassign, setPendingUnassign] = useState(null)
  const [actionError, setActionError] = useState('')
  const [successMessage, setSuccessMessage] = useState('')
  const [isUnassigning, setIsUnassigning] = useState(false)

  const loadAssignments = useCallback(
    (signal) => getParcelAssignments({
      page: currentPage,
      pageSize: PAGE_SIZE,
      activeOnly,
      signal,
    }),
    [activeOnly, currentPage],
  )

  const assignments = useDashboardData(loadAssignments)
  const rows = assignments.data?.items ?? []

  async function handleReassign(id, request) {
    setActionError('')
    await reassignParcel(id, request)

    setSelectedAssignment(null)
    setSuccessMessage('The parcel assignment was updated.')
    assignments.refresh()
  }

  async function handleUnassign() {
    if (!pendingUnassign) return

    setActionError('')
    setIsUnassigning(true)

    try {
      await unassignParcel(pendingUnassign.parcelAssignmentId)

      setPendingUnassign(null)

      setSuccessMessage(
        `${pendingUnassign.trackingNumber} was unassigned.`
      )

      assignments.refresh()

    } catch (error) {
      setActionError(getErrorMessage(error))

    } finally {
      setIsUnassigning(false)
    }
  }

  const isActiveAssignment = (assignment) =>
    assignment.unassignedAt == null

  return (
    <div className="role-dashboard role-dashboard--dispatcher">

      <PageHeader
        actions={
          <>
            <Link
              className="ui-button ui-button--primary"
              to="/dispatch/assign"
            >
              Assign courier
            </Link>

            <RefreshButton
              isLoading={assignments.isLoading}
              onRefresh={assignments.refresh}
            />
          </>
        }
        description="Review assignment history and manage current courier assignments."
        eyebrow="Parcel movement"
        title="Parcel Assignments"
      />

      {assignments.isLoading && !assignments.data && (
        <div className="dispatcher-inline-loading" role="status">
          Loading assignments…
        </div>
      )}

      {assignments.error && (
        <ErrorMessage
          error={assignments.error}
          onRetry={assignments.refresh}
        />
      )}

      {actionError && (
        <p className="dispatcher-form-error" role="alert">
          {actionError}
        </p>
      )}

      {successMessage && (
        <p className="dispatcher-success" role="status">
          {successMessage}
        </p>
      )}

      {assignments.data && !assignments.error && (
        <section className="dispatcher-list-card">

          <DataTable
            caption="Parcel assignments"
            columns={[
              {
                key: 'trackingNumber',
                header: 'Tracking number',
              },
              {
                key: 'courierName',
                header: 'Courier',
              },
              {
                key: 'employeeCode',
                header: 'Employee code',
              },
              {
                key: 'assignedAt',
                header: 'Assigned',
                render: (row) => formatDate(row.assignedAt),
              },
              {
                key: 'unassignedAt',
                header: 'Unassigned',
                render: (row) => formatDate(row.unassignedAt),
              },
              {
                key: 'status',
                header: 'Status',
                render: (row) =>
                  isActiveAssignment(row) ? 'Active' : 'Ended',
              },
            ]}
            emptyState={
              <EmptyState
                action={
                  <Link
                    className="ui-button ui-button--primary"
                    to="/dispatch/assign"
                  >
                    Assign a parcel
                  </Link>
                }
                description="New courier assignments will appear here."
                title="No assignments found"
              />
            }
            getRowKey={(row) => row.parcelAssignmentId}
            rowActions={(row) => (
              isActiveAssignment(row) ? (

                <div className="dispatcher-row-actions">

                  <Button
                    onClick={() => setSelectedAssignment(row)}
                    variant="outline"
                  >
                    Reassign
                  </Button>

                  <Button
                    onClick={() => {
                      setActionError('')
                      setPendingUnassign(row)
                    }}
                    variant="danger"
                  >
                    Unassign
                  </Button>

                </div>

              ) : (
                <span className="dispatcher-muted">
                  Completed
                </span>
              )
            )}
            rows={rows}
          />

          <Pagination
            currentPage={currentPage}
            onPageChange={setCurrentPage}
            pageCount={assignments.data.totalPages}
            pageSize={assignments.data.pageSize}
            totalRecords={assignments.data.totalRecords}
          />

        </section>
      )}

      {selectedAssignment && (
        <ReassignDialog
          assignment={selectedAssignment}
          onClose={() => setSelectedAssignment(null)}
          onSubmit={handleReassign}
        />
      )}

      {pendingUnassign && (
        <div className="dispatcher-dialog-backdrop">

          <section
            aria-labelledby="dispatcher-unassign-title"
            aria-modal="true"
            className="dispatcher-dialog dispatcher-dialog--compact"
            role="dialog"
          >

            <p className="dispatcher-dialog__eyebrow">
              End assignment
            </p>

            <h2
              className="dispatcher-dialog__title"
              id="dispatcher-unassign-title"
            >
              Unassign {pendingUnassign.trackingNumber}?
            </h2>

            <p className="dispatcher-dialog__description">
              The active assignment will end and the courier can be considered for another parcel.
            </p>

            <div className="dispatcher-form__actions">

              <Button
                isLoading={isUnassigning}
                onClick={handleUnassign}
                variant="danger"
              >
                Confirm unassignment
              </Button>

              <Button
                disabled={isUnassigning}
                onClick={() => setPendingUnassign(null)}
                variant="outline"
              >
                Cancel
              </Button>

            </div>

          </section>

        </div>
      )}

    </div>
  )
}

export {
  DispatcherAssignParcelPage,
  DispatcherAssignmentsPage,
}
