import { useEffect, useState } from 'react'
import { Outlet, useLocation } from 'react-router-dom'
import { useAuth } from '../../hooks/useAuth'
import ErrorMessage from '../common/ErrorMessage'
import Navbar from './Navbar'
import Sidebar from './Sidebar'
import { getMyAssignedScreens } from '../../services/screenAssignmentService'
import '../../style/layout/dashboard-layout.css'

function DashboardLayout() {
  const { user, logout } = useAuth()
  const location = useLocation()
  const [isCollapsed, setIsCollapsed] = useState(false)
  const [isMobileOpen, setIsMobileOpen] = useState(false)
  const [logoutError, setLogoutError] = useState(null)
  const [screenAccess, setScreenAccess] = useState({
    requestId: null,
    screens: null,
    error: null,
  })
  const [accessRefreshKey, setAccessRefreshKey] = useState(0)
  const currentUserId = user?.userId
  const accessRequestId = `${currentUserId}:${location.pathname}:${accessRefreshKey}`

  useEffect(() => {
    function refreshOnFocus() {
      setAccessRefreshKey((key) => key + 1)
    }

    window.addEventListener('focus', refreshOnFocus)
    return () => window.removeEventListener('focus', refreshOnFocus)
  }, [])

  useEffect(() => {
    if (!currentUserId) return undefined

    const controller = new AbortController()
    getMyAssignedScreens(controller.signal)
      .then((screens) => {
        if (!controller.signal.aborted) {
          setScreenAccess({ requestId: accessRequestId, screens, error: null })
        }
      })
      .catch((error) => {
        if (controller.signal.aborted) return
        setScreenAccess({ requestId: accessRequestId, screens: [], error })
      })

    return () => controller.abort()
  }, [currentUserId, accessRequestId])

  const hasCurrentScreenAccess = screenAccess.requestId === accessRequestId
  const assignedScreens = hasCurrentScreenAccess ? screenAccess.screens : null
  const screenAssignmentError = hasCurrentScreenAccess
    ? screenAccess.error
    : null

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
        assignedScreens={assignedScreens ?? []}
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
          {screenAssignmentError && <ErrorMessage error={screenAssignmentError} />}
          <Outlet context={{
            assignedScreens,
            isScreenAssignmentsLoading: assignedScreens === null,
          }} />
        </main>
      </div>
    </div>
  )
}

export default DashboardLayout
