import axiosInstance from './api/axiosInstance'
import { API_PATHS } from './api/apiConfig'
import { getResponseData } from './api/baseResponse'

const assignmentPath = API_PATHS.screenAssignments

async function getData(path, signal) {
  const response = await axiosInstance.get(path, { signal })
  return getResponseData(response.data)
}

async function getList(path, signal, label) {
  const data = await getData(path, signal)
  if (!Array.isArray(data)) {
    throw new Error(`The API response did not contain a ${label} list.`)
  }
  return data
}

export function getMyAssignedScreens(signal) {
  return getData(`${assignmentPath}/mine`, signal)
}

export function getActiveScreens(signal) {
  return getList(`${assignmentPath}/screens`, signal, 'screen')
}

export function getEligibleScreenUsers(screenId, signal) {
  return getList(
    `${assignmentPath}/eligible-users?screenId=${screenId}`,
    signal,
    'eligible user',
  )
}

export function getScreenAssignments(signal) {
  return getData(assignmentPath, signal)
}

export async function assignScreen({ screenId, userId }) {
  const response = await axiosInstance.post(assignmentPath, { screenId, userId })
  return getResponseData(response.data)
}

export async function reassignScreen(permissionId, userId) {
  const response = await axiosInstance.put(`${assignmentPath}/${permissionId}`, { userId })
  return getResponseData(response.data)
}

export async function revokeScreenAssignment(permissionId) {
  const response = await axiosInstance.delete(`${assignmentPath}/${permissionId}`)
  return getResponseData(response.data)
}
