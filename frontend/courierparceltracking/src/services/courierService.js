import axiosInstance from './api/axiosInstance'
import { API_PATHS } from './api/apiConfig'
import { getResponseData } from './api/baseResponse'

export async function getCouriers({
  page = 1,
  pageSize = 10,
  search,
  isAvailable,
  isActive,
  signal,
} = {}) {
  const response = await axiosInstance.get(API_PATHS.couriers, {
    params: { page, pageSize, search, isAvailable, isActive },
    signal,
  })
  return getResponseData(response.data)
}

export async function getDispatchCourierOptions(parcelId, signal) {
  const parsedParcelId = Number(parcelId)
  if (!Number.isFinite(parsedParcelId) || parsedParcelId <= 0) {
    throw new Error('A valid parcel must be selected to load eligible couriers.')
  }

  const response = await axiosInstance.get(`${API_PATHS.couriers}/dispatch-options`, {
    params: { parcelId: parsedParcelId },
    signal,
  })
  return getResponseData(response.data)
}

export async function getCourierById(id, signal) {
  const response = await axiosInstance.get(`${API_PATHS.couriers}/${id}`, { signal })
  return getResponseData(response.data)
}

export async function updateCourier(id, request) {
  const response = await axiosInstance.put(`${API_PATHS.couriers}/${id}`, request)
  return getResponseData(response.data)
}

export async function deleteCourier(id) {
  const response = await axiosInstance.delete(`${API_PATHS.couriers}/${id}`)
  return getResponseData(response.data)
}
