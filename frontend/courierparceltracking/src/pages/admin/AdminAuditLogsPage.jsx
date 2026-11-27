import { useEffect, useState } from 'react'
import { Eye, RefreshCw } from 'lucide-react'
import Button from '../../components/common/Button'
import EmptyState from '../../components/common/EmptyState'
import ErrorMessage from '../../components/common/ErrorMessage'
import FormInput from '../../components/forms/FormInput'
import FormSelect from '../../components/forms/FormSelect'
import PageHeader from '../../components/common/PageHeader'
import DataTable from '../../components/tables/DataTable'
import Pagination from '../../components/tables/Pagination'
import { getAuditLogs } from '../../services/auditLogService'
import AuditLogDetailsModal from '../../components/admin/AuditLogDetailsModal'
import '../../style/admin/admin-management.css'

const PAGE_SIZE_OPTIONS = [10, 25, 50, 100].map((value) => ({
  label: String(value),
  value,
}))

const ACTION_OPTIONS = [
  'ASSIGN',
  'CREATE',
  'DELETE',
  'HUB_ARRIVAL',
  'HUB_DEPARTURE',
  'HUB_TRANSFER',
  'LOGIN',
  'LOGIN_FAILED',
  'LOGOUT',
  'REASSIGN',
  'REVOKE',
  'ROLE_ASSIGN',
  'ROLE_REMOVE',
  'ROUTE_ASSIGN',
  'STATUS_CHANGE',
  'UNASSIGN',
  'UPDATE',
].map((value) => ({ label: value.replaceAll('_', ' '), value }))

function formatDateTime(value) {
  if (!value) return '—'
  const date = new Date(value)
  return Number.isNaN(date.getTime()) ? '—' : date.toLocaleString()
}

function actorName(log) {
  return log.actorFullName
    || log.actorUserName
    || (log.actorUserId ? `User ${log.actorUserId}` : 'System / unauthenticated')
}

const columns = [
  { key: 'auditLogId', header: 'Audit Log ID' },
  { key: 'actor', header: 'Actor/User', render: actorName },
  { key: 'action', header: 'Action' },
  { key: 'entityName', header: 'Entity Name' },
  { key: 'entityId', header: 'Entity ID' },
  { key: 'occurredAt', header: 'Occurred At', render: (log) => formatDateTime(log.occurredAt) },
  { key: 'ipAddress', header: 'IP Address', render: (log) => log.ipAddress || '—' },
]

function dateTimeStart(value) {
  return value ? `${value}T00:00:00.000Z` : undefined
}

function dateTimeEnd(value) {
  return value ? `${value}T23:59:59.999Z` : undefined
}

