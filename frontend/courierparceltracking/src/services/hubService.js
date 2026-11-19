import axiosInstance from './api/axiosInstance'
import { API_PATHS } from './api/apiConfig'
import { getResponseData } from './api/baseResponse'

export async function getHubs({
  page = 1,
  pageSize = 10,
  search,
  activeOnly,
  isActive,
  signal,
} = {}) {
  const response = await axiosInstance.get(API_PATHS.hubs, {
    params: { page, pageSize, search, activeOnly, isActive },
    signal,
  })
  return getResponseData(response.data)
}

export async function getHubById(id, signal) {
  const response = await axiosInstance.get(`${API_PATHS.hubs}/${id}`, { signal })
  return getResponseData(response.data)
}

export async function createHub(request) {
  const response = await axiosInstance.post(API_PATHS.hubs, request)
  return getResponseData(response.data)
}

export async function updateHub(id, request) {
  const response = await axiosInstance.put(`${API_PATHS.hubs}/${id}`, request)
  return getResponseData(response.data)
}

export async function deleteHub(id) {
  const response = await axiosInstance.delete(`${API_PATHS.hubs}/${id}`)
  return getResponseData(response.data)
}
