import { useEffect, useId } from 'react'
import Button from '../common/Button'
import '../../style/components/components.css'

function ConfirmationModal({
  cancelLabel = 'Cancel',
  confirmLabel = 'Confirm',
  isBusy = false,
  isOpen = false,
  message,
  onCancel,
  onConfirm,
  title = 'Confirm action',
}) {
  const titleId = useId()

  useEffect(() => {
    if (!isOpen) return undefined

    function handleKeyDown(event) {
      if (event.key === 'Escape' && !isBusy) onCancel?.()
    }

    window.addEventListener('keydown', handleKeyDown)
    return () => window.removeEventListener('keydown', handleKeyDown)
  }, [isBusy, isOpen, onCancel])

  if (!isOpen) return null

  return (
    <div className="ui-modal-backdrop">
      <section
        aria-labelledby={titleId}
        aria-modal="true"
        className="ui-modal"
        role="dialog"
      >
        <h2 className="ui-modal__title" id={titleId}>{title}</h2>
        {message && <p className="ui-modal__message">{message}</p>}
        <div className="ui-modal__actions">
          <Button disabled={isBusy} onClick={onCancel} variant="outline">{cancelLabel}</Button>
          <Button isLoading={isBusy} onClick={onConfirm} variant="danger">{confirmLabel}</Button>
        </div>
      </section>
    </div>
  )
}

export default ConfirmationModal
