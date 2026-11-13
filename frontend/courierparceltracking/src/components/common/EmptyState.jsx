import { Inbox } from 'lucide-react'
import '../../style/components/components.css'

function EmptyState({
  action,
  description = 'There is nothing to show here yet.',
  icon: Icon = Inbox,
  title = 'No data available',
}) {
  return (
    <div className="ui-empty-state">
      <span className="ui-empty-state__icon"><Icon aria-hidden="true" size={24} /></span>
      <h3 className="ui-empty-state__title">{title}</h3>
      <p className="ui-empty-state__description">{description}</p>
      {action && <div className="ui-empty-state__action">{action}</div>}
    </div>
  )
}

export default EmptyState
