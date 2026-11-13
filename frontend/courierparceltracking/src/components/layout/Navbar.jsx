import { useState } from 'react'
import { ChevronDown, Menu, UserRound, X } from 'lucide-react'
import { useLocation } from 'react-router-dom'
import { getNavigationItem } from '../../constants/navigation'
import '../../style/layout/navbar.css'

function Navbar({
  isNavigationOpen = false,
  onLogout,
  onToggleNavigation,
  roles = [],
  user,
}) {
  const [isProfileOpen, setIsProfileOpen] = useState(false)
  const { pathname } = useLocation()
  const currentPage = getNavigationItem(pathname)

  return (
    <header className="app-navbar">
      <div className="app-navbar__page">
        <button
          aria-controls="app-sidebar"
          aria-expanded={isNavigationOpen}
          aria-label={isNavigationOpen ? 'Close navigation menu' : 'Open navigation menu'}
          className="app-navbar__menu-button"
          onClick={onToggleNavigation}
          type="button"
        >
          {isNavigationOpen
            ? <X aria-hidden="true" size={21} />
            : <Menu aria-hidden="true" size={21} />}
        </button>
        <div>
          <p className="app-navbar__eyebrow">Courier Parcel Tracking System</p>
          <h1 className="app-navbar__title">{currentPage?.label ?? 'Workspace'}</h1>
        </div>
      </div>

      <div className="app-navbar__profile">
        <button
          aria-expanded={isProfileOpen}
          aria-controls="app-navbar-profile-menu"
          aria-haspopup="menu"
          className="app-navbar__profile-button"
          onClick={() => setIsProfileOpen((open) => !open)}
          type="button"
        >
          <span className="app-navbar__avatar"><UserRound aria-hidden="true" size={18} /></span>
          <span className="app-navbar__profile-copy">
            <span className="app-navbar__user-name">{user?.fullName ?? user?.userName}</span>
            <span className="app-navbar__user-role">{roles.join(', ')}</span>
          </span>
          <ChevronDown aria-hidden="true" className="app-navbar__chevron" size={17} />
        </button>
        {isProfileOpen && (
          <div className="app-navbar__profile-menu" id="app-navbar-profile-menu" role="menu">
            <div className="app-navbar__profile-details">
              <span className="app-navbar__profile-name">{user?.fullName ?? user?.userName}</span>
              <span className="app-navbar__profile-role">{roles.join(', ')}</span>
              <span className="app-navbar__profile-email">{user?.email}</span>
            </div>
            <button
              className="app-navbar__logout-button"
              onClick={() => {
                setIsProfileOpen(false)
                onLogout()
              }}
              role="menuitem"
              type="button"
            >
              Sign out
            </button>
          </div>
        )}
      </div>
    </header>
  )
}

export default Navbar
