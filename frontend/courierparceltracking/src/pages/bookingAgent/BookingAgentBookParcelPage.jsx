import { useEffect, useMemo, useState } from 'react'
import { PackageCheck } from 'lucide-react'
import { Link } from 'react-router-dom'
import Button from '../../components/common/Button'
import ErrorMessage from '../../components/common/ErrorMessage'
import FormInput from '../../components/forms/FormInput'
import FormSelect from '../../components/forms/FormSelect'
import FormTextArea from '../../components/forms/FormTextArea'
import LoadingSpinner from '../../components/common/LoadingSpinner'
import PageHeader from '../../components/common/PageHeader'
import { createParcelBooking } from '../../services/bookingService'
import { getCustomers } from '../../services/customerService'
import { getRoutes } from '../../services/routeService'

const initialForm = {
  senderCustomerId: '',
  routeId: '',
  totalWeightKg: '',
  declaredValue: '',
  expectedDeliveryDate: '',
  specialInstructions: '',
  recipientName: '',
  recipientPhone: '',
  recipientEmail: '',
  recipientAddressLine: '',
  recipientCity: '',
  recipientProvince: '',
  recipientPostalCode: '',
}

const BOOKING_IDEMPOTENCY_KEY = 'courierParcelTracking.parcelBookingIdempotencyKey'
const BOOKING_REQUEST_FINGERPRINT = 'courierParcelTracking.parcelBookingFingerprint'

function createBookingIdempotencyKey() {
  if (!window.crypto?.randomUUID) {
    throw new Error('This browser does not support secure booking request IDs.')
  }

  const key = window.crypto.randomUUID()
  window.sessionStorage.setItem(BOOKING_IDEMPOTENCY_KEY, key)
  return key
}

function getBookingIdempotencyKey() {
  const key = window.sessionStorage.getItem(BOOKING_IDEMPOTENCY_KEY)
  return key && /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i.test(key)
    ? key
    : createBookingIdempotencyKey()
}

async function getBookingFingerprint(payload) {
  const encoded = new TextEncoder().encode(JSON.stringify(payload))
  const digest = await window.crypto.subtle.digest('SHA-256', encoded)
  return Array.from(new Uint8Array(digest), (byte) => (
    byte.toString(16).padStart(2, '0')
  )).join('')
}

function toOptions(records, idField, labelBuilder) {
  const seenIds = new Set()

  return records.map((record) => {
    const value = record?.[idField]
    if (!Number.isInteger(value) || value <= 0) {
      throw new Error(`The lookup response contains an invalid ${idField}.`)
    }
    if (seenIds.has(value)) {
      throw new Error(`The lookup response contains a duplicate ${idField}.`)
    }

    seenIds.add(value)
    return { value, label: labelBuilder(record) }
  })
}

function getPagedItems(payload, resourceName) {
  if (Array.isArray(payload)) return payload
  if (payload && Array.isArray(payload.items)) return payload.items
  throw new Error(`The ${resourceName} response did not contain a valid item list.`)
}

