import { useState } from 'react'
import Button from '../../components/common/Button'
import EmptyState from '../../components/common/EmptyState'
import ErrorMessage from '../../components/common/ErrorMessage'
import FormInput from '../../components/forms/FormInput'
import LoadingSpinner from '../../components/common/LoadingSpinner'
import ParcelRouteSummary from '../../components/common/ParcelRouteSummary'
import PageHeader from '../../components/common/PageHeader'
import StatusBadge from '../../components/common/StatusBadge'
import TrackingEventDetails from '../../components/common/TrackingEventDetails'
import { getTrackingHistory } from '../../services/trackingService'

function formatDate(value) {
  return value ? new Date(value).toLocaleString() : '—'
}

function BookingAgentTrackParcelPage() {
  const [trackingNumber, setTrackingNumber] = useState('')
  const [history, setHistory] = useState([])
  const [isLoading, setIsLoading] = useState(false)
  const [error, setError] = useState(null)

  async function handleSubmit(event) {
    event.preventDefault()
    const value = trackingNumber.trim()
    if (!value) {
      setError(new Error('Enter a tracking number to view parcel history.'))
      return
    }

    setIsLoading(true)
    setError(null)
    try {
      const result = await getTrackingHistory(value)
      setHistory(Array.isArray(result) ? result : [])
    } catch (requestError) {
      setError(requestError)
      setHistory([])
    } finally {
      setIsLoading(false)
    }
  }

  return (
    <div className="role-dashboard role-dashboard--booking-agent">
      <PageHeader
        description="Search by tracking number to review parcel status history and the most recent events from the backend."
        eyebrow="Tracking"
        title="Track Parcel"
      />

      <section className="booking-agent-form-card booking-agent-form-card--compact">
        <form className="booking-agent-form booking-agent-form--inline" onSubmit={handleSubmit}>
          <FormInput
            id="trackingNumber"
            label="Tracking number"
            onChange={(event) => setTrackingNumber(event.target.value)}
            placeholder="Enter tracking number"
            value={trackingNumber}
          />
          <Button disabled={isLoading} isLoading={isLoading} type="submit">
            Search
          </Button>
        </form>
      </section>

      {error && <ErrorMessage error={error} />}
      {isLoading && <LoadingSpinner label="Loading tracking history" />}

      {!isLoading && history.length === 0 && !error && (
        <EmptyState description="No tracking events were returned for the provided tracking number." title="No tracking history" />
      )}

      {history.length > 0 && (
        <section className="booking-agent-list-card">
          <div className="booking-agent-list-card__header">
            <h3>Tracking history</h3>
            <span>{history[0].trackingNumber}</span>
          </div>
          <ParcelRouteSummary data={history[0]} />
          <div className="booking-agent-timeline">
            {history
              .slice()
              .sort((a, b) => new Date(a.eventAt) - new Date(b.eventAt))
              .map((event) => (
                <article className="booking-agent-timeline__item" key={event.statusEventId}>
                  <div className="booking-agent-timeline__marker" />
                  <div className="booking-agent-timeline__content">
                    <div className="booking-agent-timeline__row">
                      <StatusBadge status={event.statusName} />
                      <span>{formatDate(event.eventAt)}</span>
                    </div>
                    <p><strong>{event.statusName}</strong> ({event.statusCode})</p>
                    {event.notes && <p>{event.notes}</p>}
                    <TrackingEventDetails event={event} />
                  </div>
                </article>
              ))}
          </div>
        </section>
      )}
    </div>
  )
}

export default BookingAgentTrackParcelPage
