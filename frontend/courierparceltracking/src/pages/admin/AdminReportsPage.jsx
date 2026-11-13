import { useEffect, useRef, useState } from 'react'
import Button from '../../components/common/Button'
import EmptyState from '../../components/common/EmptyState'
import ErrorMessage from '../../components/common/ErrorMessage'
import FormInput from '../../components/forms/FormInput'
import FormSelect from '../../components/forms/FormSelect'
import DataTable from '../../components/tables/DataTable'
import PageHeader from '../../components/common/PageHeader'
import RefreshButton from '../../components/common/RefreshButton'
import StatusBadge from '../../components/common/StatusBadge'
import {
  getCourierPerformanceReport,
  getParcelBookingsReport,
  getParcelStatusReport,
} from '../../services/reportService'
import '../../style/admin/admin-management.css'

function formatDate(value) {
  return value ? new Date(value).toLocaleDateString() : '—'
}

const REPORTS = {
  bookings: {
    label: 'Parcel bookings',
    load: getParcelBookingsReport,
    columns: [
      { key: 'trackingNumber', header: 'Tracking number' },
      { key: 'senderName', header: 'Sender' },
      { key: 'recipientName', header: 'Recipient' },
      { key: 'recipientCity', header: 'Recipient city' },
      { key: 'currentStatus', header: 'Status', render: (row) => <StatusBadge status={row.currentStatus} /> },
      { key: 'bookingDate', header: 'Booked', render: (row) => formatDate(row.bookingDate) },
      { key: 'expectedDeliveryDate', header: 'Expected delivery', render: (row) => formatDate(row.expectedDeliveryDate) },
      { key: 'totalWeightKg', header: 'Weight (kg)' },
    ],
    getRowKey: (row) => row.parcelId,
    summary: 'Parcel booking and recipient details returned for the selected date range.',
  },
  statuses: {
    label: 'Parcel statuses',
    load: getParcelStatusReport,
    columns: [
      { key: 'statusCode', header: 'Status code' },
      { key: 'statusName', header: 'Status name' },
      { key: 'totalParcels', header: 'Parcels' },
    ],
    getRowKey: (row) => row.parcelStatusId,
    summary: 'Parcel totals grouped by the backend status records.',
  },
  couriers: {
    label: 'Courier performance',
    load: getCourierPerformanceReport,
    columns: [
      { key: 'employeeCode', header: 'Employee code' },
      { key: 'courierName', header: 'Courier' },
      { key: 'totalAssignedParcels', header: 'Assigned parcels' },
      { key: 'deliveredParcels', header: 'Delivered parcels' },
    ],
    getRowKey: (row) => row.courierId,
    summary: 'Assigned and delivered parcel totals for each courier.',
  },
}

function AdminReportsPage() {
  const [reportKey, setReportKey] = useState('bookings')
  const [fromDate, setFromDate] = useState('')
  const [toDate, setToDate] = useState('')
  const [reportRows, setReportRows] = useState([])
  const [hasRun, setHasRun] = useState(false)
  const [isLoading, setIsLoading] = useState(false)
  const [error, setError] = useState(null)
  const controllerRef = useRef(null)
  const report = REPORTS[reportKey]

  useEffect(() => () => controllerRef.current?.abort(), [])

  async function loadReport() {
    if (fromDate && toDate && fromDate > toDate) {
      controllerRef.current?.abort()
      setIsLoading(false)
      setError(new Error('From date must be on or before the to date.'))
      return
    }

    if (controllerRef.current) controllerRef.current.abort()
    const controller = new AbortController()
    controllerRef.current = controller
    setIsLoading(true)
    setError(null)
    setHasRun(true)

    try {
      const rows = await report.load({
        fromDate: fromDate || undefined,
        toDate: toDate || undefined,
        signal: controller.signal,
      })
      if (!Array.isArray(rows)) throw new Error('The report response was not a record list.')
      setReportRows(rows)
    } catch (requestError) {
      if (!controller.signal.aborted) setError(requestError)
    } finally {
      if (!controller.signal.aborted) setIsLoading(false)
    }
  }

  function resetReport() {
    controllerRef.current?.abort()
    setIsLoading(false)
    setHasRun(false)
    setReportRows([])
    setError(null)
  }

  return (
    <div className="role-dashboard admin-management">
      <PageHeader
        actions={<RefreshButton disabled={!hasRun} isLoading={isLoading} onRefresh={loadReport} />}
        description="Run supported backend reports with optional date filters."
        eyebrow="Administration"
        title="Reports"
      />

      <form
        className="admin-report-controls"
        onSubmit={(event) => {
          event.preventDefault()
          loadReport()
        }}
      >
        <FormSelect
          id="admin-report-type"
          label="Report"
          onChange={(event) => {
            resetReport()
            setReportKey(event.target.value)
          }}
          options={Object.entries(REPORTS).map(([value, item]) => ({
            label: item.label,
            value,
          }))}
          value={reportKey}
        />
        <FormInput
          id="admin-report-from"
          label="From date"
          onChange={(event) => {
            resetReport()
            setFromDate(event.target.value)
          }}
          type="date"
          value={fromDate}
        />
        <FormInput
          id="admin-report-to"
          label="To date"
          onChange={(event) => {
            resetReport()
            setToDate(event.target.value)
          }}
          type="date"
          value={toDate}
        />
        <Button disabled={isLoading} isLoading={isLoading} type="submit">
          Run report
        </Button>
      </form>

      <section className="admin-report-section">
        <p className="admin-report-summary">{report.summary}</p>
        {error && <ErrorMessage error={error} onRetry={loadReport} />}
        {hasRun && !error && (
          <DataTable
            caption={report.label}
            columns={report.columns}
            emptyState={(
              <EmptyState
                description="No records were returned for the selected filters."
                title="No report data"
              />
            )}
            getRowKey={report.getRowKey}
            isLoading={isLoading}
            rows={reportRows}
          />
        )}
        {!hasRun && !error && (
          <EmptyState
            description="Choose a report and optional date range, then run it to retrieve backend data."
            title="Ready to run a report"
          />
        )}
      </section>
    </div>
  )
}

export default AdminReportsPage
