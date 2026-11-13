export class ApiError extends Error {
  constructor(message, status, code) {
    super(message)
    this.name = 'ApiError'
    this.status = status
    this.code = code
  }
}

function findProperty(object, propertyName) {
  const key = Object.keys(object)
    .find((candidate) => candidate.toLowerCase() === propertyName.toLowerCase())

  return key === undefined
    ? { found: false }
    : { found: true, value: object[key] }
}

function getErrorMessage(responseBody) {
  if (!responseBody || typeof responseBody !== 'object') {
    return typeof responseBody === 'string' ? responseBody : undefined
  }

  return findProperty(responseBody, 'message').value
    || findProperty(responseBody, 'detail').value
    || findProperty(responseBody, 'title').value
}

export function getResponseData(responseBody) {
  if (responseBody == null) {
    return null
  }

  if (typeof responseBody !== 'object' || Array.isArray(responseBody)) {
    return responseBody
  }

  const success = findProperty(responseBody, 'success')
  const data = findProperty(responseBody, 'data')
  const message = getErrorMessage(responseBody)

  if (success.found) {
    if (success.value !== true) {
      throw new ApiError(message || 'The request was not successful.')
    }

    if (!data.found) {
      throw new ApiError(message || 'The server returned an invalid response.')
    }

    return data.value
  }

  const isDataEnvelope = data.found && Object.keys(responseBody)
    .every((key) => ['data', 'message'].includes(key.toLowerCase()))

  return isDataEnvelope ? data.value : responseBody
}

export function normalizeApiError(error) {
  if (error instanceof ApiError) return error

  const responseBody = error.response?.data
  const message = getErrorMessage(responseBody)
    || error.message
    || 'The API request failed.'
  return new ApiError(message, error.response?.status, error.code)
}
