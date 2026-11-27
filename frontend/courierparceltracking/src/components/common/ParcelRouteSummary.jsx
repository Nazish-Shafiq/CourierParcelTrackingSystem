import '../../style/components/parcel-route-summary.css'

function ParcelRouteSummary({ data, className = '' }) {
  if (!data) return null

  const routeLocations = [data.originHubName, data.destinationHubName]
    .filter(Boolean)
    .join(' → ')
  const routeName = data.routeName || routeLocations || '—'
  const currentStatus = data.currentStatusName || data.currentStatus || '—'

  return (
    <dl className={`parcel-route-summary ${className}`.trim()}>
      <div>
        <dt>Route</dt>
        <dd>{routeName}</dd>
      </div>
      <div>
        <dt>Origin hub</dt>
        <dd>{data.originHubName || '—'}</dd>
      </div>
      <div>
        <dt>Destination hub</dt>
        <dd>{data.destinationHubName || '—'}</dd>
      </div>
      <div>
        <dt>Current hub</dt>
        <dd>{data.currentHubName || '—'}</dd>
      </div>
      <div>
        <dt>Current status</dt>
        <dd>{currentStatus}</dd>
      </div>
    </dl>
  )
}

export default ParcelRouteSummary
