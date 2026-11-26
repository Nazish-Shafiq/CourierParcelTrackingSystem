const ACCESS_TOKEN_KEY = 'courierParcelTracking.accessToken'

function getTokenExpiration(token) {
  const payload = token.split('.')[1]
  if (!payload) return null

  try {
    const base64 = payload.replace(/-/g, '+').replace(/_/g, '/')
    const decoded = window.atob(base64.padEnd(Math.ceil(base64.length / 4) * 4, '='))
    const expiresAt = JSON.parse(decoded).exp
    return Number.isFinite(expiresAt) ? expiresAt * 1000 : null
  } catch {
    return null
  }
}

export function getAccessToken() {
  const token = window.sessionStorage.getItem(ACCESS_TOKEN_KEY)
  if (!token) return null

  const expiresAt = getTokenExpiration(token)
  if (expiresAt !== null && expiresAt <= Date.now()) {
    clearAccessToken()
    return null
  }

  return token
}

export function storeAccessToken(token) {
  window.sessionStorage.setItem(ACCESS_TOKEN_KEY, token)
}

export function clearAccessToken() {
  window.sessionStorage.removeItem(ACCESS_TOKEN_KEY)
}
