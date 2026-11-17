import { useId } from 'react'
import Button from './Button'
import '../../style/components/confirm-dialog.css'

function ConfirmDialog({
  children,
  confirmLabel = 'Confirm',
  confirmVariant = 'primary',
  description,
  isConfirming = false,
  onCancel,
  onConfirm,
  title,
}) {
  const titleId = useId()
  const descriptionId = useId()

  return (
    <div className="confirm-dialog-backdrop">
      <section
        aria-describedby={description ? descriptionId : undefined}
        aria-labelledby={titleId}
        aria-modal="true"
        className="confirm-dialog"
        role="dialog"
      >
        <h2 className="confirm-dialog__title" id={titleId}>{title}</h2>
        {description && (
          <p className="confirm-dialog__description" id={descriptionId}>
            {description}
          </p>
        )}
        {children}
        <div className="confirm-dialog__actions">
          <Button disabled={isConfirming} onClick={onCancel} variant="outline">
            Cancel
          </Button>
          <Button
            isLoading={isConfirming}
            onClick={onConfirm}
            variant={confirmVariant}
          >
            {confirmLabel}
          </Button>
        </div>
      </section>
    </div>
  )
}

export default ConfirmDialog
