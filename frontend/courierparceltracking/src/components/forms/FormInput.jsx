import '../../style/components/components.css'
import FormErrorMessage from './FormErrorMessage'
import FormLabel from './FormLabel'
import PasswordInput from './PasswordInput'

function FormInput({
  className = '',
  error,
  helpText,
  id,
  label,
  required = false,
  ...inputProps
}) {
  const messageId = `${id}-message`
  const { type, ...controlProps } = inputProps
  const controlClassName = `ui-form-field__control${error ? ' ui-form-field__control--invalid' : ''}${className ? ` ${className}` : ''}`

  return (
    <div className="ui-form-field">
      <FormLabel htmlFor={id} required={required}>{label}</FormLabel>
      {type === 'password'
        ? (
          <PasswordInput
            {...controlProps}
            aria-describedby={error || helpText ? messageId : undefined}
            aria-invalid={Boolean(error)}
            id={id}
            inputClassName={controlClassName}
            required={required}
          />
        )
        : (
          <input
            aria-describedby={error || helpText ? messageId : undefined}
            aria-invalid={Boolean(error)}
            className={controlClassName}
            id={id}
            required={required}
            type={type}
            {...controlProps}
          />
        )}
      <FormErrorMessage error={error} helpText={helpText} id={messageId} />
    </div>
  )
}

export default FormInput
