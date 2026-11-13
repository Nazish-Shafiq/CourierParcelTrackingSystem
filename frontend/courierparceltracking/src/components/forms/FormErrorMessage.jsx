import '../../style/components/components.css'

function FormErrorMessage({ error, helpText, id }) {
  const message = error || helpText
  if (!message) return null

  return (
    <span
      className={`ui-form-field__message${error ? ' ui-form-field__message--error' : ''}`}
      id={id}
    >
      {message}
    </span>
  )
}

export default FormErrorMessage
