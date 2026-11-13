import { ChevronRight } from 'lucide-react'
import { Link } from 'react-router-dom'
import '../../style/components/components.css'

function Breadcrumbs({ items = [] }) {
  if (items.length === 0) return null

  return (
    <nav aria-label="Breadcrumb" className="ui-breadcrumbs">
      <ol className="ui-breadcrumbs__list">
        {items.map((item, index) => {
          const isCurrent = index === items.length - 1

          return (
            <li className="ui-breadcrumbs__item" key={`${item.label}-${index}`}>
              {isCurrent || !item.to
                ? <span aria-current={isCurrent ? 'page' : undefined}>{item.label}</span>
                : <Link to={item.to}>{item.label}</Link>}
              {!isCurrent && <ChevronRight aria-hidden="true" size={14} />}
            </li>
          )
        })}
      </ol>
    </nav>
  )
}

export default Breadcrumbs
