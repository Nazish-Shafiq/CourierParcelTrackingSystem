import axiosInstance from './api/axiosInstance'
import { API_PATHS } from './api/apiConfig'
import { getResponseData } from './api/baseResponse'

export async function getParcelBookings({
  page = 1,
  pageSize = 10,
  search,
  filter,
  fromBookingDate,
  beforeBookingDate,
  signal,
} = {}) {
  const response = await axiosInstance.get(API_PATHS.parcelBookings, {
    params: {
      page,
      pageSize,
      search,
      filter,
      fromBookingDate,
      beforeBookingDate,
    },
    signal,
  })
  return getResponseData(response.data)
}

export async function getParcelBookingById(id, signal) {
  const response = await axiosInstance.get(`${API_PATHS.parcelBookings}/${id}`, { signal })
  return getResponseData(response.data)
}

export async function createParcelBooking(request) {
  const response = await axiosInstance.post(API_PATHS.parcelBookings, request)
  return getResponseData(response.data)
}
