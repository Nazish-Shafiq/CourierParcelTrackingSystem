import { ShieldAlert } from 'lucide-react'
import EmptyState from './EmptyState'
import PageHeader from './PageHeader'

function UnavailableDashboard({ className, description, title }) {
  return (
    <div className={`role-dashboard role-dashboard--unavailable ${className || ''}`.trim()}>
      <PageHeader
        eyebrow="Workspace"
        title={title}
      />
      <EmptyState
        description={description}
        icon={ShieldAlert}
        title="Dashboard data is not available"
      />
    </div>
  )
}

export default UnavailableDashboard