function BookingAgentBookParcelPage() {
  const [customers, setCustomers] = useState([])
  const [routes, setRoutes] = useState([])
  const [formValues, setFormValues] = useState(initialForm)
  const [idempotencyKey, setIdempotencyKey] = useState(getBookingIdempotencyKey)
  const [isLoadingOptions, setIsLoadingOptions] = useState(true)
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [loadError, setLoadError] = useState(null)
  const [submitError, setSubmitError] = useState(null)
  const [confirmation, setConfirmation] = useState(null)

  useEffect(() => {
    const controller = new AbortController()

    async function loadOptions() {
      setIsLoadingOptions(true)
      setLoadError(null)

      try {
        const [customerResponse, routeResponse] = await Promise.all([
          getCustomers({ page: 1, pageSize: 100, signal: controller.signal }),
          getRoutes({ page: 1, pageSize: 100, signal: controller.signal }),
        ])

        if (!controller.signal.aborted) {
          const customerItems = getPagedItems(customerResponse, 'customer')
          const routeItems = getPagedItems(routeResponse, 'route')

          setCustomers(customerItems.filter((customer) => customer.isActive))
          setRoutes(routeItems.filter((route) => route.isActive))
        }
      } catch (error) {
        if (!controller.signal.aborted) setLoadError(error)
      } finally {
        if (!controller.signal.aborted) setIsLoadingOptions(false)
      }
    }

    loadOptions()
    return () => controller.abort()
  }, [])

  const customerOptions = useMemo(
    () => toOptions(customers, 'customerId', (customer) => `${customer.fullName} (${customer.customerCode})`),
    [customers],
  )

  const routeOptions = useMemo(
    () => toOptions(routes, 'routeId', (route) => `${route.routeName} (${route.routeCode})`),
    [routes],
  )

  function handleChange(event) {
    const { name, value } = event.target
    setFormValues((current) => ({ ...current, [name]: value }))
    setConfirmation(null)
    if (submitError) setSubmitError(null)
  }

  async function handleSubmit(event) {
    event.preventDefault()

    const payload = {
      senderCustomerId: Number(formValues.senderCustomerId),
      routeId: formValues.routeId ? Number(formValues.routeId) : undefined,
      totalWeightKg: Number(formValues.totalWeightKg),
      declaredValue: formValues.declaredValue === '' ? undefined : Number(formValues.declaredValue),
      expectedDeliveryDate: formValues.expectedDeliveryDate || undefined,
      specialInstructions: formValues.specialInstructions || undefined,
      recipientName: formValues.recipientName.trim(),
      recipientPhone: formValues.recipientPhone.trim(),
      recipientEmail: formValues.recipientEmail.trim() || undefined,
      recipientAddressLine: formValues.recipientAddressLine.trim(),
      recipientCity: formValues.recipientCity.trim(),
      recipientProvince: formValues.recipientProvince.trim() || undefined,
      recipientPostalCode: formValues.recipientPostalCode.trim() || undefined,
    }

    const validationMessage = !payload.senderCustomerId || !payload.recipientName || !payload.recipientPhone || !payload.recipientAddressLine || !payload.recipientCity || !payload.totalWeightKg
      ? 'Please complete the required sender, parcel, and recipient details.'
      : null

    if (validationMessage) {
      setSubmitError(new Error(validationMessage))
      return
    }

    setIsSubmitting(true)
    setSubmitError(null)
    try {
      const fingerprint = await getBookingFingerprint(payload)
      const previousFingerprint = window.sessionStorage.getItem(
        BOOKING_REQUEST_FINGERPRINT,
      )
      const requestKey = previousFingerprint &&
        previousFingerprint !== fingerprint
        ? createBookingIdempotencyKey()
        : idempotencyKey
      setIdempotencyKey(requestKey)
      window.sessionStorage.setItem(
        BOOKING_REQUEST_FINGERPRINT,
        fingerprint,
      )

      const result = await createParcelBooking(payload, requestKey)
      setConfirmation(result)
      setFormValues(initialForm)
      window.sessionStorage.removeItem(BOOKING_REQUEST_FINGERPRINT)
      setIdempotencyKey(createBookingIdempotencyKey())
    } catch (error) {
      setSubmitError(error)
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <div className="role-dashboard role-dashboard--booking-agent">
      <PageHeader
        description="Create parcel bookings using the active customer list and the booking API exposed by the backend."
        eyebrow="Bookings"
        title="Book Parcel"
      />

      {isLoadingOptions && !customers.length && !routes.length ? (
        <LoadingSpinner label="Loading booking form" />
      ) : null}

      {loadError && <ErrorMessage error={loadError} />}
      {submitError && <ErrorMessage error={submitError} />}

      {!loadError && (
        <div className="booking-agent-page">
          <section className="booking-agent-form-card">
            <form className="booking-agent-form" onSubmit={handleSubmit}>
              <div className="booking-agent-form__section">
                <h3>Sender information</h3>
                <div className="booking-agent-form__grid">
                  <FormSelect
                    id="senderCustomerId"
                    label="Sender customer"
                    name="senderCustomerId"
                    onChange={handleChange}
                    options={customerOptions}
                    disabled={isLoadingOptions || customerOptions.length === 0}
                    helpText={isLoadingOptions
                      ? 'Loading active customers…'
                      : customerOptions.length === 0
                        ? 'No active customers are available. Register a customer before booking.'
                        : undefined}
                    placeholder={isLoadingOptions
                      ? 'Loading customers…'
                      : customerOptions.length === 0
                        ? 'No active customers available'
                        : 'Select a customer'}
                    required
                    value={formValues.senderCustomerId}
                  />
                </div>
              </div>

              <div className="booking-agent-form__section">
                <h3>Parcel information</h3>
                <div className="booking-agent-form__grid">
                  <FormInput
                    id="totalWeightKg"
                    label="Total weight (kg)"
                    max="9999999.999"
                    min="0.01"
                    name="totalWeightKg"
                    onChange={handleChange}
                    required
                    step="0.01"
                    type="number"
                    value={formValues.totalWeightKg}
                  />
                  <FormInput
                    id="declaredValue"
                    label="Declared value"
                    min="0"
                    name="declaredValue"
                    onChange={handleChange}
                    step="0.01"
                    type="number"
                    value={formValues.declaredValue}
                  />
                  <FormInput
                    id="expectedDeliveryDate"
                    label="Expected delivery date"
                    name="expectedDeliveryDate"
                    onChange={handleChange}
                    type="date"
                    value={formValues.expectedDeliveryDate}
                  />
                  <FormSelect
                    id="routeId"
                    label="Route (optional)"
                    name="routeId"
                    onChange={handleChange}
                    options={routeOptions}
                    disabled={isLoadingOptions || routeOptions.length === 0}
                    helpText={isLoadingOptions
                      ? 'Loading active routes…'
                      : routeOptions.length === 0
                        ? 'No active routes are available. You can continue without selecting a route.'
                        : undefined}
                    placeholder={isLoadingOptions
                      ? 'Loading routes…'
                      : routeOptions.length === 0
                        ? 'No active routes available'
                        : 'Select a route (optional)'}
                    value={formValues.routeId}
                  />
                  <div className="booking-agent-form__full-width">
                    <FormTextArea
                      id="specialInstructions"
                      label="Special instructions"
                      maxLength={500}
                      name="specialInstructions"
                      onChange={handleChange}
                      rows={4}
                      value={formValues.specialInstructions}
                    />
                  </div>
                </div>
              </div>

              <div className="booking-agent-form__section">
                <h3>Receiver information</h3>
                <div className="booking-agent-form__grid">
                  <FormInput
                    id="recipientName"
                    label="Receiver name"
                    name="recipientName"
                    onChange={handleChange}
                    required
                    value={formValues.recipientName}
                  />
                  <FormInput
                    id="recipientPhone"
                    label="Receiver phone"
                    name="recipientPhone"
                    onChange={handleChange}
                    required
                    type="tel"
                    value={formValues.recipientPhone}
                  />
                  <FormInput
                    id="recipientEmail"
                    label="Receiver email"
                    name="recipientEmail"
                    onChange={handleChange}
                    type="email"
                    value={formValues.recipientEmail}
                  />
                  <FormInput
                    id="recipientAddressLine"
                    label="Receiver address"
                    name="recipientAddressLine"
                    onChange={handleChange}
                    required
                    value={formValues.recipientAddressLine}
                  />
                  <FormInput
                    id="recipientCity"
                    label="Receiver city"
                    name="recipientCity"
                    onChange={handleChange}
                    required
                    value={formValues.recipientCity}
                  />
                  <FormInput
                    id="recipientProvince"
                    label="Receiver province"
                    name="recipientProvince"
                    onChange={handleChange}
                    value={formValues.recipientProvince}
                  />
                  <FormInput
                    id="recipientPostalCode"
                    label="Receiver postal code"
                    name="recipientPostalCode"
                    onChange={handleChange}
                    value={formValues.recipientPostalCode}
                  />
                </div>
              </div>

              <div className="booking-agent-form__actions">
                <Button disabled={isSubmitting} isLoading={isSubmitting} type="submit">
                  Create booking
                </Button>
              </div>
            </form>
          </section>

          {confirmation && (
            <aside className="booking-agent-confirmation" role="status">
              <h3><PackageCheck aria-hidden="true" size={18} /> Booking created successfully</h3>
              <p className="booking-agent-confirmation__message">Tracking number: <strong>{confirmation.trackingNumber}</strong></p>
              <dl className="booking-agent-confirmation__details">
                <div><dt>Parcel ID</dt><dd>{confirmation.parcelId}</dd></div>
                <div><dt>Sender</dt><dd>{confirmation.senderCustomerName}</dd></div>
                <div><dt>Status</dt><dd>{confirmation.currentStatusName}</dd></div>
                <div><dt>Weight</dt><dd>{confirmation.totalWeightKg} kg</dd></div>
                <div><dt>Booking date</dt><dd>{new Date(confirmation.bookingDate).toLocaleString()}</dd></div>
              </dl>
              <div className="booking-agent-confirmation__actions">
                <Link className="ui-button ui-button--primary" to="/booking/bookings">View bookings</Link>
                <Link className="ui-button ui-button--outline" to="/booking/track-parcel">Track parcel</Link>
              </div>
            </aside>
          )}
        </div>
      )}
    </div>
  )
}

export default BookingAgentBookParcelPage
