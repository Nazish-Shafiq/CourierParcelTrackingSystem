import '../../style/components/components.css'

function FormLabel({ children, className = '', htmlFor, required = false }) {
  return (
    <label className={`ui-form-field__label${className ? ` ${className}` : ''}`} htmlFor={htmlFor}>
      {children}
      {required && <span aria-hidden="true" className="ui-form-field__required"> *</span>}
    </label>
  )
}

export default FormLabel
