import axiosInstance from './api/axiosInstance'
import { API_BASE_URL, AUTH_API_PATH } from './api/apiConfig'
import { ApiError, getResponseData } from './api/baseResponse'
import { clearAccessToken, getAccessToken, storeAccessToken } from '../utils/tokenStorage'

function ensureApiUrl() {
  if (!API_BASE_URL) {
    throw new Error('Set VITE_API_URL in the frontend .env file before making API requests.')
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
  const response = await axiosInstance.post(`${AUTH_API_PATH}/bootstrap-admin`, request)
  return getResponseData(response.data)
}

export async function login({ userNameOrEmail, password }) {
  ensureApiUrl()
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
  const response = await axiosInstance.post(`${AUTH_API_PATH}/register-staff`, request)
  return getResponseData(response.data)
}

export async function registerCustomer(request) {
  ensureApiUrl()
  const response = await axiosInstance.post(`${AUTH_API_PATH}/register-customer`, request)
  return getResponseData(response.data)
}

export async function getCurrentUser() {
  ensureApiUrl()
  const response = await axiosInstance.get(`${AUTH_API_PATH}/me`)
  return normalizeUser(getResponseData(response.data))
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