function AdminAuditLogsPage() {
  const [pageData, setPageData] = useState(null)
  const [selectedLog, setSelectedLog] = useState(null)
  const [entityName, setEntityName] = useState('')
  const [entityId, setEntityId] = useState('')
  const [action, setAction] = useState('')
  const [actorUserId, setActorUserId] = useState('')
  const [fromDate, setFromDate] = useState('')
  const [toDate, setToDate] = useState('')
  const [page, setPage] = useState(1)
  const [pageSize, setPageSize] = useState(25)
  const [refreshVersion, setRefreshVersion] = useState(0)
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState(null)

  useEffect(() => {
    const controller = new AbortController()
    const query = {
      page,
      pageSize,
      entityName: entityName.trim() || undefined,
      entityId: entityId.trim() || undefined,
      action: action || undefined,
      actorUserId: actorUserId ? Number(actorUserId) : undefined,
      fromDate: dateTimeStart(fromDate),
      toDate: dateTimeEnd(toDate),
    }

    getAuditLogs(query, controller.signal)
      .then((result) => {
        if (!result || !Array.isArray(result.items)) {
          throw new Error('The server returned an invalid audit log response.')
        }
        setError(null)
        setPageData(result)
      })
      .catch((requestError) => {
        if (!controller.signal.aborted) setError(requestError)
      })
      .finally(() => {
        if (!controller.signal.aborted) setIsLoading(false)
      })

    return () => controller.abort()
  }, [
    action,
    actorUserId,
    entityId,
    entityName,
    fromDate,
    page,
    pageSize,
    refreshVersion,
    toDate,
  ])

  function resetPage(update) {
    setIsLoading(true)
    setError(null)
    update()
    setPage(1)
  }

  function refresh() {
    setIsLoading(true)
    setError(null)
    setRefreshVersion((version) => version + 1)
  }

  return (
    <div className="role-dashboard admin-management">
      <PageHeader
        actions={(
          <Button
            disabled={isLoading}
            isLoading={isLoading}
            onClick={refresh}
            variant="outline"
          >
            <RefreshCw aria-hidden="true" size={16} />
            Refresh
          </Button>
        )}
        description="Review administrative and business changes recorded by the system."
        eyebrow="Administration"
        title="Audit Logs"
      />

      <section aria-label="Audit log filters" className="admin-audit-filters">
        <FormInput
          id="audit-entity-name"
          label="Entity name"
          onChange={(event) => resetPage(() => setEntityName(event.target.value))}
          placeholder="Search entity"
          value={entityName}
        />
        <FormInput
          id="audit-entity-id"
          label="Entity ID"
          onChange={(event) => resetPage(() => setEntityId(event.target.value))}
          placeholder="Search entity ID"
          value={entityId}
        />
        <FormSelect
          id="audit-action"
          label="Action"
          onChange={(event) => resetPage(() => setAction(event.target.value))}
          options={ACTION_OPTIONS}
          placeholder="All actions"
          value={action}
        />
        <FormInput
          id="audit-actor-user-id"
          label="Actor user ID"
          min="1"
          onChange={(event) => resetPage(() => setActorUserId(event.target.value))}
          type="number"
          value={actorUserId}
        />
        <FormInput
          id="audit-from-date"
          label="From date"
          onChange={(event) => resetPage(() => setFromDate(event.target.value))}
          type="date"
          value={fromDate}
        />
        <FormInput
          id="audit-to-date"
          label="To date"
          onChange={(event) => resetPage(() => setToDate(event.target.value))}
          type="date"
          value={toDate}
        />
        <FormSelect
          id="audit-page-size"
          label="Rows per page"
          onChange={(event) => {
            setIsLoading(true)
            setPageSize(Number(event.target.value))
            setPage(1)
          }}
          options={PAGE_SIZE_OPTIONS}
          value={pageSize}
        />
      </section>

      {error && <ErrorMessage error={error} onRetry={refresh} />}
      {!error && (
        <>
          <DataTable
            caption="Audit log records"
            columns={columns}
            emptyState={(
              <EmptyState
                description="No audit records match the selected filters."
                title="No audit logs found"
              />
            )}
            getRowKey={(log) => log.auditLogId}
            isLoading={isLoading}
            rowActions={(log) => (
              <Button
                aria-label={`View details for audit log ${log.auditLogId}`}
                className="admin-row-action"
                onClick={() => setSelectedLog(log)}
                title="View details"
                variant="outline"
              >
                <Eye aria-hidden="true" size={16} />
              </Button>
            )}
            rows={pageData?.items ?? []}
          />
          <Pagination
            currentPage={pageData?.page ?? page}
            onPageChange={(nextPage) => {
              setIsLoading(true)
              setPage(nextPage)
            }}
            pageCount={pageData?.totalPages ?? 0}
            pageSize={pageData?.pageSize ?? pageSize}
            totalRecords={pageData?.totalRecords ?? 0}
          />
        </>
      )}

      {selectedLog && (
        <AuditLogDetailsModal
          log={selectedLog}
          onClose={() => setSelectedLog(null)}
        />
      )}
    </div>
  )
}

export default AdminAuditLogsPage
