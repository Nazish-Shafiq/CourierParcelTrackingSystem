import axios from 'axios'
import { API_BASE_URL } from './apiConfig'
import { normalizeApiError } from './baseResponse'
import { clearAccessToken, getAccessToken } from '../../utils/tokenStorage'

const axiosInstance = axios.create({
  baseURL: API_BASE_URL || undefined,
  headers: {
    Accept: 'application/json',
    'Content-Type': 'application/json',
  },
})

axiosInstance.interceptors.request.use((config) => {
  const token = getAccessToken()
  if (token) {
    config.headers.set('Authorization', `Bearer ${token}`)
  }
  return config
})

axiosInstance.interceptors.response.use(
  (response) => response,
  (error) => {
    const apiError = normalizeApiError(error)
    if (apiError.status === 401 && getAccessToken()) {
      clearAccessToken()
      window.dispatchEvent(new Event('auth:unauthorized'))
    }
    return Promise.reject(apiError)
  },
)

export default axiosInstance
