import { getActiveParcelAssignments } from './parcelAssignmentService'
import { getParcels } from './parcelService'
import { getCouriers } from './courierService'

async function getAllCouriers(signal) {
  const firstPage = await getCouriers({ page: 1, pageSize: 100, signal })
  if (!firstPage || !Array.isArray(firstPage.items) || !Number.isInteger(firstPage.totalPages)) {
    throw new Error('The courier response did not contain valid pagination data.')
  }

  const couriers = [...firstPage.items]
  for (let page = 2; page <= firstPage.totalPages; page += 1) {
    const result = await getCouriers({ page, pageSize: 100, signal })
    if (!result || !Array.isArray(result.items)) {
      throw new Error(`The courier response for page ${page} was invalid.`)
    }
    couriers.push(...result.items)
  }

  return couriers
}

function getUnassignedParcels(parcels, assignments) {
  const assignedParcelIds = new Set(
    assignments
      .filter((assignment) => assignment.unassignedAt == null)
      .map((assignment) => assignment.parcelId),
  )

  return parcels.filter((parcel) => !assignedParcelIds.has(parcel.parcelId))
}

function getAvailableCouriers(couriers, assignments) {
  const assignedCourierIds = new Set(
    assignments
      .filter((assignment) => assignment.unassignedAt == null)
      .map((assignment) => assignment.courierId),
  )

  return couriers.filter((courier) => (
    courier.isActive
    && courier.isAvailable
    && !assignedCourierIds.has(courier.courierId)
  ))
}

export async function getDispatcherUnassignedParcels(signal) {
  const [parcels, assignments] = await Promise.all([
    getParcels(signal),
    getActiveParcelAssignments(signal),
  ])

  if (!Array.isArray(parcels) || !Array.isArray(assignments)) {
    throw new Error('The parcel assignment lookup returned an invalid response.')
  }

  return getUnassignedParcels(parcels, assignments)
}

export async function getDispatcherAssignmentOptions(signal) {
  const [parcels, assignments, couriers] = await Promise.all([
    getParcels(signal),
    getActiveParcelAssignments(signal),
    getAllCouriers(signal),
  ])

  if (!Array.isArray(parcels) || !Array.isArray(assignments)) {
    throw new Error('The parcel assignment lookup returned an invalid response.')
  }

  return {
    parcels: getUnassignedParcels(parcels, assignments),
    couriers: getAvailableCouriers(couriers, assignments),
  }
}

export async function getDispatcherAvailableCouriers(signal) {
  const [couriers, assignments] = await Promise.all([
    getAllCouriers(signal),
    getActiveParcelAssignments(signal),
  ])

  if (!Array.isArray(assignments)) {
    throw new Error('The active assignments response was invalid.')
  }

  return getAvailableCouriers(couriers, assignments)
}

export async function getDispatcherCouriers(signal) {
  const [couriers, assignments] = await Promise.all([
    getAllCouriers(signal),
    getActiveParcelAssignments(signal),
  ])

  if (!Array.isArray(assignments)) {
    throw new Error('The active assignments response was invalid.')
  }

  const assignedCourierIds = new Set(
    assignments
      .filter((assignment) => assignment.unassignedAt == null)
      .map((assignment) => assignment.courierId),
  )

  return couriers.map((courier) => ({
    ...courier,
    hasActiveAssignment: assignedCourierIds.has(courier.courierId),
  }))
}
