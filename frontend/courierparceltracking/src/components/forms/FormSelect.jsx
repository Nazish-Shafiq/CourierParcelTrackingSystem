import '../../style/components/components.css'
import FormErrorMessage from './FormErrorMessage'
import FormLabel from './FormLabel'

function FormSelect({
  className = '',
  error,
  helpText,
  id,
  label,
  options = [],
  placeholder = 'Select an option',
  required = false,
  ...selectProps
}) {
  const messageId = `${id}-message`

  return (
    <div className="ui-form-field">
      <FormLabel htmlFor={id} required={required}>{label}</FormLabel>
      <select
        aria-describedby={error || helpText ? messageId : undefined}
        aria-invalid={Boolean(error)}
        className={`ui-form-field__control${error ? ' ui-form-field__control--invalid' : ''}${className ? ` ${className}` : ''}`}
        id={id}
        required={required}
        {...selectProps}
      >
        {placeholder && <option value="">{placeholder}</option>}
        {options.map((option) => (
          <option disabled={option.disabled} key={option.value} value={option.value}>
            {option.label}
          </option>
        ))}
      </select>
      <FormErrorMessage error={error} helpText={helpText} id={messageId} />
    </div>
  )
}

export default FormSelect
