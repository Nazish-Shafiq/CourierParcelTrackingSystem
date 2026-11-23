import { ChevronLeft, ChevronRight, PackageCheck, X } from 'lucide-react'
import { NavLink } from 'react-router-dom'
import { getNavigationForRoles } from '../../constants/navigation'
import '../../style/layout/sidebar.css'

function Sidebar({
  isCollapsed = false,
  isMobileOpen = false,
  onClose,
  onToggleCollapse,
  assignedScreens = [],
  roles = [],
}) {
  const { roleItems, additionalItems } = getNavigationForRoles(
    roles,
    assignedScreens,
  )

  function renderItems(items) {
    return items.map(({ label, path, icon: Icon }) => (
      <NavLink
        className={({ isActive }) => `app-sidebar__link${isActive ? ' app-sidebar__link--active' : ''}`}
        end={path === '/'}
        key={path}
        onClick={onClose}
        title={isCollapsed ? label : undefined}
        to={path}
      >
        <Icon aria-hidden="true" className="app-sidebar__link-icon" size={19} />
        <span className="app-sidebar__link-label">{label}</span>
      </NavLink>
    ))
  }

  return (
    <aside
      className={`app-sidebar${isCollapsed ? ' app-sidebar--collapsed' : ''}${isMobileOpen ? ' app-sidebar--mobile-open' : ''}`}
      id="app-sidebar"
    >
      <div className="app-sidebar__brand-row">
        <NavLink
          aria-label="Courier Parcel Tracking System home"
          className="app-sidebar__brand"
          onClick={onClose}
          to="/"
        >
          <span className="app-sidebar__brand-icon"><PackageCheck aria-hidden="true" size={23} /></span>
          <span className="app-sidebar__brand-name">Courier Parcel Tracking System</span>
        </NavLink>
        <button
          aria-label="Close navigation menu"
          className="app-sidebar__mobile-close"
          onClick={onClose}
          type="button"
        >
          <X aria-hidden="true" size={20} />
        </button>
      </div>

      <nav aria-label="Main navigation" className="app-sidebar__navigation">
        <p className="app-sidebar__section-label">Your role</p>
        {renderItems(roleItems)}
        {additionalItems.length > 0 && (
          <>
            <p className="app-sidebar__section-label">Additional access</p>
            {renderItems(additionalItems)}
          </>
        )}
      </nav>

      <button
        aria-label={isCollapsed ? 'Expand sidebar' : 'Collapse sidebar'}
        aria-pressed={isCollapsed}
        className="app-sidebar__collapse"
        onClick={onToggleCollapse}
        type="button"
      >
        {isCollapsed ? <ChevronRight size={18} /> : <ChevronLeft size={18} />}
        <span>{isCollapsed ? 'Expand' : 'Collapse sidebar'}</span>
      </button>
    </aside>
  )
}

export default Sidebar
