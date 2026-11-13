import '../../style/components/components.css'

function Button({
  children,
  className = '',
  disabled = false,
  isLoading = false,
  onClick,
  type = 'button',
  variant = 'primary',
  ...props
}) {
  return (
    <button
      className={`ui-button ui-button--${variant}${className ? ` ${className}` : ''}`}
      disabled={disabled || isLoading}
      onClick={onClick}
      type={type}
      {...props}
    >
      {isLoading && <span aria-hidden="true" className="ui-button__spinner" />}
      {children}
    </button>
  )
}

export default Button
