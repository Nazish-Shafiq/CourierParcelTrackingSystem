import '../../style/components/components.css'

const STATUS_TONES = {
  BOOKED: 'info',
  DELIVERED: 'success',
  IN_TRANSIT: 'info',
}

function StatusBadge({ status, tone }) {
  const label = status == null ? 'Unknown' : String(status)
  const normalized = label.trim().toUpperCase().replace(/[\s-]+/g, '_')
  const statusTone = tone || STATUS_TONES[normalized] || 'neutral'

  return (
    <span className={`ui-status-badge ui-status-badge--${statusTone}`}>
      <span aria-hidden="true" className="ui-status-badge__dot" />
      {label}
    </span>
  )
}

export default StatusBadge
