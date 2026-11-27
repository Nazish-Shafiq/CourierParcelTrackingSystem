import Button from '../common/Button'
import '../../style/admin/admin-management.css'

function formatJson(value) {
  if (!value) return 'None recorded'
  try {
    return JSON.stringify(JSON.parse(value), null, 2)
  } catch {
    return value
  }
}

function AuditLogDetailsModal({ log, onClose }) {
  const actor = log.actorFullName
    || log.actorUserName
    || (log.actorUserId ? `User ${log.actorUserId}` : 'System / unauthenticated')

  return (
    <div className="admin-dialog-backdrop">
      <section
        aria-labelledby="audit-log-details-title"
        aria-modal="true"
        className="admin-dialog admin-dialog--details"
        role="dialog"
      >
        <header className="admin-dialog__header">
          <div>
            <p className="admin-dialog__eyebrow">Audit Log {log.auditLogId}</p>
            <h2 className="admin-dialog__title" id="audit-log-details-title">
              {log.action} — {log.entityName} {log.entityId}
            </h2>
          </div>
          <Button onClick={onClose} variant="outline">Close</Button>
        </header>
        <dl className="admin-details">
          <div className="admin-details__item">
            <dt>Actor/User</dt>
            <dd>{actor}</dd>
          </div>
          <div className="admin-details__item">
            <dt>Occurred At</dt>
            <dd>{new Date(log.occurredAt).toLocaleString()}</dd>
          </div>
          <div className="admin-details__item">
            <dt>Action</dt>
            <dd>{log.action}</dd>
          </div>
          <div className="admin-details__item">
            <dt>Entity</dt>
            <dd>{log.entityName} — {log.entityId}</dd>
          </div>
        </dl>
        <div className="admin-audit-json-grid">
          <section>
            <h3>Old Values</h3>
            <pre>{formatJson(log.oldValues)}</pre>
          </section>
          <section>
            <h3>New Values</h3>
            <pre>{formatJson(log.newValues)}</pre>
          </section>
        </div>
      </section>
    </div>
  )
}

export default AuditLogDetailsModal
