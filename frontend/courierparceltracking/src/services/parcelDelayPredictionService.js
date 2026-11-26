import axiosInstance from './api/axiosInstance'
import { API_PATHS } from './api/apiConfig'
import { getResponseData } from './api/baseResponse'

const predictionPath = API_PATHS.parcelDelayPredictions

export async function getDispatcherDelayPredictions(signal) {
  const response = await axiosInstance.get(`${predictionPath}/dispatcher`, { signal })
  return getResponseData(response.data)
}

export async function getAdminDelayModelStatus(signal) {
  const response = await axiosInstance.get(`${predictionPath}/admin/status`, { signal })
  return getResponseData(response.data)
}

export async function trainParcelDelayModel() {
  const response = await axiosInstance.post(`${predictionPath}/admin/train`)
  return getResponseData(response.data)
}
