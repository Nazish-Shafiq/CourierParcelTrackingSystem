import axiosInstance from './api/axiosInstance'
import { API_BASE_URL, AUTH_API_PATH } from './api/apiConfig'
import { ApiError, getResponseData } from './api/baseResponse'
import { clearAccessToken, getAccessToken, storeAccessToken } from '../utils/tokenStorage'

let currentUserRequest = null

function ensureApiUrl() {
  if (!API_BASE_URL) {
    throw new Error('Set VITE_API_URL in the frontend .env file before making API requests.')
  }
}

function ensureSecurePasswordTransport() {
  if (typeof window === 'undefined') return

  const apiUrl = new URL(API_BASE_URL, window.location.origin)
  if (apiUrl.protocol === 'https:') return

  const proxyTarget = import.meta.env.VITE_API_PROXY_TARGET
  const isLocalHost = ['localhost', '127.0.0.1', '[::1]'].includes(apiUrl.hostname)
  const usesLocalDevProxy = import.meta.env.DEV
    && API_BASE_URL.startsWith('/')
    && isLocalHost
    && typeof proxyTarget === 'string'
    && /^https?:\/\/(localhost|127\.0\.0\.1|\[::1\])(?::\d+)?$/i.test(proxyTarget.trim())

  if (!usesLocalDevProxy) {
    throw new ApiError('Password requests require an HTTPS API connection.')
  }
}

function normalizeUser(userData) {
  if (!userData || !Array.isArray(userData.roles)) {
    throw new ApiError('The authentication response did not contain the expected user and role data.')
  }

  return {
    userId: userData.userId,
    userName: userData.userName,
    fullName: userData.fullName,
    email: userData.email,
    roles: userData.roles,
    roleIds: Array.isArray(userData.roleIds) ? userData.roleIds : [],
  }
}

export async function bootstrapAdmin(request) {
  ensureApiUrl()
  ensureSecurePasswordTransport()
  const response = await axiosInstance.post(`${AUTH_API_PATH}/bootstrap-admin`, request)
  return getResponseData(response.data)
}

export async function login({ userNameOrEmail, password }) {
  ensureApiUrl()
  ensureSecurePasswordTransport()
  const response = await axiosInstance.post(`${AUTH_API_PATH}/login`, {
    userNameOrEmail,
    password,
  })
  const loginData = getResponseData(response.data)

  if (!loginData || typeof loginData.token !== 'string' || !loginData.token) {
    throw new ApiError('The login response did not contain an access token.')
  }

  const authenticatedUser = normalizeUser(loginData)
  storeAccessToken(loginData.token)
  if (getAccessToken() !== loginData.token) {
    throw new ApiError('The access token could not be saved in session storage.')
  }
  return authenticatedUser
}

export async function registerStaff(request) {
  ensureApiUrl()
  ensureSecurePasswordTransport()
  const response = await axiosInstance.post(`${AUTH_API_PATH}/register-staff`, request)
  return getResponseData(response.data)
}

export async function getStaffRoles(signal) {
  ensureApiUrl()
  const response = await axiosInstance.get(`${AUTH_API_PATH}/staff-roles`, { signal })
  const roles = getResponseData(response.data)

  if (!Array.isArray(roles)) {
    throw new ApiError('The staff roles response did not contain a role list.')
  }

  return roles.map((role) => {
    const roleId = Number(role?.roleId)
    if (!Number.isInteger(roleId) || roleId <= 0 || typeof role?.roleName !== 'string') {
      throw new ApiError('The staff roles response contained an invalid role.')
    }

    return { label: role.roleName, value: roleId }
  })
}

export async function getAssignableRoles(signal) {
  ensureApiUrl()
  const response = await axiosInstance.get(`${AUTH_API_PATH}/roles`, { signal })
  const roles = getResponseData(response.data)

  if (!Array.isArray(roles)) {
    throw new ApiError('The roles response did not contain a role list.')
  }

  return roles.map((role) => {
    const roleId = Number(role?.roleId)
    if (!Number.isInteger(roleId) || roleId <= 0 || typeof role?.roleName !== 'string') {
      throw new ApiError('The roles response contained an invalid role.')
    }

    return { label: role.roleName, value: roleId }
  })
}

export async function registerCustomer(request) {
  ensureApiUrl()
  ensureSecurePasswordTransport()
  const response = await axiosInstance.post(`${AUTH_API_PATH}/register-customer`, request)
  return getResponseData(response.data)
}

export async function getCurrentUser() {
  ensureApiUrl()
  if (currentUserRequest) return currentUserRequest

  currentUserRequest = axiosInstance
    .get(`${AUTH_API_PATH}/me`)
    .then((response) => normalizeUser(getResponseData(response.data)))
    .finally(() => {
      currentUserRequest = null
    })

  return currentUserRequest
}

export async function logout() {
  try {
    ensureApiUrl()
    const response = await axiosInstance.post(`${AUTH_API_PATH}/logout`)
    getResponseData(response.data)
  } finally {
    clearAccessToken()
  }
}
