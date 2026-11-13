import { AlertCircle } from 'lucide-react'
import Button from './Button'
import '../../style/components/components.css'

function ErrorMessage({ error, onRetry, title = 'Something went wrong' }) {
  const message = typeof error === 'string' ? error : error?.message

  return (
    <div className="ui-error" role="alert">
      <AlertCircle aria-hidden="true" className="ui-error__icon" size={21} />
      <div className="ui-error__content">
        <h3 className="ui-error__title">{title}</h3>
        {message && <p className="ui-error__message">{message}</p>}
      </div>
      {onRetry && <Button onClick={onRetry} variant="outline">Try again</Button>}
    </div>
  )
}

export default ErrorMessage
