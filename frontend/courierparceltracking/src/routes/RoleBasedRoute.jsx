import { Navigate, Outlet, useLocation } from 'react-router-dom'
import { useAuth } from '../hooks/useAuth'
import { hasAnyRole } from '../utils/roleAccess.js'

function RoleBasedRoute({ allowedRoles = [] }) {
  const { status, user } = useAuth()
  const location = useLocation()

  if (status === 'loading') {
    return <div className="container py-5 text-center" role="status">Checking your session…</div>
  }

  if (!user) {
    return <Navigate to="/login" replace state={{ from: location }} />
  }

  if (!hasAnyRole(user.roles, allowedRoles)) {
    return <Navigate to="/access-denied" replace />
  }

  return <Outlet />
}

export default RoleBasedRoute
