import '../../style/components/components.css'
import Breadcrumbs from './Breadcrumbs'

function PageHeader({ actions, breadcrumbs, description, eyebrow, title }) {
  return (
    <header className="page-header">
      <div className="page-header__copy">
        {breadcrumbs?.length > 0 && <Breadcrumbs items={breadcrumbs} />}
        {eyebrow && <p className="page-header__eyebrow">{eyebrow}</p>}
        <h2 className="page-header__title">{title}</h2>
        {description && <p className="page-header__description">{description}</p>}
      </div>
      {actions && <div className="page-header__actions">{actions}</div>}
    </header>
  )
}

export default PageHeader
