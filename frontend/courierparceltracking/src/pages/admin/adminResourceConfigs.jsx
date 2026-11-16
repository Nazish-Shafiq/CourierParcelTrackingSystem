import StatusBadge from "../../components/common/StatusBadge";
import {
  getCouriers,
  getCourierById,
  getEligibleCourierUsers,
  createCourier,
  updateCourier,
  deleteCourier,
} from "../../services/courierService";
import {
  getCustomers,
  getCustomerById,
  createCustomer,
  updateCustomer,
  deleteCustomer,
} from "../../services/customerService";
import {
  getHubs,
  getHubById,
  createHub,
  updateHub,
  deleteHub,
} from "../../services/hubService";
import {
  getParcelById,
  getParcels,
  createParcel,
} from "../../services/parcelService";
import {
  getParcelStatusById,
  getParcelStatuses,
  createParcelStatus,
  updateParcelStatus,
  deleteParcelStatus,
} from "../../services/parcelStatusService";
import {
  getRouteById,
  getRoutes,
  createRoute,
  updateRoute,
  deleteRoute,
} from "../../services/routeService";
import { getUserById, getUsers } from "../../services/userService";
import {
  getParcelFormOptions,
  getRouteFormOptions,
} from "../../services/adminLookupService";

function formatDate(value) {
  return value ? new Date(value).toLocaleDateString() : "—";
}

function activeLabel(isActive) {
  return isActive ? "Active" : "Inactive";
}

async function loadCourierFormOptions(signal, courierId) {
  const users = await getEligibleCourierUsers(courierId, signal);
  return {
    eligibleCourierUsers: users.map((user) => ({
      value: user.userId,
      label: `${user.fullName} (${user.userName}) - ${user.email}`,
      searchText: [user.userName, user.fullName, user.email, user.phone]
        .filter(Boolean)
        .join(" "),
      fullName: user.fullName,
      phone: user.phone ?? "",
      email: user.email,
    })),
  };
}

export const usersConfig = {
  key: "users",
  title: "Users",
  singular: "user",
  description: "View active user accounts and their assigned role names.",
  clientSide: true,
  list: ({ signal }) => getUsers(signal),
  getById: getUserById,
  idField: "userId",
  searchFields: ["userName", "fullName", "email", "phone", "roles"],
  getDisplayName: (user) => user.fullName || user.userName,
  columns: [
    { key: "fullName", header: "Full name" },
    { key: "userName", header: "Username" },
    { key: "email", header: "Email" },
    {
      key: "roles",
      header: "Roles",
      render: (user) => user.roles.join(", ") || "—",
    },
    {
      key: "isActive",
      header: "Account",
      render: (user) => activeLabel(user.isActive),
    },
  ],
  detailFields: [
    { key: "userId", label: "User ID" },
    { key: "userName", label: "Username" },
    { key: "fullName", label: "Full name" },
    { key: "email", label: "Email" },
    { key: "phone", label: "Phone" },
    { key: "roles", label: "Roles" },
    { key: "isActive", label: "Active" },
    {
      key: "createdAt",
      label: "Created",
      value: (user) => formatDate(user.createdAt),
    },
  ],
};

export const customersConfig = {
  key: "customers",
  title: "Customers",
  singular: "customer",
  description: "Search, create, update, and deactivate customer records.",
  paged: true,
  list: getCustomers,
  getById: getCustomerById,
  create: createCustomer,
  update: updateCustomer,
  remove: deleteCustomer,
  idField: "customerId",
  getDisplayName: (customer) => customer.fullName || customer.customerCode,
  searchFields: [],
  columns: [
    { key: "customerCode", header: "Customer code" },
    { key: "fullName", header: "Full name" },
    { key: "phone", header: "Phone" },
    { key: "email", header: "Email" },
    { key: "city", header: "City" },
    {
      key: "isActive",
      header: "Account",
      render: (customer) => activeLabel(customer.isActive),
    },
  ],
  fields: [
    {
      name: "fullName",
      label: "Full name",
      required: true,
      minLength: 2,
      maxLength: 150,
    },
    {
      name: "phone",
      label: "Phone",
      required: true,
      minLength: 7,
      maxLength: 30,
      type: "tel",
    },
    {
      name: "email",
      label: "Email",
      type: "email",
      nullable: true,
      maxLength: 256,
    },
    { name: "addressLine", label: "Address", nullable: true, maxLength: 300 },
    { name: "city", label: "City", nullable: true, maxLength: 100 },
    { name: "province", label: "Province", nullable: true, maxLength: 100 },
    { name: "postalCode", label: "Postal code", nullable: true, maxLength: 20 },
  ],
  detailFields: [
    { key: "customerCode", label: "Customer code" },
    { key: "fullName", label: "Full name" },
    { key: "email", label: "Email" },
    { key: "phone", label: "Phone" },
    { key: "addressLine", label: "Address" },
    { key: "city", label: "City" },
    { key: "province", label: "Province" },
    { key: "postalCode", label: "Postal code" },
    { key: "isActive", label: "Active" },
    {
      key: "createdAt",
      label: "Created",
      value: (customer) => formatDate(customer.createdAt),
    },
  ],
};

