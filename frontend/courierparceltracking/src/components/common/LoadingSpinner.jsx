import { LoaderCircle } from 'lucide-react'
import '../../style/components/components.css'

function LoadingSpinner({ label = 'Loading', size = 'medium' }) {
  return (
    <div aria-live="polite" className={`ui-loading ui-loading--${size}`} role="status">
      <LoaderCircle aria-hidden="true" className="ui-spin" />
      <span>{label}</span>
    </div>
  )
}

export default LoadingSpinner
