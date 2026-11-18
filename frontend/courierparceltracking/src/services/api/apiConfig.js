const configuredApiUrl = import.meta.env.VITE_API_URL?.trim()

export const API_BASE_URL = configuredApiUrl
  ? configuredApiUrl.replace(/\/+$/, '')
  : ''

export const API_PATHS = Object.freeze({
  auth: '/Auth',
  users: '/users',
  couriers: '/couriers',
  customers: '/customers',
  hubs: '/hubs',
  parcels: '/parcels',
  parcelStatuses: '/ParcelStatus',
  parcelBookings: '/ParcelBooking',
  parcelAssignments: '/parcel-assignments',
  hubTransfers: '/hub-transfers',
  parcelTracking: '/parcel-tracking',
  routes: '/Route',
  dashboard: '/dashboard',
  customerDashboard: '/customer-dashboard',
  reports: '/reports',
})

export const AUTH_API_PATH = API_PATHS.auth
