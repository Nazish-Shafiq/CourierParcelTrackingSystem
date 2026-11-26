import { useCallback, useEffect, useMemo, useRef, useState } from 'react'
import { useSearchParams } from 'react-router-dom'
import Button from '../../components/common/Button'
import DashboardStatCard from '../../components/cards/DashboardStatCard'
import EmptyState from '../../components/common/EmptyState'
import ErrorMessage from '../../components/common/ErrorMessage'
import FormInput from '../../components/forms/FormInput'
import FormSelect from '../../components/forms/FormSelect'
import PageHeader from '../../components/common/PageHeader'
import RefreshButton from '../../components/common/RefreshButton'
import SearchBar from '../../components/common/SearchBar'
import StatusBadge from '../../components/common/StatusBadge'
import DataTable from '../../components/tables/DataTable'
import Pagination from '../../components/tables/Pagination'
import {
  getCourierPerformanceReport,
  getDeliveryReport,
  getHubParcelReport,
  getParcelBookingsReport,
  getParcelStatusReport,
  getUserReport,
} from '../../services/reportService'
import '../../style/admin/admin-management.css'

const PAGE_SIZE = 10

function formatDate(value) {
  return value ? new Date(value).toLocaleDateString() : '—'
}

function formatDateTime(value) {
  return value ? new Date(value).toLocaleString() : '—'
}

function getStatusCount(statuses, code) {
  return statuses.find((item) => item.statusCode === code)?.totalParcels ?? 0
}

function mapParcels({ fromDate, toDate, signal }) {
  return Promise.all([
    getParcelBookingsReport({ fromDate, toDate, signal }),
    getParcelStatusReport({ fromDate, toDate, signal }),
  ]).then(([rows, statuses]) => ({
    rows,
    metrics: [
      { label: 'Total parcels', value: statuses.reduce((total, item) => total + item.totalParcels, 0) },
      { label: 'Booked', value: getStatusCount(statuses, 'BOOKED') },
      { label: 'In transit', value: getStatusCount(statuses, 'IN_TRANSIT') },
      { label: 'Delivered', value: getStatusCount(statuses, 'DELIVERED') },
      { label: 'Returned', value: getStatusCount(statuses, 'RETURNED') },
      { label: 'Cancelled', value: getStatusCount(statuses, 'CANCELLED') },
      { label: 'On hold', value: getStatusCount(statuses, 'ON_HOLD') },
    ],
  }))
}

