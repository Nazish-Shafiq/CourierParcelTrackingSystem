import {
  BarChart3,
  Boxes,
  ClipboardList,
  Clock3,
  FileBarChart,
  MapPinned,
  Package,
  PackageCheck,
  PackagePlus,
  Route,
  Truck,
  Users,
  Warehouse,
} from 'lucide-react'

export const ROLE_NAMES = {
  ADMIN: 'Admin',
  BOOKING_AGENT: 'Booking Agent',
  DISPATCHER: 'Dispatcher',
  COURIER: 'Courier',
  CUSTOMER_VIEWER: 'Customer Viewer',
}

export const ROLE_DASHBOARD_PATHS = {
  [ROLE_NAMES.ADMIN]: '/dashboard/admin',
  [ROLE_NAMES.BOOKING_AGENT]: '/dashboard/booking-agent',
  [ROLE_NAMES.DISPATCHER]: '/dashboard/dispatcher',
  [ROLE_NAMES.COURIER]: '/dashboard/courier',
  [ROLE_NAMES.CUSTOMER_VIEWER]: '/dashboard/customer',
}

const dashboardItem = {
  label: 'Dashboard',
  path: '/',
  icon: BarChart3,
  allowedRoles: Object.values(ROLE_NAMES),
}

export function getDashboardPathForRoles(roles = []) {
  const roleNames = new Set(roles.map((role) => role.toLowerCase()))
  const matchingRole = Object.keys(ROLE_DASHBOARD_PATHS)
    .find((role) => roleNames.has(role.toLowerCase()))

  return matchingRole ? ROLE_DASHBOARD_PATHS[matchingRole] : null
}

export const ROLE_NAVIGATION = {
  [ROLE_NAMES.ADMIN]: [
    { label: 'Users', path: '/admin/users', icon: Users },
    { label: 'Customers', path: '/admin/customers', icon: Users },
    { label: 'Couriers', path: '/admin/couriers', icon: Truck },
    { label: 'Hubs', path: '/admin/hubs', icon: Warehouse },
    { label: 'Parcels', path: '/admin/parcels', icon: Package },
    { label: 'Parcel Status', path: '/admin/parcel-status', icon: PackageCheck },
    { label: 'Routes', path: '/admin/routes', icon: Route },
    { label: 'Reports', path: '/admin/reports', icon: FileBarChart },
  ],
  [ROLE_NAMES.BOOKING_AGENT]: [
    { label: 'Book Parcel', path: '/booking/book-parcel', icon: PackagePlus },
    { label: 'Parcel Bookings', path: '/booking/bookings', icon: ClipboardList },
    { label: 'Parcels', path: '/booking/parcels', icon: Package },
    { label: 'Customers', path: '/booking/customers', icon: Users },
  ],
  [ROLE_NAMES.DISPATCHER]: [
    { label: 'Parcels', path: '/dispatch/parcels', icon: Package },
    { label: 'Parcel Assignments', path: '/dispatch/assignments', icon: ClipboardList },
    { label: 'Routes', path: '/dispatch/routes', icon: Route },
    { label: 'Hubs', path: '/dispatch/hubs', icon: Warehouse },
    { label: 'Tracking', path: '/dispatch/tracking', icon: MapPinned },
  ],
  [ROLE_NAMES.COURIER]: [
    { label: 'Assigned Parcels', path: '/courier/assigned-parcels', icon: Boxes },
    { label: 'Parcel Details', path: '/courier/parcel-details', icon: Package },
    { label: 'Update Parcel Status', path: '/courier/update-status', icon: PackageCheck },
    { label: 'Delivery History', path: '/courier/delivery-history', icon: Clock3 },
  ],
  [ROLE_NAMES.CUSTOMER_VIEWER]: [
    { label: 'My Parcels', path: '/customer/parcels', icon: Package },
    { label: 'Track Parcel', path: '/customer/track-parcel', icon: MapPinned },
    { label: 'Tracking History', path: '/customer/tracking-history', icon: Clock3 },
  ],
}

export const NAVIGATION_ITEMS = Object.entries(ROLE_NAVIGATION).flatMap(
  ([role, items]) => items.map((item) => ({ ...item, allowedRoles: [role] })),
)

export function getNavigationForRoles(roles = []) {
  const roleNames = new Set(roles.map((role) => role.toLowerCase()))
  const roleItems = Object.entries(ROLE_NAVIGATION)
    .filter(([role]) => roleNames.has(role.toLowerCase()))
    .flatMap(([, items]) => items)
  const dashboardPath = getDashboardPathForRoles(roles)

  return [{ ...dashboardItem, path: dashboardPath || dashboardItem.path }, ...roleItems]
}

export function getNavigationItem(pathname) {
  if (pathname === dashboardItem.path || Object.values(ROLE_DASHBOARD_PATHS).includes(pathname)) {
    return dashboardItem
  }
  return NAVIGATION_ITEMS.find((item) => item.path === pathname)
}
