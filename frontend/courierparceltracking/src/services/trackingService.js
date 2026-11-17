import axiosInstance from './api/axiosInstance'
import { API_PATHS } from './api/apiConfig'
import { getResponseData } from './api/baseResponse'

export async function getParcelTrackingEvents({ page = 1, pageSize = 10, signal } = {}) {
  const response = await axiosInstance.get(API_PATHS.parcelTracking, {
    params: { page, pageSize },
    signal,
  })
  return getResponseData(response.data)
}

export async function getTrackingHistory(trackingNumber, signal) {
  const response = await axiosInstance.get(
    `${API_PATHS.parcelTracking}/${encodeURIComponent(trackingNumber)}`,
    { signal },
  )
  return getResponseData(response.data)
}

export async function createTrackingEvent(request) {
  const response = await axiosInstance.post(API_PATHS.parcelTracking, request)
  return getResponseData(response.data)
}

export async function updateParcelTrackingStatus(parcelId, request) {
  const response = await axiosInstance.put(
    `${API_PATHS.parcelTracking}/${parcelId}/status`,
    request,
  )
  return getResponseData(response.data)
}

export async function updateCourierParcelStatus(parcelId, request) {
  const response = await axiosInstance.put(
    `${API_PATHS.parcelTracking}/${parcelId}/courier-transition`,
    request,
  )
  return getResponseData(response.data)
}
