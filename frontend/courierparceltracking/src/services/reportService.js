import axiosInstance from './api/axiosInstance'
import { API_PATHS } from './api/apiConfig'
import { getResponseData } from './api/baseResponse'

async function getReport(path, { fromDate, toDate, signal } = {}) {
  const response = await axiosInstance.get(`${API_PATHS.reports}/${path}`, {
    params: { fromDate, toDate },
    signal,
  })
  return getResponseData(response.data)
}

export function getParcelBookingsReport({ fromDate, toDate, signal } = {}) {
  return getReport('parcel-bookings', { fromDate, toDate, signal })
}

export function getParcelStatusReport({ fromDate, toDate, signal } = {}) {
  return getReport('parcel-status', { fromDate, toDate, signal })
}

export function getCourierPerformanceReport({ fromDate, toDate, signal } = {}) {
  return getReport('courier-performance', { fromDate, toDate, signal })
}
