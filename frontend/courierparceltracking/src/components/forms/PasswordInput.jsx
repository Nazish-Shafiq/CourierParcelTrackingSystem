import { useState } from 'react'
import { Eye, EyeOff } from 'lucide-react'
import '../../style/components/password-input.css'

function PasswordInput({
  id,
  inputClassName = '',
  wrapperClassName = '',
  ...inputProps
}) {
  const [isVisible, setIsVisible] = useState(false)

  return (
    <div className={`password-input ${wrapperClassName}`.trim()}>
      <input
        {...inputProps}
        className={`password-input__field ${inputClassName}`.trim()}
        id={id}
        type={isVisible ? 'text' : 'password'}
      />
      <button
        aria-controls={id}
        aria-label={isVisible ? 'Hide password' : 'Show password'}
        aria-pressed={isVisible}
        className="password-input__toggle"
        onClick={() => setIsVisible((visible) => !visible)}
        type="button"
      >
        {isVisible
          ? <EyeOff aria-hidden="true" size={18} />
          : <Eye aria-hidden="true" size={18} />}
      </button>
    </div>
  )
}

export default PasswordInput
