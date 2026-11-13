import FormErrorMessage from './FormErrorMessage'
import FormLabel from './FormLabel'
import '../../style/components/components.css'

function FormTextArea({
  className = '',
  error,
  helpText,
  id,
  label,
  required = false,
  rows = 4,
  ...textAreaProps
}) {
  const messageId = `${id}-message`

  return (
    <div className="ui-form-field">
      <FormLabel htmlFor={id} required={required}>{label}</FormLabel>
      <textarea
        aria-describedby={error || helpText ? messageId : undefined}
        aria-invalid={Boolean(error)}
        className={`ui-form-field__control ui-form-field__textarea${error ? ' ui-form-field__control--invalid' : ''}${className ? ` ${className}` : ''}`}
        id={id}
        required={required}
        rows={rows}
        {...textAreaProps}
      />
      <FormErrorMessage error={error} helpText={helpText} id={messageId} />
    </div>
  )
}

export default FormTextArea