const REPORTS = {
  parcels: {
    label: 'Parcel report',
    load: mapParcels,
    columns: [
      { key: 'trackingNumber', header: 'Tracking number' },
      { key: 'senderName', header: 'Sender' },
      { key: 'recipientName', header: 'Recipient' },
      { key: 'recipientCity', header: 'Recipient city' },
      { key: 'currentStatus', header: 'Status', render: (row) => <StatusBadge status={row.currentStatus} /> },
      { key: 'bookingDate', header: 'Booked', render: (row) => formatDate(row.bookingDate), exportValue: (row) => formatDate(row.bookingDate) },
      { key: 'expectedDeliveryDate', header: 'Expected delivery', render: (row) => formatDate(row.expectedDeliveryDate), exportValue: (row) => formatDate(row.expectedDeliveryDate) },
      { key: 'totalWeightKg', header: 'Weight (kg)' },
    ],
    getRowKey: (row) => row.parcelId,
    filter: {
      label: 'Current status',
      getValue: (row) => row.currentStatus,
    },
    summary: 'Parcel records and current-status totals for the selected booking date range.',
    dateFiltered: true,
  },
  couriers: {
    label: 'Courier report',
    load: async (params) => {
      const rows = await getCourierPerformanceReport(params)
      return {
        rows,
        metrics: [
          { label: 'Couriers', value: rows.length },
          { label: 'Current assignments', value: rows.reduce((total, row) => total + row.activeAssignments, 0) },
          { label: 'Assigned parcels', value: rows.reduce((total, row) => total + row.totalAssignedParcels, 0) },
          { label: 'Delivered parcels', value: rows.reduce((total, row) => total + row.deliveredParcels, 0) },
        ],
      }
    },
    columns: [
      { key: 'courierName', header: 'Courier' },
      { key: 'employeeCode', header: 'Employee code' },
      { key: 'email', header: 'Email' },
      { key: 'phone', header: 'Phone' },
      { key: 'hubName', header: 'Assigned hub', render: (row) => row.hubName || 'Not Assigned' },
      { key: 'vehicleType', header: 'Vehicle type' },
      { key: 'vehicleNumber', header: 'Vehicle number' },
      { key: 'isAvailable', header: 'Availability', render: (row) => (row.isAvailable ? 'Available' : 'Unavailable'), exportValue: (row) => (row.isAvailable ? 'Available' : 'Unavailable') },
      { key: 'isActive', header: 'Account status', render: (row) => (row.isActive ? 'Active' : 'Inactive'), exportValue: (row) => (row.isActive ? 'Active' : 'Inactive') },
      { key: 'activeAssignments', header: 'Current workload' },
      { key: 'totalAssignedParcels', header: 'Assigned in date range' },
      { key: 'deliveredParcels', header: 'Delivered in date range' },
    ],
    getRowKey: (row) => row.courierId,
    filter: {
      label: 'Availability',
      getValue: (row) => (row.isAvailable ? 'Available' : 'Unavailable'),
    },
    summary: 'Courier contact, assigned hub, availability, account status, and workload from database records.',
    dateFiltered: true,
  },
  deliveries: {
    label: 'Delivery report',
    load: async (params) => {
      const report = await getDeliveryReport(params)
      return {
        rows: report.events,
        metrics: [
          { label: 'Successful deliveries', value: report.successfulDeliveries },
          { label: 'Failed delivery attempts', value: report.failedDeliveryAttempts },
        ],
      }
    },
    columns: [
      { key: 'trackingNumber', header: 'Tracking number' },
      { key: 'statusName', header: 'Event', render: (row) => <StatusBadge status={row.statusName} /> },
      { key: 'courierName', header: 'Courier', render: (row) => row.courierName || '—' },
      { key: 'hubName', header: 'Hub', render: (row) => row.hubName || '—' },
      { key: 'eventAt', header: 'Timestamp', render: (row) => formatDateTime(row.eventAt), exportValue: (row) => formatDateTime(row.eventAt) },
    ],
    getRowKey: (row) => row.statusEventId,
    filter: {
      label: 'Delivery event',
      getValue: (row) => row.statusName,
    },
    summary: 'Successful deliveries and failed delivery attempts from chronological status-event records.',
    dateFiltered: true,
  },
  hubs: {
    label: 'Hub report',
    load: async (params) => {
      const rows = await getHubParcelReport(params)
      return {
        rows,
        metrics: [
          { label: 'Hubs', value: rows.length },
          { label: 'Origin parcel records', value: rows.reduce((total, row) => total + row.originParcelCount, 0) },
          { label: 'Destination parcel records', value: rows.reduce((total, row) => total + row.destinationParcelCount, 0) },
        ],
      }
    },
    columns: [
      { key: 'hubName', header: 'Hub' },
      { key: 'hubCode', header: 'Hub code' },
      { key: 'city', header: 'City' },
      { key: 'province', header: 'Province' },
      { key: 'isActive', header: 'Status', render: (row) => (row.isActive ? 'Active' : 'Inactive'), exportValue: (row) => (row.isActive ? 'Active' : 'Inactive') },
      { key: 'originParcelCount', header: 'Origin parcels' },
      { key: 'destinationParcelCount', header: 'Destination parcels' },
    ],
    getRowKey: (row) => row.hubId,
    filter: {
      label: 'Hub status',
      getValue: (row) => (row.isActive ? 'Active' : 'Inactive'),
    },
    summary: 'Active and inactive, non-deleted hubs with parcel counts grouped by route origin and destination.',
    dateFiltered: true,
  },
  users: {
    label: 'User report',
    load: async (params) => {
      const report = await getUserReport(params.signal)
      return {
        rows: report.roles,
        metrics: [
          { label: 'Total users', value: report.totalUsers },
          { label: 'Active users', value: report.activeUsers },
          { label: 'Inactive users', value: report.inactiveUsers },
          { label: 'Roles', value: report.roles.length },
        ],
      }
    },
    columns: [
      { key: 'roleName', header: 'Role' },
      { key: 'totalUsers', header: 'Total users' },
      { key: 'activeUsers', header: 'Active users' },
      { key: 'inactiveUsers', header: 'Inactive users' },
    ],
    getRowKey: (row) => row.roleName,
    filter: {
      label: 'Role',
      getValue: (row) => row.roleName,
    },
    summary: 'Non-deleted user totals grouped by database role and active status. No authentication data is returned.',
    dateFiltered: false,
  },
}