export const couriersConfig = {
  key: "couriers",
  title: "Couriers",
  singular: "courier",
  description:
    "Add Courier profiles by linking existing Courier login accounts. Create new logins from Users.",
  paged: true,
  list: getCouriers,
  getById: getCourierById,
  create: createCourier,
  update: updateCourier,
  remove: deleteCourier,
  loadFormOptions: loadCourierFormOptions,
  idField: "courierId",
  getDisplayName: (courier) => courier.fullName || courier.employeeCode,
  columns: [
    { key: "employeeCode", header: "Employee code" },
    { key: "fullName", header: "Full name" },
    { key: "phone", header: "Phone" },
    { key: "vehicleType", header: "Vehicle" },
    {
      key: "isAvailable",
      header: "Availability",
      render: (courier) => (courier.isAvailable ? "Available" : "Unavailable"),
    },
    {
      key: "isActive",
      header: "Account",
      render: (courier) => activeLabel(courier.isActive),
    },
  ],
  fields: [
    {
      name: "userId",
      label: "Courier login account",
      type: "searchable-select",
      integer: true,
      nullable: true,
      numeric: true,
      requiredOnCreate: true,
      optionsKey: "eligibleCourierUsers",
      searchLabel: "Search Courier login accounts",
      placeholder: "Select an existing Courier login",
      emptyPlaceholder: "No eligible Courier accounts",
      helpText: "Choose an active Courier login account that has no profile.",
      emptyHelpText: "Create a Courier login account from Users first.",
      onSelect: (user) => ({
        fullName: user?.fullName ?? "",
        phone: user?.phone ?? "",
        email: user?.email ?? "",
      }),
    },
    {
      name: "fullName",
      label: "Full name",
      required: true,
      readOnly: true,
      minLength: 2,
      maxLength: 150,
    },
    {
      name: "phone",
      label: "Phone",
      required: true,
      minLength: 7,
      maxLength: 30,
      type: "tel",
    },
    {
      name: "email",
      label: "Email",
      type: "email",
      nullable: true,
      readOnly: true,
      maxLength: 256,
    },
    {
      name: "vehicleType",
      label: "Vehicle type",
      nullable: true,
      maxLength: 50,
    },
    {
      name: "vehicleNumber",
      label: "Vehicle number",
      nullable: true,
      maxLength: 30,
    },
    {
      name: "isAvailable",
      label: "Available for assignments",
      type: "checkbox",
      editOnly: true,
    },
    { name: "isActive", label: "Active", type: "checkbox", editOnly: true },
  ],
  detailFields: [
    { key: "employeeCode", label: "Employee code" },
    { key: "fullName", label: "Full name" },
    { key: "userId", label: "Linked user ID" },
    { key: "phone", label: "Phone" },
    { key: "email", label: "Email" },
    { key: "vehicleType", label: "Vehicle type" },
    { key: "vehicleNumber", label: "Vehicle number" },
    { key: "isAvailable", label: "Available" },
    { key: "isActive", label: "Active" },
    {
      key: "createdAt",
      label: "Created",
      value: (courier) => formatDate(courier.createdAt),
    },
  ],
};

