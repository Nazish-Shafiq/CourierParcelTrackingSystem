import '../../style/components/tracking-event-details.css'

function TrackingEventDetails({ event }) {
  const statusCode = event.statusCode?.toUpperCase()
  const routeLocations = [event.originHubName, event.destinationHubName]
    .filter(Boolean)
    .join(' → ')
  const route = event.routeName
    ? routeLocations
      ? `${event.routeName} · ${routeLocations}`
      : event.routeName
    : routeLocations
  const showRoute = ['PICKED_UP', 'IN_TRANSIT', 'OUT_FOR_DELIVERY']
    .includes(statusCode)
  const deliveryAddress = ['OUT_FOR_DELIVERY', 'DELIVERED'].includes(statusCode)
    ? event.deliveryAddress
    : null

  if (!event.courierName && !event.hubName && !(showRoute && route) && !deliveryAddress)
    return null

  return (
    <div className="tracking-event-details">
      {event.courierName && <p><strong>Courier:</strong> {event.courierName}</p>}
      {event.hubName && <p><strong>Hub:</strong> {event.hubName}</p>}
      {showRoute && route && (
        <p>
          <strong>{statusCode === 'IN_TRANSIT' ? 'Current movement' : 'Route'}:</strong>{' '}
          {route}
        </p>
      )}
      {deliveryAddress && (
        <p>
          <strong>{statusCode === 'DELIVERED' ? 'Delivered at' : 'Destination'}:</strong>{' '}
          {deliveryAddress}
        </p>
      )}
    </div>
  )
}

export default TrackingEventDetails
