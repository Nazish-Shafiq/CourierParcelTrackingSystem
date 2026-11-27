import axiosInstance from './api/axiosInstance'
import { API_PATHS } from './api/apiConfig'
import { getResponseData } from './api/baseResponse'

export async function getAuditLogs(query, signal) {
  const response = await axiosInstance.get(API_PATHS.auditLogs, {
    params: query,
    signal,
  })
  return getResponseData(response.data)
}