export const hubsConfig = {
  key: "hubs",
  title: "Hubs",
  singular: "hub",
  description: "Manage operational hubs and their contact details.",
  paged: true,
  list: getHubs,
  getById: getHubById,
  create: createHub,
  update: updateHub,
  remove: deleteHub,
  idField: "hubId",
  getDisplayName: (hub) => hub.hubName || hub.hubCode,
  columns: [
    { key: "hubCode", header: "Hub code" },
    { key: "hubName", header: "Hub name" },
    { key: "city", header: "City" },
    { key: "country", header: "Country" },
    { key: "contactPhone", header: "Contact phone" },
    {
      key: "isActive",
      header: "Status",
      render: (hub) => activeLabel(hub.isActive),
    },
  ],
  fields: [
    {
      name: "hubName",
      label: "Hub name",
      required: true,
      minLength: 2,
      maxLength: 150,
    },
    { name: "addressLine", label: "Address", required: true, maxLength: 300 },
    { name: "city", label: "City", required: true, maxLength: 100 },
    { name: "province", label: "Province", nullable: true, maxLength: 100 },
    { name: "postalCode", label: "Postal code", nullable: true, maxLength: 20 },
    { name: "country", label: "Country", required: true, maxLength: 100 },
    {
      name: "contactPhone",
      label: "Contact phone",
      type: "tel",
      nullable: true,
      maxLength: 30,
    },
    { name: "isActive", label: "Active", type: "checkbox", editOnly: true },
  ],
  detailFields: [
    { key: "hubCode", label: "Hub code" },
    { key: "hubName", label: "Hub name" },
    { key: "addressLine", label: "Address" },
    { key: "city", label: "City" },
    { key: "province", label: "Province" },
    { key: "postalCode", label: "Postal code" },
    { key: "country", label: "Country" },
    { key: "contactPhone", label: "Contact phone" },
    { key: "isActive", label: "Active" },
    {
      key: "createdAt",
      label: "Created",
      value: (hub) => formatDate(hub.createdAt),
    },
  ],
};

export const parcelStatusesConfig = {
  key: "parcel-statuses",
  title: "Parcel Statuses",
  singular: "parcel status",
  description: "Manage the status names and ordering used by parcel tracking.",
  paged: true,
  list: getParcelStatuses,
  getById: getParcelStatusById,
  create: createParcelStatus,
  update: updateParcelStatus,
  remove: deleteParcelStatus,
  idField: "parcelStatusId",
  getDisplayName: (status) => status.statusName,
  columns: [
    { key: "statusCode", header: "Status code" },
    { key: "statusName", header: "Status name" },
    { key: "displayOrder", header: "Display order" },
    {
      key: "isTerminal",
      header: "Terminal",
      render: (status) => (status.isTerminal ? "Yes" : "No"),
    },
    {
      key: "isActive",
      header: "Status",
      render: (status) => activeLabel(status.isActive),
    },
  ],
  fields: [
    {
      name: "statusName",
      label: "Status name",
      required: true,
      maxLength: 100,
    },
    {
      name: "description",
      label: "Description",
      type: "textarea",
      nullable: true,
    },
    {
      name: "displayOrder",
      label: "Display order",
      type: "number",
      integer: true,
      required: true,
      min: 0,
      defaultValue: 0,
    },
    { name: "isTerminal", label: "Terminal status", type: "checkbox" },
    { name: "isActive", label: "Active", type: "checkbox", editOnly: true },
  ],
  detailFields: [
    { key: "statusCode", label: "Status code" },
    { key: "statusName", label: "Status name" },
    { key: "description", label: "Description" },
    { key: "displayOrder", label: "Display order" },
    { key: "isTerminal", label: "Terminal" },
    { key: "isActive", label: "Active" },
    {
      key: "createdAt",
      label: "Created",
      value: (status) => formatDate(status.createdAt),
    },
  ],
};

export const routesConfig = {
  key: "routes",
  title: "Routes",
  singular: "route",
  description: "Manage routes between active hubs.",
  paged: true,
  list: getRoutes,
  getById: getRouteById,
  create: createRoute,
  update: updateRoute,
  remove: deleteRoute,
  loadFormOptions: getRouteFormOptions,
  idField: "routeId",
  getDisplayName: (route) => route.routeName || route.routeCode,
  validate: (values) =>
    values.originHubId && values.originHubId === values.destinationHubId
      ? "Origin and destination hubs must be different."
      : null,
  columns: [
    { key: "routeCode", header: "Route code" },
    { key: "routeName", header: "Route name" },
    {
      key: "hubs",
      header: "Origin to destination",
      render: (route) =>
        `${route.originHubName} to ${route.destinationHubName}`,
    },
    {
      key: "distanceKm",
      header: "Distance (km)",
      render: (route) => route.distanceKm ?? "—",
    },
    {
      key: "estimatedHours",
      header: "Estimated hours",
      render: (route) => route.estimatedHours ?? "—",
    },
    {
      key: "isActive",
      header: "Status",
      render: (route) => activeLabel(route.isActive),
    },
  ],
  fields: [
    { name: "routeName", label: "Route name", required: true, maxLength: 100 },
    {
      name: "originHubId",
      label: "Origin hub",
      type: "select",
      required: true,
      numeric: true,
      optionsKey: "hubs",
    },
    {
      name: "destinationHubId",
      label: "Destination hub",
      type: "select",
      required: true,
      numeric: true,
      optionsKey: "hubs",
    },
    {
      name: "distanceKm",
      label: "Distance (km)",
      type: "number",
      nullable: true,
      min: 0,
      step: "any",
    },
    {
      name: "estimatedHours",
      label: "Estimated hours",
      type: "number",
      nullable: true,
      min: 0,
      step: "any",
    },
    {
      name: "description",
      label: "Description",
      type: "textarea",
      nullable: true,
    },
    { name: "isActive", label: "Active", type: "checkbox", editOnly: true },
  ],
  detailFields: [
    { key: "routeCode", label: "Route code" },
    { key: "routeName", label: "Route name" },
    { key: "originHubName", label: "Origin hub" },
    { key: "destinationHubName", label: "Destination hub" },
    { key: "distanceKm", label: "Distance (km)" },
    { key: "estimatedHours", label: "Estimated hours" },
    { key: "description", label: "Description" },
    { key: "isActive", label: "Active" },
    {
      key: "createdAt",
      label: "Created",
      value: (route) => formatDate(route.createdAt),
    },
  ],
};

