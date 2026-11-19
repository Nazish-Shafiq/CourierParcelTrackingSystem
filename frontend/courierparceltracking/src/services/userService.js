import axiosInstance from './api/axiosInstance'
import { API_PATHS } from './api/apiConfig'
import { getResponseData } from './api/baseResponse'

export async function getUsers(signal) {
  const response = await axiosInstance.get(API_PATHS.users, { signal })
  return getResponseData(response.data)
}

export async function getUserById(id, signal) {
  const response = await axiosInstance.get(`${API_PATHS.users}/${id}`, { signal })
  return getResponseData(response.data)
}

export async function updateUser(
  id,
  { userName, email, fullName, phone, roleId, isActive },
) {
  const response = await axiosInstance.put(`${API_PATHS.users}/${id}`, {
    userName,
    email,
    fullName,
    phone,
    roleId,
    isActive,
  })
  return getResponseData(response.data)
}

export async function deleteUser(id) {
  const response = await axiosInstance.delete(`${API_PATHS.users}/${id}`)
  return getResponseData(response.data)
}
