import axiosInstance from './api/axiosInstance'
import { API_PATHS } from './api/apiConfig'
import { getResponseData } from './api/baseResponse'

export async function getParcelAssignments({ page = 1, pageSize = 10, signal } = {}) {
  const response = await axiosInstance.get(API_PATHS.parcelAssignments, {
    params: { page, pageSize },
    signal,
  })
  return getResponseData(response.data)
}

export async function getActiveParcelAssignments(signal) {
  const response = await axiosInstance.get(`${API_PATHS.parcelAssignments}/active`, { signal })
  return getResponseData(response.data)
}

export async function getParcelAssignmentById(id, signal) {
  const response = await axiosInstance.get(`${API_PATHS.parcelAssignments}/${id}`, { signal })
  return getResponseData(response.data)
}

export async function assignParcel(request) {
  const response = await axiosInstance.post(API_PATHS.parcelAssignments, request)
  return getResponseData(response.data)
}

export async function reassignParcel(id, request) {
  const response = await axiosInstance.put(
    `${API_PATHS.parcelAssignments}/${id}/reassign`,
    request,
  )
  return getResponseData(response.data)
}

export async function unassignParcel(id) {
  const response = await axiosInstance.put(`${API_PATHS.parcelAssignments}/${id}/unassign`)
  return getResponseData(response.data)
}
