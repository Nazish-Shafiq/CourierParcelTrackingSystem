import AdminEntityManager from '../../components/admin/AdminEntityManager'
import { customersConfig } from '../admin/adminResourceConfigs'

function BookingAgentCustomersPage() {
  return (
    <AdminEntityManager
      config={{
        ...customersConfig,
        description: 'Search, view, and create customer records that the Booking Agent can use for parcel booking.',
        title: 'Customers',
      }}
    />
  )
}

export default BookingAgentCustomersPage
