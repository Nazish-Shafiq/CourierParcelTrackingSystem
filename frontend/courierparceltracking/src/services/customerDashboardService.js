import axiosInstance from './api/axiosInstance'
import { API_PATHS } from './api/apiConfig'
import { getResponseData } from './api/baseResponse'

export async function getCustomerDashboardStats(signal) {
  const response = await axiosInstance.get(`${API_PATHS.customerDashboard}/stats`, { signal })
  return getResponseData(response.data)
}

export async function getCustomerParcels({
  page = 1,
  pageSize = 10,
  search,
  statusCode,
  signal,
} = {}) {
  const response = await axiosInstance.get(`${API_PATHS.customerDashboard}/parcels`, {
    params: { page, pageSize, search, statusCode },
    signal,
  })
  return getResponseData(response.data)
}

export async function getCustomerParcelDetails(parcelId, signal) {
  const response = await axiosInstance.get(
    `${API_PATHS.customerDashboard}/parcels/${parcelId}`,
    { signal },
  )
  return getResponseData(response.data)
}
