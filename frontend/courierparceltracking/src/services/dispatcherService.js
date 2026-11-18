import { getActiveParcelAssignments } from './parcelAssignmentService'
import { getParcels } from './parcelService'
import { getCouriers, getDispatchCourierOptions } from './courierService'

function getUnassignedParcels(parcels, assignments) {
  const assignedParcelIds = new Set(
    assignments
      .filter((assignment) => assignment.unassignedAt == null)
      .map((assignment) => assignment.parcelId),
  )

  return parcels.filter((parcel) => !assignedParcelIds.has(parcel.parcelId))
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
  const [parcels, assignments] = await Promise.all([
    getParcels(signal),
    getActiveParcelAssignments(signal),
  ])

  if (!Array.isArray(parcels) || !Array.isArray(assignments)) {
    throw new Error('The parcel assignment lookup returned an invalid response.')
  }

  return {
    parcels: getUnassignedParcels(parcels, assignments),
  }
}

export async function getDispatcherAvailableCouriers(parcelId, signal) {
  const couriers = await getDispatchCourierOptions(parcelId, signal)
  if (!Array.isArray(couriers)) {
    throw new Error('The courier dispatch options response was invalid.')
  }
  return couriers
}

export async function getDispatcherCouriers(signal) {
  const [pagedCouriers, assignments] = await Promise.all([
    getCouriers({ page: 1, pageSize: 100, signal }),
    getActiveParcelAssignments(signal),
  ])

  if (!pagedCouriers || !Array.isArray(pagedCouriers.items)) {
    throw new Error('The courier directory response was invalid.')
  }
  if (!Array.isArray(assignments)) {
    throw new Error('The active assignments response was invalid.')
  }

  const assignedCourierIds = new Set(
    assignments
      .filter((assignment) => assignment.unassignedAt == null)
      .map((assignment) => assignment.courierId),
  )

  return pagedCouriers.items.map((courier) => ({
    ...courier,
    hasActiveAssignment: assignedCourierIds.has(courier.courierId),
  }))
}
