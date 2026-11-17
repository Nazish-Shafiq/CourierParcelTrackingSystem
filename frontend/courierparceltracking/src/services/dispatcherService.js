import { getActiveParcelAssignments } from './parcelAssignmentService'
import { getParcels } from './parcelService'
import { getDispatchCourierOptions } from './courierService'

async function getAllCouriers(signal) {
  const couriers = await getDispatchCourierOptions(signal)
  if (!Array.isArray(couriers)) {
    throw new Error('The courier dispatch options response was invalid.')
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
    courier.isAvailable
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
