import UnavailableDashboard from '../../components/common/UnavailableDashboard'
import '../../style/bookingAgent/booking-agent-dashboard.css'

function BookingAgentDashboard() {
  return (
    <UnavailableDashboard
      className="role-dashboard--booking-agent"
      description="The backend does not currently provide a Booking Agent dashboard API. Shared booking endpoints are not used here because they do not scope dashboard data by role."
      title="Booking Agent Dashboard"
    />
  )
}

export default BookingAgentDashboard
