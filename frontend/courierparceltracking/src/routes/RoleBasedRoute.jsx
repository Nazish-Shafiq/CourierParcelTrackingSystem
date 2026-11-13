import { Navigate, Outlet, useLocation } from 'react-router-dom'
import { useAuth } from '../hooks/useAuth'

function RoleBasedRoute({ allowedRoles = [] }) {
  const { status, user } = useAuth()
  const location = useLocation()

  if (status === 'loading') {
    return <div className="container py-5 text-center" role="status">Checking your session…</div>
  }

  if (!user) {
    return <Navigate to="/login" replace state={{ from: location }} />
  }

  const roleNames = new Set(user.roles.map((role) => role.toLowerCase()))
  const hasAccess = allowedRoles.some((role) => roleNames.has(role.toLowerCase()))

  if (!hasAccess) {
    return <Navigate to="/access-denied" replace />
  }

  return <Outlet />
}

export default RoleBasedRoute
