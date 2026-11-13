import axiosInstance from './api/axiosInstance'
import { API_PATHS } from './api/apiConfig'
import { getResponseData } from './api/baseResponse'

export async function getParcelStatuses({ page = 1, pageSize = 10, search, signal } = {}) {
  const response = await axiosInstance.get(API_PATHS.parcelStatuses, {
    params: { page, pageSize, search },
    signal,
  })
  return getResponseData(response.data)
}

export async function getParcelStatusById(id, signal) {
  const response = await axiosInstance.get(`${API_PATHS.parcelStatuses}/${id}`, { signal })
  return getResponseData(response.data)
}

export async function createParcelStatus(request) {
  const response = await axiosInstance.post(API_PATHS.parcelStatuses, request)
  return getResponseData(response.data)
}

export async function updateParcelStatus(id, request) {
  const response = await axiosInstance.put(`${API_PATHS.parcelStatuses}/${id}`, request)
  return getResponseData(response.data)
}

export async function deleteParcelStatus(id) {
  const response = await axiosInstance.delete(`${API_PATHS.parcelStatuses}/${id}`)
  return getResponseData(response.data)
}
