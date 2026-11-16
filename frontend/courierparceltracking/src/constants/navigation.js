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
} from "lucide-react";
import { hasAnyRole, normalizeRoleName } from "../utils/roleAccess.js";

export const ROLE_NAMES = {
  ADMIN: "Admin",
  BOOKING_AGENT: "Booking Agent",
  DISPATCHER: "Dispatcher",
  COURIER: "Courier",
  CUSTOMER_VIEWER: "Customer Viewer",
};

export const ROLE_DASHBOARD_PATHS = {
  [ROLE_NAMES.ADMIN]: "/dashboard/admin",
  [ROLE_NAMES.BOOKING_AGENT]: "/dashboard/booking-agent",
  [ROLE_NAMES.DISPATCHER]: "/dashboard/dispatcher",
  [ROLE_NAMES.COURIER]: "/dashboard/courier",
  [ROLE_NAMES.CUSTOMER_VIEWER]: "/dashboard/customer",
};

const dashboardItem = {
  label: "Dashboard",
  path: "/",
  icon: BarChart3,
  allowedRoles: Object.values(ROLE_NAMES),
};

export function getDashboardPathForRoles(roles = []) {
  const roleNames = new Set(roles.map(normalizeRoleName).filter(Boolean));
  const matchingRole = Object.keys(ROLE_DASHBOARD_PATHS).find((role) =>
    roleNames.has(normalizeRoleName(role)),
  );

  return matchingRole ? ROLE_DASHBOARD_PATHS[matchingRole] : null;
}

export const ROLE_NAVIGATION = {
  [ROLE_NAMES.ADMIN]: [
    { label: "Users", path: "/admin/users", icon: Users },
    { label: "Customers", path: "/admin/customers", icon: Users },
    { label: "Couriers", path: "/admin/couriers", icon: Truck },
    { label: "Hubs", path: "/admin/hubs", icon: Warehouse },
    { label: "Parcels", path: "/admin/parcels", icon: Package },
    {
      label: "Parcel Status",
      path: "/admin/parcel-status",
      icon: PackageCheck,
    },
    { label: "Routes", path: "/admin/routes", icon: Route },
    { label: "Reports", path: "/admin/reports", icon: FileBarChart },
  ],
  [ROLE_NAMES.BOOKING_AGENT]: [
    {
      label: "Register Customer",
      path: "/booking/register-customer",
      icon: Users,
    },
    { label: "Customers", path: "/booking/customers", icon: Users },
    { label: "Book Parcel", path: "/booking/book-parcel", icon: PackagePlus },
    {
      label: "Parcel Bookings",
      path: "/booking/bookings",
      icon: ClipboardList,
    },
    { label: "Track Parcel", path: "/booking/track-parcel", icon: MapPinned },
  ],
  [ROLE_NAMES.DISPATCHER]: [
    { label: "Parcels", path: "/dispatch/parcels", icon: Package },
    {
      label: "Unassigned Parcels",
      path: "/dispatch/unassigned-parcels",
      icon: PackageCheck,
    },
    { label: "Assign Courier", path: "/dispatch/assign", icon: PackagePlus },
    {
      label: "Parcel Assignments",
      path: "/dispatch/assignments",
      icon: ClipboardList,
    },
    { label: "Routes", path: "/dispatch/routes", icon: Route },
    { label: "Couriers", path: "/dispatch/couriers", icon: Truck },
    { label: "Hubs", path: "/dispatch/hubs", icon: Warehouse },
    { label: "Tracking", path: "/dispatch/tracking", icon: MapPinned },
  ],
  [ROLE_NAMES.COURIER]: [
    { label: "My Assigned Parcels", path: "/courier/assigned-parcels", icon: Boxes },
  ],
  [ROLE_NAMES.CUSTOMER_VIEWER]: [
    { label: "My Parcels", path: "/customer/parcels", icon: Package },
    { label: "Track Parcel", path: "/customer/track-parcel", icon: MapPinned },
    {
      label: "Tracking History",
      path: "/customer/tracking-history",
      icon: Clock3,
    },
  ],
};

export const NAVIGATION_ITEMS = Object.entries(ROLE_NAVIGATION).flatMap(
  ([role, items]) => items.map((item) => ({ ...item, allowedRoles: [role] })),
);

export function getPostLoginRedirect(roles, fromLocation) {
  const dashboardPath = getDashboardPathForRoles(roles);
  if (!dashboardPath) return "/access-denied";

  const requestedPath = fromLocation?.pathname;
  if (!requestedPath || requestedPath === "/") return dashboardPath;

  const dashboardRole = Object.entries(ROLE_DASHBOARD_PATHS)
    .find(([, path]) => path === requestedPath)?.[0];
  const navigationItem = NAVIGATION_ITEMS.find((item) => (
    item.path === requestedPath || requestedPath.startsWith(`${item.path}/`)
  ));
  const requiredRoles = dashboardRole
    ? [dashboardRole]
    : navigationItem?.allowedRoles;

  if (requiredRoles && hasAnyRole(roles, requiredRoles)) {
    return {
      pathname: requestedPath,
      search: fromLocation.search ?? "",
      hash: fromLocation.hash ?? "",
    };
  }

  return dashboardPath;
}

export function getNavigationForRoles(roles = []) {
  const roleItems = Object.entries(ROLE_NAVIGATION)
    .filter(([role]) => hasAnyRole(roles, [role]))
    .flatMap(([, items]) => items);
  const dashboardPath = getDashboardPathForRoles(roles);

  return [
    { ...dashboardItem, path: dashboardPath || dashboardItem.path },
    ...roleItems,
  ];
}

export function getNavigationItem(pathname) {
  if (
    pathname === dashboardItem.path ||
    Object.values(ROLE_DASHBOARD_PATHS).includes(pathname)
  ) {
    return dashboardItem;
  }
  if (pathname.startsWith("/courier/assigned-parcels/")) {
    return NAVIGATION_ITEMS.find((item) => item.path === "/courier/assigned-parcels");
  }
  return NAVIGATION_ITEMS.find((item) => item.path === pathname);
}
