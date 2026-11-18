import axiosInstance from './api/axiosInstance'
import { API_PATHS } from './api/apiConfig'
import { getResponseData } from './api/baseResponse'

export async function getHubTransfers({ page = 1, pageSize = 10, search, status, signal } = {}) {
  const response = await axiosInstance.get(API_PATHS.hubTransfers, {
    params: { page, pageSize, search, status },
    signal,
  })
  return getResponseData(response.data)
}

export async function getHubTransferOptions(signal) {
  const response = await axiosInstance.get(`${API_PATHS.hubTransfers}/options`, { signal })
  return getResponseData(response.data)
}

export async function getHubTransferById(id, signal) {
  const response = await axiosInstance.get(`${API_PATHS.hubTransfers}/${id}`, { signal })
  return getResponseData(response.data)
}

export async function createHubTransfer(request) {
  const response = await axiosInstance.post(API_PATHS.hubTransfers, request)
  return getResponseData(response.data)
}

export async function confirmHubTransferArrival(id, request = {}) {
  const response = await axiosInstance.put(
    `${API_PATHS.hubTransfers}/${id}/confirm-arrival`,
    request,
  )
  return getResponseData(response.data)
}