export const parcelsConfig = {
  key: "parcels",
  title: "Parcels",
  singular: "parcel",
  description:
    "View parcels and create records using active customers, routes, and statuses.",
  clientSide: true,
  list: ({ signal }) => getParcels(signal),
  getById: getParcelById,
  create: createParcel,
  loadFormOptions: getParcelFormOptions,
  idField: "parcelId",
  getDisplayName: (parcel) => parcel.trackingNumber,
  searchFields: ["trackingNumber", "senderName", "routeName", "currentStatus"],
  columns: [
    { key: "trackingNumber", header: "Tracking number" },
    { key: "senderName", header: "Sender" },
    {
      key: "routeName",
      header: "Route",
      render: (parcel) => parcel.routeName || "—",
    },
    {
      key: "currentStatus",
      header: "Status",
      render: (parcel) => <StatusBadge status={parcel.currentStatus} />,
    },
    {
      key: "bookingDate",
      header: "Booked",
      render: (parcel) => formatDate(parcel.bookingDate),
    },
    {
      key: "expectedDeliveryDate",
      header: "Expected delivery",
      render: (parcel) => formatDate(parcel.expectedDeliveryDate),
    },
    { key: "totalWeightKg", header: "Weight (kg)" },
  ],
  fields: [
    {
      name: "senderCustomerId",
      label: "Sender customer",
      type: "select",
      required: true,
      numeric: true,
      optionsKey: "customers",
    },
    {
      name: "routeId",
      label: "Route (optional)",
      type: "select",
      nullable: true,
      numeric: true,
      optionsKey: "routes",
      placeholder: "No route",
    },
    {
      name: "currentStatusId",
      label: "Current status",
      type: "select",
      required: true,
      numeric: true,
      optionsKey: "statuses",
    },
    {
      name: "expectedDeliveryDate",
      label: "Expected delivery date",
      type: "date",
      nullable: true,
      min: new Date().toISOString().slice(0, 10),
    },
    {
      name: "totalWeightKg",
      label: "Total weight (kg)",
      type: "number",
      required: true,
      min: 0.001,
      max: 9999999,
      step: 0.001,
    },
    {
      name: "declaredValue",
      label: "Declared value",
      type: "number",
      nullable: true,
      min: 0,
      max: 9999999999,
      step: "any",
    },
    {
      name: "specialInstructions",
      label: "Special instructions",
      type: "textarea",
      nullable: true,
      maxLength: 500,
    },
  ],
  detailFields: [
    { key: "trackingNumber", label: "Tracking number" },
    { key: "senderName", label: "Sender" },
    { key: "routeName", label: "Route" },
    { key: "currentStatus", label: "Current status" },
    {
      key: "bookingDate",
      label: "Booking date",
      value: (parcel) => formatDate(parcel.bookingDate),
    },
    {
      key: "expectedDeliveryDate",
      label: "Expected delivery",
      value: (parcel) => formatDate(parcel.expectedDeliveryDate),
    },
    {
      key: "deliveredAt",
      label: "Delivered at",
      value: (parcel) => formatDate(parcel.deliveredAt),
    },
    { key: "totalWeightKg", label: "Weight (kg)" },
    { key: "declaredValue", label: "Declared value" },
    { key: "specialInstructions", label: "Special instructions" },
  ],
};