function escapeCsv(value) {
  const text = value == null ? '' : String(value)
  const safeText = /^[\t\r\n ]*[=+\-@]/.test(text) ? `'${text}` : text
  return `"${safeText.replaceAll('"', '""')}"`
}

function downloadCsv(report, rows) {
  const headers = report.columns.map((column) => column.header)
  const csvRows = rows.map((row) => report.columns.map((column) => (
    escapeCsv(column.exportValue ? column.exportValue(row) : row[column.key])
  )))
  const content = [headers, ...csvRows].map((row) => row.join(',')).join('\r\n')
  const url = URL.createObjectURL(new Blob([content], { type: 'text/csv;charset=utf-8' }))
  const link = document.createElement('a')
  link.href = url
  link.download = `${report.label.toLowerCase().replaceAll(' ', '-')}.csv`
  link.click()
  window.setTimeout(() => URL.revokeObjectURL(url), 0)
}

function AdminReportsPage() {
  const [searchParams] = useSearchParams()
  const requestedReport = searchParams.get('report')
  const [reportKey, setReportKey] = useState(() => (
    REPORTS[requestedReport] ? requestedReport : 'parcels'
  ))
  const [fromDate, setFromDate] = useState('')
  const [toDate, setToDate] = useState('')
  const [reportRows, setReportRows] = useState([])
  const [metrics, setMetrics] = useState([])
  const [hasRun, setHasRun] = useState(false)
  const [isLoading, setIsLoading] = useState(false)
  const [error, setError] = useState(null)
  const [search, setSearch] = useState('')
  const [filterValue, setFilterValue] = useState('')
  const [currentPage, setCurrentPage] = useState(1)
  const controllerRef = useRef(null)
  const shouldAutoRunRef = useRef(Boolean(REPORTS[requestedReport]))
  const report = REPORTS[reportKey]

  useEffect(() => () => controllerRef.current?.abort(), [])

  const filterOptions = useMemo(() => {
    if (!report.filter) return []
    return [...new Set(reportRows.map(report.filter.getValue).filter(Boolean))]
      .sort((left, right) => String(left).localeCompare(String(right)))
      .map((value) => ({ label: value, value }))
  }, [report, reportRows])

  const filteredRows = useMemo(() => {
    const query = search.trim().toLowerCase()
    return reportRows.filter((row) => {
      const matchesFilter = !filterValue || report.filter?.getValue(row) === filterValue
      const matchesSearch = !query || report.columns.some((column) => {
        const value = column.exportValue ? column.exportValue(row) : row[column.key]
        return value != null && String(value).toLowerCase().includes(query)
      })
      return matchesFilter && matchesSearch
    })
  }, [filterValue, report, reportRows, search])

  const pageRows = filteredRows.slice((currentPage - 1) * PAGE_SIZE, currentPage * PAGE_SIZE)

  const loadReport = useCallback(async () => {
    if (report.dateFiltered && fromDate && toDate && fromDate > toDate) {
      controllerRef.current?.abort()
      setIsLoading(false)
      setError(new Error('From date must be on or before the to date.'))
      return
    }

    controllerRef.current?.abort()
    const controller = new AbortController()
    controllerRef.current = controller
    setIsLoading(true)
    setError(null)
    setHasRun(true)
    setSearch('')
    setFilterValue('')
    setCurrentPage(1)

    try {
      const result = await report.load({
        fromDate: report.dateFiltered ? fromDate || undefined : undefined,
        toDate: report.dateFiltered ? toDate || undefined : undefined,
        signal: controller.signal,
      })
      if (!result || !Array.isArray(result.rows) || !Array.isArray(result.metrics)) {
        throw new Error('The report response did not contain valid report data.')
      }
      setReportRows(result.rows)
      setMetrics(result.metrics)
    } catch (requestError) {
      if (!controller.signal.aborted) setError(requestError)
    } finally {
      if (!controller.signal.aborted) setIsLoading(false)
    }
  }, [fromDate, report, toDate])

  useEffect(() => {
    if (!shouldAutoRunRef.current) return
    shouldAutoRunRef.current = false
    void loadReport()
  }, [loadReport])

  function resetReport() {
    controllerRef.current?.abort()
    setIsLoading(false)
    setHasRun(false)
    setReportRows([])
    setMetrics([])
    setError(null)
    setSearch('')
    setFilterValue('')
    setCurrentPage(1)
  }

  return (
    <div className="role-dashboard admin-management">
      <PageHeader
        actions={<RefreshButton disabled={!hasRun} isLoading={isLoading} onRefresh={loadReport} />}
        description="Read-only reports with live database totals, date ranges, search, filtering, and CSV export."
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
          disabled={!report.dateFiltered}
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
          disabled={!report.dateFiltered}
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
          <>
            {metrics.length > 0 && (
              <div className="dashboard-stat-grid">
                {metrics.map((metric) => (
                  <DashboardStatCard
                    key={metric.label}
                    label={metric.label}
                    value={metric.value}
                  />
                ))}
              </div>
            )}
            <div className="admin-list-toolbar">
              <SearchBar
                label={`Search ${report.label.toLowerCase()}`}
                onChange={(value) => {
                  setSearch(value)
                  setCurrentPage(1)
                }}
                placeholder={`Search ${report.label.toLowerCase()}…`}
                value={search}
              />
              {report.filter && (
                <FormSelect
                  id="admin-report-filter"
                  label={report.filter.label}
                  onChange={(event) => {
                    setFilterValue(event.target.value)
                    setCurrentPage(1)
                  }}
                  options={filterOptions}
                  placeholder={`All ${report.filter.label.toLowerCase()}s`}
                  value={filterValue}
                />
              )}
              <Button
                disabled={!filteredRows.length}
                onClick={() => downloadCsv(report, filteredRows)}
                type="button"
                variant="outline"
              >
                Export CSV
              </Button>
            </div>
            <DataTable
              caption={report.label}
              columns={report.columns}
              emptyState={(
                <EmptyState
                  description="No records were returned for the selected search and filters."
                  title="No report data"
                />
              )}
              getRowKey={report.getRowKey}
              isLoading={isLoading}
              rows={pageRows}
            />
            <Pagination
              currentPage={currentPage}
              onPageChange={setCurrentPage}
              pageCount={Math.ceil(filteredRows.length / PAGE_SIZE)}
              pageSize={PAGE_SIZE}
              totalRecords={filteredRows.length}
            />
          </>
        )}
        {!hasRun && !error && (
          <EmptyState
            description="Choose a report and optional date range, then run it to retrieve current database records."
            title="Ready to run a report"
          />
        )}
      </section>
    </div>
  )
}

export default AdminReportsPage
