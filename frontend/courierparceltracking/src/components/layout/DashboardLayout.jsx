import { useEffect, useState } from 'react'
import { Outlet } from 'react-router-dom'
import { useAuth } from '../../hooks/useAuth'
import ErrorMessage from '../common/ErrorMessage'
import Navbar from './Navbar'
import Sidebar from './Sidebar'
import '../../style/layout/dashboard-layout.css'

function DashboardLayout() {
  const { user, logout } = useAuth()
  const [isCollapsed, setIsCollapsed] = useState(false)
  const [isMobileOpen, setIsMobileOpen] = useState(false)
  const [logoutError, setLogoutError] = useState(null)

  useEffect(() => {
    if (!isMobileOpen) return undefined

    function handleKeyDown(event) {
      if (event.key === 'Escape') setIsMobileOpen(false)
    }

    window.addEventListener('keydown', handleKeyDown)
    return () => window.removeEventListener('keydown', handleKeyDown)
  }, [isMobileOpen])

  async function handleLogout() {
    setLogoutError(null)
    try {
      await logout()
    } catch (error) {
      setLogoutError(error)
    }
  }

  function closeMobileNavigation() {
    setIsMobileOpen(false)
  }

  return (
    <div className={`dashboard-layout${isCollapsed ? ' dashboard-layout--collapsed' : ''}`}>
      <Sidebar
        isCollapsed={isCollapsed}
        isMobileOpen={isMobileOpen}
        onClose={closeMobileNavigation}
        onToggleCollapse={() => setIsCollapsed((collapsed) => !collapsed)}
        roles={user?.roles ?? []}
      />
      {isMobileOpen && (
        <button
          aria-label="Close navigation menu"
          className="dashboard-layout__backdrop"
          onClick={closeMobileNavigation}
          type="button"
        />
      )}
      <div className="dashboard-layout__workspace">
        <Navbar
          isNavigationOpen={isMobileOpen}
          onLogout={handleLogout}
          onToggleNavigation={() => setIsMobileOpen((open) => !open)}
          roles={user?.roles ?? []}
          user={user}
        />
        <main className="dashboard-layout__main">
          {logoutError && <ErrorMessage error={logoutError} />}
          <Outlet />
        </main>
      </div>
    </div>
  )
}

export default DashboardLayout
