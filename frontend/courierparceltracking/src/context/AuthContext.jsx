import { useCallback, useEffect, useMemo, useState } from 'react'
import AuthContext from './authContext'
import { clearAccessToken, getAccessToken } from '../utils/tokenStorage'
import * as authService from '../services/authService'

function AuthProvider({ children }) {
  const [user, setUser] = useState(null)
  const [status, setStatus] = useState(() => (getAccessToken() ? 'loading' : 'unauthenticated'))
  const [authError, setAuthError] = useState(null)

  useEffect(() => {
    let active = true

    function handleUnauthorized() {
      if (!active) return
      setUser(null)
      setStatus('unauthenticated')
    }

    window.addEventListener('auth:unauthorized', handleUnauthorized)

    if (getAccessToken()) {
      authService.getCurrentUser()
        .then((currentUser) => {
          if (!active) return
          setUser(currentUser)
          setStatus('authenticated')
        })
        .catch((error) => {
          if (!active) return
          if (error.status === 404) clearAccessToken()
          setUser(null)
          setStatus('unauthenticated')
          setAuthError(error.status === 401
            ? new Error('Your session is no longer valid. Please sign in again.')
            : error)
        })
    }

    return () => {
      active = false
      window.removeEventListener('auth:unauthorized', handleUnauthorized)
    }
  }, [])

  const login = useCallback(async (credentials) => {
    setAuthError(null)
    try {
      const authenticatedUser = await authService.login(credentials)
      setUser(authenticatedUser)
      setStatus('authenticated')
      return authenticatedUser
    } catch (error) {
      setUser(null)
      setStatus('unauthenticated')
      throw error
    }
  }, [])

  const logout = useCallback(async () => {
    try {
      await authService.logout()
    } finally {
      clearAccessToken()
      setUser(null)
      setStatus('unauthenticated')
    }
  }, [])

  const value = useMemo(() => ({
    user,
    status,
    authError,
    login,
    logout,
  }), [user, status, authError, login, logout])

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

export default AuthProvider
