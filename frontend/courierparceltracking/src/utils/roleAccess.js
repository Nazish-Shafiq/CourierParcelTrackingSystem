export function normalizeRoleName(role) {
  return typeof role === 'string' ? role.trim().toLowerCase() : ''
}

export function hasAnyRole(userRoles, allowedRoles) {
  if (!Array.isArray(userRoles) || !Array.isArray(allowedRoles)) {
    return false
  }

  const normalizedUserRoles = new Set(userRoles.map(normalizeRoleName).filter(Boolean))
  return allowedRoles.some((role) => normalizedUserRoles.has(normalizeRoleName(role)))
}
