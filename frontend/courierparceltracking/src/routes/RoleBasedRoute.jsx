import { Navigate, Outlet, useLocation, useOutletContext } from 'react-router-dom'
import {
  getNavigationItem,
  hasAssignedScreen,
  ROLE_DASHBOARD_PATHS,
} from '../constants/navigation'
import { useAuth } from '../hooks/useAuth'
import { hasAnyRole } from '../utils/roleAccess.js'

function RoleBasedRoute({ allowedRoles = [] }) {
  const { status, user } = useAuth()
  const location = useLocation()
  const {
    assignedScreens = null,
    isScreenAssignmentsLoading = false,
  } = useOutletContext() ?? {}

  if (status === 'loading') {
    return <div className="container py-5 text-center" role="status">Checking your session…</div>
  }

  if (!user) {
    return <Navigate to="/login" replace state={{ from: location }} />
  }

  if (hasAnyRole(user.roles, allowedRoles)) {
    return <Outlet />
  }

  const navigationItem = getNavigationItem(location.pathname, assignedScreens)
  const dashboardPath = Object.values(ROLE_DASHBOARD_PATHS)
    .includes(location.pathname)
    ? location.pathname
    : null
  const requiredRoutePath = dashboardPath ||
    navigationItem?.path ||
    navigationItem?.routePath
  if (!requiredRoutePath) {
    return <Navigate to="/access-denied" replace />
  }

  if (isScreenAssignmentsLoading) {
    return <div className="container py-5 text-center" role="status">Checking screen access…</div>
  }

  if (hasAssignedScreen(assignedScreens, requiredRoutePath)) {
    return <Outlet />
  }

  return <Navigate to="/access-denied" replace />
}

export default RoleBasedRoute
