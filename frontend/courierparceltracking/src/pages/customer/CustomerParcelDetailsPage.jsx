import { useCallback } from 'react'
import { Link, useParams } from 'react-router-dom'
import EmptyState from '../../components/common/EmptyState'
import ErrorMessage from '../../components/common/ErrorMessage'
import LoadingSpinner from '../../components/common/LoadingSpinner'
import TrackingEventDetails from '../../components/common/TrackingEventDetails'
import PageHeader from '../../components/common/PageHeader'
import RefreshButton from '../../components/common/RefreshButton'
import StatusBadge from '../../components/common/StatusBadge'
import { useDashboardData } from '../../hooks/useDashboardData'
import { getCustomerParcelDetails } from '../../services/customerDashboardService'
import '../../style/customer/customer-dashboard.css'

function formatDate(value, includeTime = false) {
  if (!value) return '—'
  const date = new Date(value)
  return includeTime ? date.toLocaleString() : date.toLocaleDateString()
}

function formatAddress(contact) {
  return [
    contact?.addressLine,
    contact?.city,
    contact?.province,
    contact?.postalCode,
  ].filter(Boolean).join(', ') || '—'
}

function getTrackingDescription(event, parcel) {
  const statusCode = event.statusCode?.toUpperCase()
  const originHub = event.originHubName || parcel.originHubName
  const destinationHub = event.destinationHubName || parcel.destinationHubName
  const eventHub = event.hubName
  const deliveryAddress = event.deliveryAddress || [
    parcel.recipient?.addressLine,
    parcel.recipient?.city,
    parcel.recipient?.province,
    parcel.recipient?.postalCode,
  ].filter(Boolean).join(', ')

  switch (statusCode) {
    case 'BOOKED':
      return originHub && destinationHub
        ? `Parcel booked for delivery from ${originHub} to ${destinationHub}.`
        : originHub
          ? `Parcel booking created at ${originHub}.`
        : 'Parcel booking created.'
    case 'AT_ORIGIN_HUB': {
      const hub = originHub || eventHub
      return hub ? `Parcel has arrived at ${hub}.` : 'Parcel is at the origin hub.'
    }
    case 'PICKED_UP': {
      const hub = originHub || eventHub
      return hub ? `Parcel collected from ${hub}.` : 'Parcel collected.'
    }
    case 'IN_TRANSIT':
      return originHub && destinationHub
        ? `Current movement: ${originHub} to ${destinationHub}.`
        : 'Parcel is in transit.'
    case 'AT_DESTINATION_HUB': {
      const hub = destinationHub || eventHub
      return hub
        ? `Parcel has arrived at ${hub}.`
        : 'Parcel has arrived at the destination hub.'
    }
    case 'OUT_FOR_DELIVERY': {
      const location = deliveryAddress || destinationHub || eventHub
      return location
        ? `Parcel is out for delivery to ${location}.`
        : 'Parcel is out for delivery.'
    }
    case 'DELIVERY_ATTEMPTED': {
      const hub = eventHub || destinationHub
      return hub
        ? `Delivery was attempted at ${hub}.`
        : 'A delivery attempt was made.'
    }
    case 'DELIVERED':
      return deliveryAddress
        ? `Parcel successfully delivered to ${deliveryAddress}.`
        : 'Parcel successfully delivered.'
    case 'ON_HOLD':
      return 'Parcel movement is on hold.'
    case 'RETURNED':
      return 'Parcel has been returned.'
    case 'CANCELLED':
      return 'Parcel delivery was cancelled.'
    default:
      return event.description || event.statusName || 'Tracking update recorded.'
  }
}

function ContactCard({ contact, title }) {
  if (!contact) return null

  return (
    <section className="customer-dashboard__detail-card">
      <h3>{title}</h3>
      <dl>
        <div><dt>Name</dt><dd>{contact.name || '—'}</dd></div>
        <div><dt>Phone</dt><dd>{contact.phone || '—'}</dd></div>
        <div><dt>Email</dt><dd>{contact.email || '—'}</dd></div>
        <div><dt>Address</dt><dd>{formatAddress(contact)}</dd></div>
      </dl>
    </section>
  )
}

