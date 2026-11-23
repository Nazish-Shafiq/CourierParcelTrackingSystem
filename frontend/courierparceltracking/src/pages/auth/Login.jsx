import { useState } from 'react'
import { KeyRound, PackageCheck, UserRound } from 'lucide-react'
import { useAuth } from '../../hooks/useAuth'
import PasswordInput from '../../components/forms/PasswordInput'
import '../../style/auth/login.css'

function Login() {
  const { login, authError } = useAuth()
  const [userNameOrEmail, setUserNameOrEmail] = useState('')
  const [password, setPassword] = useState('')
  const [formError, setFormError] = useState('')
  const [isSubmitting, setIsSubmitting] = useState(false)

  async function handleSubmit(event) {
    event.preventDefault()
    setFormError('')
    setIsSubmitting(true)

    try {
      await login({ userNameOrEmail: userNameOrEmail.trim(), password })
    } catch (error) {
      setFormError(error.message)
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <main className="login-page">
      <section aria-labelledby="login-title" className="login-card">
        <div className="login-brand">
          <span className="login-brand-icon"><PackageCheck aria-hidden="true" size={26} /></span>
          <span>Courier Parcel Tracking</span>
        </div>
        <p className="login-eyebrow">Secure access</p>
        <h1 className="login-title" id="login-title">Sign in to your account</h1>
        <p className="login-subtitle">Use the credentials provided by your organization.</p>

        {(formError || authError) && (
          <div className="alert alert-danger py-2" role="alert">
            {formError || authError.message}
          </div>
        )}

        <form onSubmit={handleSubmit}>
          <div className="mb-3">
            <label className="form-label" htmlFor="userNameOrEmail">Username or email</label>
            <div className="input-group">
              <span className="input-group-text"><UserRound aria-hidden="true" size={18} /></span>
              <input
                autoComplete="username"
                className="form-control"
                id="userNameOrEmail"
                maxLength={256}
                onChange={(event) => setUserNameOrEmail(event.target.value)}
                required
                value={userNameOrEmail}
              />
            </div>
          </div>
          <div className="mb-4">
            <label className="form-label" htmlFor="password">Password</label>
            <div className="input-group">
              <span className="input-group-text"><KeyRound aria-hidden="true" size={18} /></span>
              <PasswordInput
                autoComplete="current-password"
                id="password"
                inputClassName="form-control"
                onChange={(event) => setPassword(event.target.value)}
                required
                value={password}
                wrapperClassName="password-input--login"
              />
            </div>
          </div>
          <button className="btn btn-primary w-100 login-submit" disabled={isSubmitting} type="submit">
            {isSubmitting ? 'Signing in…' : 'Sign in'}
          </button>
        </form>
      </section>
    </main>
  )
}

export default Login
