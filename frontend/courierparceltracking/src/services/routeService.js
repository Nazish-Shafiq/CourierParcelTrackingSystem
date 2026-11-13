import axiosInstance from './api/axiosInstance'
import { API_PATHS } from './api/apiConfig'
import { getResponseData } from './api/baseResponse'

export async function getRoutes({ page = 1, pageSize = 10, search, signal } = {}) {
  const response = await axiosInstance.get(API_PATHS.routes, {
    params: { page, pageSize, search },
    signal,
  })
  return getResponseData(response.data)
}

export async function getRouteById(id, signal) {
  const response = await axiosInstance.get(`${API_PATHS.routes}/${id}`, { signal })
  return getResponseData(response.data)
}

export async function createRoute(request) {
  const response = await axiosInstance.post(API_PATHS.routes, request)
  return getResponseData(response.data)
}

export async function updateRoute(id, request) {
  const response = await axiosInstance.put(`${API_PATHS.routes}/${id}`, request)
  return getResponseData(response.data)
}

export async function deleteRoute(id) {
  const response = await axiosInstance.delete(`${API_PATHS.routes}/${id}`)
  return getResponseData(response.data)
}
