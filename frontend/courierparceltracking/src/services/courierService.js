import axiosInstance from './api/axiosInstance'
import { API_PATHS } from './api/apiConfig'
import { getResponseData } from './api/baseResponse'

export async function getCouriers({ page = 1, pageSize = 10, search, signal } = {}) {
  const response = await axiosInstance.get(API_PATHS.couriers, {
    params: { page, pageSize, search },
    signal,
  })
  return getResponseData(response.data)
}

export async function getCourierById(id, signal) {
  const response = await axiosInstance.get(`${API_PATHS.couriers}/${id}`, { signal })
  return getResponseData(response.data)
}

export async function getEligibleCourierUsers(excludeCourierId, signal) {
  const response = await axiosInstance.get(`${API_PATHS.couriers}/eligible-users`, {
    params: { excludeCourierId },
    signal,
  })
  const users = getResponseData(response.data)
  if (!Array.isArray(users)) {
    throw new Error('The eligible Courier users response did not contain a user list.')
  }
  return users.map((user) => {
    if (
      !Number.isInteger(Number(user?.userId))
      || Number(user.userId) <= 0
      || typeof user?.userName !== 'string'
      || typeof user?.fullName !== 'string'
      || typeof user?.email !== 'string'
    ) {
      throw new Error('The eligible Courier users response contained an invalid account.')
    }
    return user
  })
}

export async function createCourier(request) {
  const response = await axiosInstance.post(API_PATHS.couriers, request)
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
