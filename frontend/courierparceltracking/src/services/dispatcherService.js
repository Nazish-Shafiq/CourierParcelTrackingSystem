import {
  getActiveParcelAssignments,
  getEligibleParcelsForInitialAssignment,
} from './parcelAssignmentService'
import { getCouriers, getDispatchCourierOptions } from './courierService'

export async function getDispatcherUnassignedParcels(signal) {
  const parcels = await getEligibleParcelsForInitialAssignment(signal)
  if (!Array.isArray(parcels)) {
    throw new Error('The eligible parcel lookup returned an invalid response.')
  }
  return parcels
}

export async function getDispatcherAssignmentOptions(signal) {
  const parcels = await getEligibleParcelsForInitialAssignment(signal)
  if (!Array.isArray(parcels)) {
    throw new Error('The eligible parcel lookup returned an invalid response.')
  }

  return {
    parcels,
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
    getCouriers({ page: 1, pageSize: 100, isActive: true, signal }),
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
