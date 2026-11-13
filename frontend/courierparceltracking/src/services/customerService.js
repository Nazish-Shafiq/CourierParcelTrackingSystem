import axiosInstance from './api/axiosInstance'
import { API_PATHS } from './api/apiConfig'
import { getResponseData } from './api/baseResponse'

export async function getCustomers({ page = 1, pageSize = 10, search, signal } = {}) {
  const response = await axiosInstance.get(API_PATHS.customers, {
    params: { page, pageSize, search },
    signal,
  })
  return getResponseData(response.data)
}

export async function getCustomerById(id, signal) {
  const response = await axiosInstance.get(`${API_PATHS.customers}/${id}`, { signal })
  return getResponseData(response.data)
}

export async function createCustomer(request) {
  const response = await axiosInstance.post(API_PATHS.customers, request)
  return getResponseData(response.data)
}

export async function updateCustomer(id, request) {
  const response = await axiosInstance.put(`${API_PATHS.customers}/${id}`, request)
  return getResponseData(response.data)
}

export async function deleteCustomer(id) {
  const response = await axiosInstance.delete(`${API_PATHS.customers}/${id}`)
  return getResponseData(response.data)
}
