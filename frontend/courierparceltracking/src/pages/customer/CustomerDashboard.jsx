import UnavailableDashboard from '../../components/common/UnavailableDashboard'
import '../../style/customer/customer-dashboard.css'

function CustomerDashboard() {
  return (
    <UnavailableDashboard
      className="role-dashboard--customer"
      description="The backend does not currently provide a Customer Viewer dashboard or an endpoint that scopes parcel information to the signed-in customer. No shared parcel or tracking data is requested."
      title="Customer Dashboard"
    />
  )
}

export default CustomerDashboard