function CustomerParcelDetailsPage() {
  const { parcelId } = useParams()
  const loadDetails = useCallback(
    (signal) => getCustomerParcelDetails(parcelId, signal),
    [parcelId],
  )
  const details = useDashboardData(loadDetails)
  const parcel = details.data

  return (
    <div className="role-dashboard role-dashboard--customer">
      <PageHeader
        actions={<RefreshButton isLoading={details.isLoading} onRefresh={details.refresh} />}
        breadcrumbs={[
          { label: 'Customer Dashboard', to: '/dashboard/customer' },
          { label: 'Parcel Details' },
        ]}
        description={parcel?.trackingNumber || 'Parcel tracking details'}
        eyebrow="My Parcels"
        title="Parcel Details"
      />

      <Link className="customer-dashboard__back-link" to="/dashboard/customer">
        Back to My Parcels
      </Link>

      {details.isLoading && !parcel && <LoadingSpinner label="Loading parcel details" />}
      {details.error && <ErrorMessage error={details.error} onRetry={details.refresh} />}

      {parcel && !details.error && (
        <>
          <section className="customer-dashboard__detail-card customer-dashboard__overview">
            <div className="customer-dashboard__overview-heading">
              <div>
                <p className="customer-dashboard__eyebrow">Tracking Number</p>
                <h3>{parcel.trackingNumber}</h3>
              </div>
              <StatusBadge status={parcel.currentStatusName} />
            </div>
            <dl className="customer-dashboard__facts">
              <div><dt>Booking Date</dt><dd>{formatDate(parcel.bookingDate)}</dd></div>
              <div><dt>Expected Delivery</dt><dd>{formatDate(parcel.expectedDeliveryDate)}</dd></div>
              <div><dt>Delivered At</dt><dd>{formatDate(parcel.deliveredAt, true)}</dd></div>
              <div><dt>Total Weight</dt><dd>{parcel.totalWeightKg} kg</dd></div>
              {parcel.declaredValue != null && (
                <div><dt>Declared Value</dt><dd>{parcel.declaredValue}</dd></div>
              )}
              <div><dt>Origin Hub</dt><dd>{parcel.originHubName || 'Not Available'}</dd></div>
              <div><dt>Destination Hub</dt><dd>{parcel.destinationHubName || 'Not Available'}</dd></div>
              <div><dt>Route</dt><dd>{parcel.routeName || 'Not Available'}</dd></div>
              <div><dt>Current Hub</dt><dd>{parcel.currentHubName || '—'}</dd></div>
              {parcel.currentStatusDescription && (
                <div className="customer-dashboard__fact-wide">
                  <dt>Status Description</dt>
                  <dd>{parcel.currentStatusDescription}</dd>
                </div>
              )}
              {parcel.specialInstructions && (
                <div className="customer-dashboard__fact-wide">
                  <dt>Special Instructions</dt>
                  <dd>{parcel.specialInstructions}</dd>
                </div>
              )}
            </dl>
          </section>

          <div className="customer-dashboard__contacts">
            <ContactCard contact={parcel.sender} title="Sender" />
            <ContactCard contact={parcel.recipient} title="Receiver" />
          </div>

          <section className="customer-dashboard__detail-card">
            <h3>Tracking History</h3>
            {parcel.trackingHistory?.length ? (
              <ol className="customer-dashboard__timeline">
                {parcel.trackingHistory.map((event) => (
                  <li className="customer-dashboard__timeline-item" key={event.statusEventId}>
                    <span aria-hidden="true" className="customer-dashboard__timeline-marker" />
                    <div className="customer-dashboard__timeline-content">
                      <div className="customer-dashboard__timeline-heading">
                        <StatusBadge status={event.statusName} />
                        <time dateTime={event.eventAt}>{formatDate(event.eventAt, true)}</time>
                      </div>
                      <p>{getTrackingDescription(event, parcel)}</p>
                      <TrackingEventDetails event={event} />
                    </div>
                  </li>
                ))}
              </ol>
            ) : (
              <EmptyState
                description="Tracking updates will appear here as your parcel moves through the delivery network."
                title="No tracking events yet"
              />
            )}
          </section>
        </>
      )}
    </div>
  )
}

export default CustomerParcelDetailsPage
