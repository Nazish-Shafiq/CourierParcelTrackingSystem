import '../../style/components/components.css'

const STATUS_TONES = {
  BOOKED: 'info',
  PICKED_UP: 'info',
  AT_ORIGIN_HUB: 'neutral',
  AT_DESTINATION_HUB: 'info',
  OUT_FOR_DELIVERY: 'warning',
  DELIVERY_ATTEMPTED: 'danger',
  DELIVERED: 'success',
  IN_TRANSIT: 'info',
  ON_HOLD: 'warning',
  RETURNED: 'neutral',
  CANCELLED: 'danger',
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
