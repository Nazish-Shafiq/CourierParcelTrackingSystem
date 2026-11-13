import axiosInstance from './api/axiosInstance'
import { API_PATHS } from './api/apiConfig'
import { getResponseData } from './api/baseResponse'

export async function getParcels(signal) {
  const response = await axiosInstance.get(API_PATHS.parcels, { signal })
  return getResponseData(response.data)
}

export async function getParcelById(id, signal) {
  const response = await axiosInstance.get(`${API_PATHS.parcels}/${id}`, { signal })
  return getResponseData(response.data)
}

export async function createParcel(request) {
  const response = await axiosInstance.post(API_PATHS.parcels, request)
  return getResponseData(response.data)
}
