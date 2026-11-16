import { Navigate, Route, Routes, useLocation } from 'react-router-dom'
import DashboardLayout from './components/layout/DashboardLayout'
import PageHeader from './components/common/PageHeader'
import {
  getDashboardPathForRoles,
  getNavigationItem,
  getPostLoginRedirect,
  ROLE_NAMES,
} from './constants/navigation'
import { useAuth } from './hooks/useAuth'
import { hasAnyRole } from './utils/roleAccess.js'
import {
  AdminCouriersPage,
  AdminCustomersPage,
  AdminHubsPage,
  AdminParcelStatusesPage,
  AdminParcelsPage,
  AdminRoutesPage,
  AdminUsersPage,
} from './pages/admin/AdminManagementPages'
import AdminDashboard from './pages/admin/AdminDashboard'
import AdminReportsPage from './pages/admin/AdminReportsPage'
import BookingAgentBookParcelPage from './pages/bookingAgent/BookingAgentBookParcelPage'
import BookingAgentBookingsPage from './pages/bookingAgent/BookingAgentBookingsPage'
import BookingAgentCustomersPage from './pages/bookingAgent/BookingAgentCustomersPage'
import BookingAgentDashboard from './pages/bookingAgent/BookingAgentDashboard'
import BookingAgentRegisterCustomerPage from './pages/bookingAgent/BookingAgentRegisterCustomerPage'
import BookingAgentTrackParcelPage from './pages/bookingAgent/BookingAgentTrackParcelPage'
import Login from './pages/auth/Login'
import CourierDashboard from './pages/courier/CourierDashboard'
import {
  CourierAssignedParcelsPage,
  CourierParcelDetailsPage,
  CourierParcelTrackingPage,
} from './pages/courier/CourierParcelPages'
import CustomerDashboard from './pages/customer/CustomerDashboard'
import DispatcherDashboard from './pages/dispatcher/DispatcherDashboard'
import {
  DispatcherAssignParcelPage,
  DispatcherAssignmentsPage,
} from './pages/dispatcher/DispatcherAssignmentPages'
import {
  DispatcherCouriersPage,
  DispatcherHubsPage,
  DispatcherRoutesPage,
  DispatcherTrackingPage,
} from './pages/dispatcher/DispatcherReferencePages'
import {
  DispatcherParcelsPage,
  DispatcherUnassignedParcelsPage,
} from './pages/dispatcher/DispatcherParcelPages'
import ProtectedRoute from './routes/ProtectedRoute'
import RoleBasedRoute from './routes/RoleBasedRoute'

function DashboardIndex() {
  const { user } = useAuth()
  const dashboardPath = getDashboardPathForRoles(user?.roles)

  return <Navigate to={dashboardPath || '/access-denied'} replace />
}

function NavigationPlaceholder() {
  const { pathname } = useLocation()
  const { user } = useAuth()
  const navigationItem = getNavigationItem(pathname)

  if (!navigationItem) {
    return <Navigate to="/" replace />
  }

  if (!hasAnyRole(user?.roles, navigationItem.allowedRoles)) {
    return <Navigate to="/access-denied" replace />
  }

  return (
    <section className="welcome-panel">
      <PageHeader
        description="This navigation destination is prepared for a future phase. No business module or API request is included yet."
        eyebrow="Coming in a later phase"
        title={navigationItem.label}
      />
    </section>
  )
}

function AccessDenied() {
  return (
    <div className="alert alert-warning" role="alert">
      Your account does not have permission to access this page.
    </div>
  )
}

function LoginRoute() {
  const { status, user } = useAuth()
  const location = useLocation()

  if (status === 'loading') {
    return <div className="container py-5 text-center" role="status">Checking your session…</div>
  }

  if (user) {
    return (
      <Navigate
        replace
        to={getPostLoginRedirect(user.roles, location.state?.from)}
      />
    )
  }

  return <Login />
}

function AppRoutes() {
  return (
    <Routes>
      <Route path="/login" element={<LoginRoute />} />
      <Route element={<ProtectedRoute />}>
        <Route element={<DashboardLayout />}>
          <Route index element={<DashboardIndex />} />
          <Route element={<RoleBasedRoute allowedRoles={[ROLE_NAMES.ADMIN]} />}>
            <Route path="dashboard/admin" element={<AdminDashboard />} />
            <Route path="admin/users" element={<AdminUsersPage />} />
            <Route path="admin/customers" element={<AdminCustomersPage />} />
            <Route path="admin/couriers" element={<AdminCouriersPage />} />
            <Route path="admin/hubs" element={<AdminHubsPage />} />
            <Route path="admin/parcels" element={<AdminParcelsPage />} />
            <Route path="admin/parcel-status" element={<AdminParcelStatusesPage />} />
            <Route path="admin/routes" element={<AdminRoutesPage />} />
            <Route path="admin/reports" element={<AdminReportsPage />} />
          </Route>
          <Route element={<RoleBasedRoute allowedRoles={[ROLE_NAMES.BOOKING_AGENT]} />}>
            <Route path="dashboard/booking-agent" element={<BookingAgentDashboard />} />
            <Route path="booking/register-customer" element={<BookingAgentRegisterCustomerPage />} />
            <Route path="booking/customers" element={<BookingAgentCustomersPage />} />
            <Route path="booking/book-parcel" element={<BookingAgentBookParcelPage />} />
            <Route path="booking/bookings" element={<BookingAgentBookingsPage />} />
            <Route path="booking/track-parcel" element={<BookingAgentTrackParcelPage />} />
          </Route>
          <Route element={<RoleBasedRoute allowedRoles={[ROLE_NAMES.DISPATCHER]} />}>
            <Route path="dashboard/dispatcher" element={<DispatcherDashboard />} />
            <Route path="dispatch/parcels" element={<DispatcherParcelsPage />} />
            <Route path="dispatch/unassigned-parcels" element={<DispatcherUnassignedParcelsPage />} />
            <Route path="dispatch/assign" element={<DispatcherAssignParcelPage />} />
            <Route path="dispatch/assignments" element={<DispatcherAssignmentsPage />} />
            <Route path="dispatch/routes" element={<DispatcherRoutesPage />} />
            <Route path="dispatch/couriers" element={<DispatcherCouriersPage />} />
            <Route path="dispatch/hubs" element={<DispatcherHubsPage />} />
            <Route path="dispatch/tracking" element={<DispatcherTrackingPage />} />
          </Route>
          <Route element={<RoleBasedRoute allowedRoles={[ROLE_NAMES.COURIER]} />}>
            <Route path="dashboard/courier" element={<CourierDashboard />} />
            <Route path="courier/assigned-parcels" element={<CourierAssignedParcelsPage />} />
            <Route path="courier/assigned-parcels/:parcelId" element={<CourierParcelDetailsPage />} />
            <Route path="courier/assigned-parcels/:parcelId/tracking" element={<CourierParcelTrackingPage />} />
          </Route>
          <Route element={<RoleBasedRoute allowedRoles={[ROLE_NAMES.CUSTOMER_VIEWER]} />}>
            <Route path="dashboard/customer" element={<CustomerDashboard />} />
          </Route>
          <Route path="access-denied" element={<AccessDenied />} />
          <Route path="*" element={<NavigationPlaceholder />} />
        </Route>
      </Route>
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  )
}

export default AppRoutes
