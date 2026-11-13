import axiosInstance from './api/axiosInstance'
import { API_PATHS } from './api/apiConfig'
import { getResponseData } from './api/baseResponse'

async function getDashboard(path, signal) {
  const response = await axiosInstance.get(`${API_PATHS.dashboard}/${path}`, { signal })
  return getResponseData(response.data)
}

export function getAdminDashboard(signal) {
  return getDashboard('admin', signal)
}

export function getDispatcherDashboard(signal) {
  return getDashboard('dispatcher', signal)
}

export function getCourierDashboard(signal) {
  return getDashboard('courier', signal)
}

export { getCourierPerformanceReport } from './reportService'
