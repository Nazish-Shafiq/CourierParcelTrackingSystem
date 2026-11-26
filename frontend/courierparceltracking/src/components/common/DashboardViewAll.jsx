import { Link } from 'react-router-dom'
import '../../style/components/dashboard-view-all.css'

export const DASHBOARD_PREVIEW_LIMIT = 5

function DashboardViewAll({ count, to, label = 'View all' }) {
  if (!Number.isFinite(count) || count <= DASHBOARD_PREVIEW_LIMIT) return null

  return (
    <div className="dashboard-view-all">
      <Link
        aria-label={label}
        className="dashboard-view-all__link"
        to={to}
      >
        {label}
      </Link>
    </div>
  )
}

export default DashboardViewAll
