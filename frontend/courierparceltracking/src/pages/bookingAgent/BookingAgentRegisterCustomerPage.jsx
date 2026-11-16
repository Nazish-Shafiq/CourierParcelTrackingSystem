import { useState } from 'react'
import Button from '../../components/common/Button'
import ErrorMessage from '../../components/common/ErrorMessage'
import FormInput from '../../components/forms/FormInput'
import PageHeader from '../../components/common/PageHeader'
import { registerCustomer } from '../../services/authService'

const initialValues = {
  userName: '',
  email: '',
  password: '',
  fullName: '',
  phone: '',
  addressLine: '',
  city: '',
  province: '',
  postalCode: '',
}

function BookingAgentRegisterCustomerPage() {
  const [values, setValues] = useState(initialValues)
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [error, setError] = useState(null)
  const [success, setSuccess] = useState(null)

  function handleChange(event) {
    const { name, value } = event.target
    setValues((current) => ({ ...current, [name]: value }))
    if (error) setError(null)
    if (success) setSuccess(null)
  }

  async function handleSubmit(event) {
    event.preventDefault()

    const payload = {
      userName: values.userName.trim(),
      email: values.email.trim(),
      password: values.password,
      fullName: values.fullName.trim(),
      phone: values.phone.trim(),
      addressLine: values.addressLine.trim() || undefined,
      city: values.city.trim() || undefined,
      province: values.province.trim() || undefined,
      postalCode: values.postalCode.trim() || undefined,
    }

    if (!payload.userName || !payload.email || !payload.password || !payload.fullName || !payload.phone) {
      setError(new Error('Please complete the required customer account details.'))
      return
    }

    setIsSubmitting(true)
    setError(null)
    setSuccess(null)

    try {
      const result = await registerCustomer(payload)
      setSuccess(result)
      setValues(initialValues)
    } catch (requestError) {
      setError(requestError)
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <div className="role-dashboard role-dashboard--booking-agent">
      <PageHeader
        description="Create a customer account for the customer viewer experience using the Booking Agent registration API."
        eyebrow="Customer onboarding"
        title="Register Customer"
      />

      <section className="booking-agent-form-card booking-agent-form-card--compact">
        <form className="booking-agent-form" onSubmit={handleSubmit}>
          <div className="booking-agent-form__grid">
            <FormInput id="userName" label="Username" name="userName" onChange={handleChange} required value={values.userName} />
            <FormInput id="email" label="Email" name="email" onChange={handleChange} required type="email" value={values.email} />
            <FormInput id="password" label="Password" name="password" onChange={handleChange} required type="password" value={values.password} />
            <FormInput id="fullName" label="Full name" name="fullName" onChange={handleChange} required value={values.fullName} />
            <FormInput id="phone" label="Phone" name="phone" onChange={handleChange} required type="tel" value={values.phone} />
            <FormInput id="addressLine" label="Address" name="addressLine" onChange={handleChange} value={values.addressLine} />
            <FormInput id="city" label="City" name="city" onChange={handleChange} value={values.city} />
            <FormInput id="province" label="Province" name="province" onChange={handleChange} value={values.province} />
            <FormInput id="postalCode" label="Postal code" name="postalCode" onChange={handleChange} value={values.postalCode} />
          </div>

          <div className="booking-agent-form__actions">
            <Button disabled={isSubmitting} isLoading={isSubmitting} type="submit">
              Register customer
            </Button>
          </div>
        </form>
      </section>

      {error && <ErrorMessage error={error} />}
      {success && (
        <div className="booking-agent-success-banner" role="status">
          <strong>Customer account created successfully.</strong>
          <span>{typeof success === 'string' ? success : 'The customer viewer account has been registered.'}</span>
        </div>
      )}
    </div>
  )
}

export default BookingAgentRegisterCustomerPage
