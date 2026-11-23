import {
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
  const pagedCouriers = await getCouriers({
    page: 1,
    pageSize: 100,
    isActive: true,
    signal,
  })

  if (!pagedCouriers || !Array.isArray(pagedCouriers.items)) {
    throw new Error('The courier directory response was invalid.')
  }
  return pagedCouriers.items
}
