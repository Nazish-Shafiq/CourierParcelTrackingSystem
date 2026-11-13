import { RefreshCw } from 'lucide-react'
import Button from './Button'

function RefreshButton({
  disabled = false,
  isLoading = false,
  loadingLabel = 'Refreshing…',
  onRefresh,
  ...props
}) {
  return (
    <Button
      aria-busy={isLoading}
      aria-label={isLoading ? loadingLabel : 'Refresh'}
      disabled={disabled || isLoading}
      onClick={onRefresh}
      variant="secondary"
      {...props}
    >
      <RefreshCw aria-hidden="true" className={isLoading ? 'ui-spin' : ''} size={17} />
      <span>{isLoading ? loadingLabel : 'Refresh'}</span>
    </Button>
  )
}

export default RefreshButton
