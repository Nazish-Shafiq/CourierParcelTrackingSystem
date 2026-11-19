# Backend API Mapping

This reference reflects the controllers and DTOs in
`backend/CourierParcelTrackingSystemAPI`. The API base path is `/api`; protected
requests use `Authorization: Bearer <token>`.

## Response conventions

Most successful controller actions return `ApiResponse<T>`:

```json
{
  "success": true,
  "message": "…",
  "data": {}
}
```

`data` may be an object, an array, a paged result, a string, a boolean, or null.
Paged results contain `items`, `page`, `pageSize`, `totalRecords`, and
`totalPages`. ASP.NET Core's web JSON defaults serialize DTO properties as
camelCase. Controller failures commonly use the same envelope; validation and
authentication middleware may return ProblemDetails or an empty 401 instead.

## Endpoint map

| Module         | Method and endpoint                          | Query / route parameters              | Request body                    | Response data                                                          | Authorization                                             |
| -------------- | -------------------------------------------- | ------------------------------------- | ------------------------------- | ---------------------------------------------------------------------- | --------------------------------------------------------- |
| Auth           | `POST /api/Auth/bootstrap-admin`             | —                                     | `BootstrapAdminRequestDto`      | `string`                                                               | Anonymous; bootstrap key and no-active-admin checks apply |
| Auth           | `POST /api/Auth/login`                       | —                                     | `LoginRequestDto`               | `LoginResponseDto`                                                     | Anonymous                                                 |
| Auth           | `GET /api/Auth/staff-roles`                  | —                                     | —                               | Active assignable staff roles (`roleId`, `roleName`)                   | Admin                                                     |
| Auth           | `GET /api/Auth/roles`                        | —                                     | —                               | All active roles (`roleId`, `roleName`) for user role edits             | Admin                                                     |
| Auth           | `POST /api/Auth/register-staff`              | —                                     | `RegisterStaffRequestDto`       | `string`                                                               | Admin; Courier requires an active `hubId`, saved to its linked profile |
| Auth           | `POST /api/Auth/register-customer`           | —                                     | `RegisterCustomerRequestDto`    | `string`                                                               | Booking Agent                                             |
| Auth           | `POST /api/Auth/logout`                      | —                                     | —                               | `string`                                                               | Authenticated                                             |
| Auth           | `GET /api/Auth/me`                           | —                                     | —                               | Current-user fields: userId, userName, fullName, email, roles, roleIds | Authenticated                                             |
| Users          | `GET /api/users`                             | —                                     | —                               | Non-deleted `UserResponseDto[]`, including inactive accounts           | Admin                                                     |
| Users          | `GET /api/users/{id}`                        | `id: int`                             | —                               | Non-deleted `UserResponseDto`, including inactive accounts; 404 when absent | Admin                                                |
| Users          | `PUT /api/users/{id}`                        | `id: int`                             | `UpdateUserRequestDto` (`userName`, `email`, `fullName`, `phone?`, `roleId`, `isActive`) | Updated `UserResponseDto`; password hash is preserved | Admin                                      |
| Users          | `DELETE /api/users/{id}`                     | `id: int`                             | —                               | `bool`; soft-deactivates the account                                   | Admin; self, final active Admin, and active-assignment Courier protections apply |
| Customer dashboard | `GET /api/customer-dashboard/stats`       | —                                     | —                               | `CustomerDashboardSummaryDto`; parcel counts for the JWT-linked customer | Customer Viewer; user ID is read from the JWT            |
| Customer dashboard | `GET /api/customer-dashboard/parcels`     | `page`, `pageSize`, optional `search`, `statusCode` | —                 | `PagedResponseDto<CustomerParcelDto>`                                  | Customer Viewer; results scoped to the JWT-linked customer |
| Customer dashboard | `GET /api/customer-dashboard/parcels/{parcelId}` | `parcelId: long`                | —                               | `CustomerParcelDetailsDto` with chronological tracking history and origin/destination hub names; 404 when not owned | Customer Viewer; results scoped to the JWT-linked customer |
| Customer dashboard | `GET /api/customer-dashboard/track/{trackingNumber}` | `trackingNumber: string` | — | `CustomerParcelDetailsDto` with current parcel details and complete tracking history; 404 when not owned | Customer Viewer; results scoped to the JWT-linked customer |
| Couriers       | `GET /api/couriers`                          | `page`, `pageSize`, optional `search`, `isAvailable`, `isActive` | —                | `PagedResponseDto<CourierResponseDto>` including assigned hub name, code, city, and province | Admin, Dispatcher                      |
| Couriers       | `GET /api/couriers/dispatch-options`         | required `parcelId: long`             | —                               | `CourierDispatchOptionDto[]`; active, available profiles assigned to the parcel's active operational hub, with no active assignment | Dispatcher                                    |
| Couriers       | `GET /api/couriers/{id}`                     | `id: int`                             | —                               | `CourierResponseDto`; 404 when absent                                  | Admin                                                     |
| Couriers       | `PUT /api/couriers/{id}`                     | `id: int`                             | `UpdateCourierRequestDto`       | `CourierResponseDto`; 404 when absent                                  | Admin; active `hubId` required; hub cannot change during an active assignment |
| Couriers       | `DELETE /api/couriers/{id}`                  | `id: int`                             | —                               | `bool`                                                                 | Admin; soft delete                                        |
| Customers      | `GET /api/customers`                         | `page`, `pageSize`, optional `search`, `isActive` | —                        | `PagedResponseDto<CustomerResponseDto>`                                | Admin, Booking Agent                                      |
| Customers      | `GET /api/customers/{id}`                    | `id: int`                             | —                               | `CustomerResponseDto`; 404 when absent                                 | Admin, Booking Agent                                      |
| Customers      | `POST /api/customers`                        | —                                     | `CreateCustomerRequestDto`      | `CustomerResponseDto` (201)                                            | Admin, Booking Agent                                      |
| Customers      | `PUT /api/customers/{id}`                    | `id: int`                             | `UpdateCustomerRequestDto`      | `CustomerResponseDto`; 404 when absent                                 | Admin, Booking Agent                                      |
| Customers      | `DELETE /api/customers/{id}`                 | `id: int`                             | —                               | `bool`                                                                 | Admin, Booking Agent; soft delete                         |
| Hubs           | `GET /api/hubs`                              | `page`, `pageSize`, optional `search`, `activeOnly`, `isActive` | —          | `PagedResponseDto<HubResponseDto>`                                     | Admin, Booking Agent, Dispatcher, Courier                 |
| Hubs           | `GET /api/hubs/{id}`                         | `id: int`                             | —                               | `HubResponseDto`; 404 when absent                                      | Admin, Booking Agent, Dispatcher, Courier                 |
| Hubs           | `POST /api/hubs`                             | —                                     | `CreateHubRequestDto`           | `HubResponseDto` (201)                                                 | Admin                                                     |
| Hubs           | `PUT /api/hubs/{id}`                         | `id: int`                             | `UpdateHubRequestDto`           | `HubResponseDto`; 404 when absent                                      | Admin                                                     |
| Hubs           | `DELETE /api/hubs/{id}`                      | `id: int`                             | —                               | `bool`                                                                 | Admin; soft delete                                        |
| Parcels        | `GET /api/parcels`                           | —                                     | —                               | `ParcelResponseDto[]`                                                  | Admin, Booking Agent, Dispatcher, Courier                 |
| Parcels        | `GET /api/parcels/{id}`                      | `id: long`                            | —                               | `ParcelResponseDto`; 404 when absent                                   | Admin, Booking Agent, Dispatcher, Courier                 |
| Parcels        | `POST /api/parcels`                          | —                                     | `CreateParcelRequestDto`        | `ParcelResponseDto` (201)                                              | Admin, Booking Agent, Dispatcher, Courier                 |
| Parcel status  | `GET /api/ParcelStatus`                      | `page`, `pageSize`, optional `search` | —                               | `PagedResponseDto<ParcelStatusResponseDto>`                            | Admin, Booking Agent, Dispatcher, Courier                 |
| Parcel status  | `GET /api/ParcelStatus/{id}`                 | `id: int`                             | —                               | `ParcelStatusResponseDto`; 404 when absent                             | Admin, Booking Agent, Dispatcher, Courier                 |
| Parcel status  | `POST /api/ParcelStatus`                     | —                                     | `CreateParcelStatusRequestDto`  | `ParcelStatusResponseDto` (201)                                        | Admin, Booking Agent, Dispatcher, Courier                 |
| Parcel status  | `PUT /api/ParcelStatus/{id}`                 | `id: int`                             | `UpdateParcelStatusRequestDto` | `ParcelStatusResponseDto`; 404 when absent                             | Admin, Booking Agent, Dispatcher, Courier                 |
| Parcel status  | `DELETE /api/ParcelStatus/{id}`              | `id: int`                             | —                               | `bool`                                                                 | Admin, Booking Agent, Dispatcher, Courier; soft delete    |
| Parcel booking | `GET /api/ParcelBooking`                     | `page`, `pageSize`, optional `search` | —                               | `PagedResponseDto<ParcelBookingResponseDto>`                           | Admin, Booking Agent, Dispatcher, Courier                 |
| Parcel booking | `GET /api/ParcelBooking/{id}`                | `id: long`                            | —                               | `ParcelBookingResponseDto`; 404 when absent                            | Admin, Booking Agent, Dispatcher, Courier                 |
| Parcel booking | `POST /api/ParcelBooking`                    | —                                     | `CreateParcelBookingRequestDto` | `ParcelBookingResponseDto` (201)                                       | Admin, Booking Agent, Dispatcher, Courier                 |
| Assignments    | `GET /api/parcel-assignments`                | `page`, `pageSize`                    | —                               | `PagedResponseDto<ParcelAssignmentResponseDto>`                        | Admin, Dispatcher                                         |
| Assignments    | `GET /api/parcel-assignments/active`         | —                                     | —                               | `ParcelAssignmentResponseDto[]`                                        | Admin, Dispatcher                                         |
| Assignments    | `GET /api/parcel-assignments/{id}`           | `id: long`                            | —                               | `ParcelAssignmentResponseDto`; 404 when absent                         | Admin, Dispatcher                                         |
| Assignments    | `POST /api/parcel-assignments`               | —                                     | `AssignParcelRequestDto`        | `ParcelAssignmentResponseDto` (201)                                    | Admin, Dispatcher                                         |
| Assignments    | `PUT /api/parcel-assignments/{id}/reassign`  | `id: long`                            | `ReassignParcelRequestDto`      | `ParcelAssignmentResponseDto`                                          | Admin, Dispatcher                                         |
| Assignments    | `PUT /api/parcel-assignments/{id}/unassign`  | `id: long`                            | —                               | `string`                                                               | Admin, Dispatcher                                         |
| Tracking       | `GET /api/parcel-tracking`                   | `page`, `pageSize`                    | —                               | `PagedResponseDto<ParcelTrackingResponseDto>` with origin/destination hub names | Admin, Booking Agent, Dispatcher, Courier; Courier scope enforced server-side |
| Tracking       | `GET /api/parcel-tracking/{trackingNumber}`  | `trackingNumber: string`              | —                               | `ParcelTrackingResponseDto[]` with origin/destination hub names; empty for an accessible parcel with no events | Admin, Booking Agent, Dispatcher, Courier; Courier scope enforced server-side |
| Tracking       | `POST /api/parcel-tracking`                  | —                                     | `CreateStatusEventRequestDto`   | `ParcelTrackingResponseDto`                                            | Admin, Booking Agent, Dispatcher                           |
| Tracking       | `PUT /api/parcel-tracking/{parcelId}/status` | `parcelId: long`                      | `UpdateParcelStatusRequestDto`  | `ParcelTrackingResponseDto`                                            | Admin, Booking Agent, Dispatcher; terminal guard           |
| Tracking       | `PUT /api/parcel-tracking/{parcelId}/courier-transition` | `parcelId: long` | `UpdateCourierParcelStatusRequestDto` | `ParcelTrackingResponseDto`                                  | Courier; verifies active profile and assignment            |
| Routes         | `GET /api/Route`                             | `page`, `pageSize`, optional `search`, `isActive` | —                       | `PagedResponseDto<RouteResponseDto>`                                   | Admin, Booking Agent, Dispatcher, Courier                 |
| Routes         | `GET /api/Route/{id}`                        | `id: int`                             | —                               | `RouteResponseDto`; 404 when absent                                    | Admin, Booking Agent, Dispatcher, Courier                 |
| Routes         | `POST /api/Route`                            | —                                     | `CreateRouteRequestDto`         | `RouteResponseDto` (201)                                               | Admin                                                     |
| Routes         | `PUT /api/Route/{id}`                        | `id: int`                             | `UpdateRouteRequestDto`         | `RouteResponseDto`; 404 when absent                                    | Admin                                                     |
| Routes         | `DELETE /api/Route/{id}`                     | `id: int`                             | —                               | `bool`                                                                 | Admin; soft delete                                        |
| Dashboard      | `GET /api/dashboard/admin`                   | —                                     | —                               | `AdminDashboardResponseDto`                                            | Admin                                                     |
| Dashboard      | `GET /api/dashboard/dispatcher`              | —                                     | —                               | `DispatcherDashboardResponseDto`                                       | Dispatcher                                                |
| Dashboard      | `GET /api/dashboard/courier`                 | —                                     | —                               | `CourierDashboardResponseDto`                                          | Courier role                                              |
| Reports        | `GET /api/reports/parcel-bookings`           | optional `fromDate`, `toDate`         | —                               | `ParcelBookingReportDto[]`                                             | Admin                                                     |
| Reports        | `GET /api/reports/parcel-status`              | optional `fromDate`, `toDate`         | —                               | `ParcelStatusReportDto[]`                                              | Admin                                                     |
| Reports        | `GET /api/reports/courier-performance`        | optional `fromDate`, `toDate`         | —                               | `CourierPerformanceReportDto[]`                                        | Admin                                                     |
| Reports        | `GET /api/reports/deliveries`                 | optional `fromDate`, `toDate`         | —                               | Delivery event rows and successful/failed-attempt counts               | Admin                                                     |
| Reports        | `GET /api/reports/hubs`                       | optional `fromDate`, `toDate`         | —                               | Hub details with origin and destination parcel counts                  | Admin                                                     |
| Reports        | `GET /api/reports/users`                      | —                                     | —                               | Non-deleted user totals grouped by role and account status              | Admin                                                     |

The authentication staff-role and staff-registration endpoints, and Courier
profile management endpoints, enforce Admin authorization at the
controller/action level. Courier dispatch options are Dispatcher-only, the
Courier dashboard and Courier transition endpoint require the Courier role,
and assignment mutations are Admin/Dispatcher-only. Courier tracking reads are
restricted server-side to the authenticated Courier's assigned parcel history.

## Admin UI scope and backend gaps

| Admin screen    | Backend operations used                                                                                      | Supported operations                                                                                                                      |
| --------------- | ------------------------------------------------------------------------------------------------------------ | ----------------------------------------------------------------------------------------------------------------------------------------- |
| Dashboard       | `GET /api/dashboard/admin`, `GET /api/reports/courier-performance`, `GET /api/users`                         | Dashboard metrics, status summary, recent bookings, courier performance, and booking-agent/dispatcher totals derived from user role names |
| Users           | `GET/PUT/DELETE /api/users`, `GET /api/users/{id}`, `GET /api/Auth/roles`, `GET /api/Auth/staff-roles`, `POST /api/Auth/register-staff` | View/search/details, update fields/role/active state without password changes, soft-delete, and create staff accounts with linked Courier profiles where applicable |
| Customers       | `GET/POST/PUT/DELETE /api/customers`                                                                         | List, create, update, and soft-delete                                                                                                     |
| Couriers        | `GET/PUT/DELETE /api/couriers`                                                                               | Admin-only profile listing, viewing, updating, and soft deletion; new accounts and profiles are created through User Management               |
| Courier dispatch | `GET /api/couriers/dispatch-options`                                                                         | Dispatcher-only list of active, available, linked Courier accounts with no active assignment; the assignment service revalidates the selected Courier |
| Hubs            | `GET/POST/PUT/DELETE /api/hubs`                                                                              | Paginated/searchable/active-state-filtered list; Admin-only create, update, and soft-delete                                                |
| Parcels         | `GET /api/parcels`, `GET /api/parcels/{id}`, `POST /api/parcels`                                             | View, search, and create; no update or delete endpoint exists                                                                             |
| Parcel statuses | `GET/POST/PUT/DELETE /api/ParcelStatus`                                                                      | Paginated/searchable list, create, update, and soft-delete                                                                                |
| Routes          | `GET/POST/PUT/DELETE /api/Route`                                                                             | Paginated/searchable/active-state-filtered list; Admin-only create, update, and soft-delete                                                |
| Reports         | Existing parcel booking/status and courier reports plus `GET /api/reports/deliveries`, `/hubs`, and `/users` | Read-only parcel, courier, delivery, hub, and user reports; Admin UI supports date filters where applicable, search, filters, and CSV export |

The Admin dashboard response does not provide booking-agent or dispatcher
counts, so those two figures are derived from roles in `GET /api/users`.
Parcel status totals are shown as returned; there is no separate backend
statistic for pending parcels, failed deliveries, or recent parcel activities.

`GET /api/Auth/staff-roles` provides the active assignable staff roles to Admins;
the Admin registration form uses those actual role records rather than the
authenticated Admin user's own role list. Courier registration requires a phone
number and optionally accepts vehicle type and number. The backend creates its
Courier profile with the generated user ID in the same transaction as the user
and role assignment. Admins can update non-deleted user accounts and
soft-deactivate them; self-deactivation, deletion of the last active Admin, and
deactivation/deletion of a Courier with an active assignment are rejected.
Inactive and soft-deleted users are rejected during JWT validation. Active role
assignments are also compared with JWT role claims, so a role edit requires the
user to sign in again. User edits accept a role selected from the Admin-only
role options endpoint (`GET /api/Auth/roles`); Admin roles cannot be granted by
editing a profile. Courier and Customer Viewer roles require their corresponding
linked profiles.
Parcel update/delete endpoints do not exist.

`UpdateCustomerRequestDto` contains `isActive`, but `CustomerService.UpdateAsync`
does not apply that field. Customer deactivation is therefore exposed only via
the existing soft-delete endpoint.

Frontend routes for Admin screens are restricted by the Admin role from
AuthContext. Courier management and staff role/registration endpoints also
enforce `[Authorize(Roles = "Admin")]` on the backend. Assignment mutations
require Admin or Dispatcher, and tracking writes require Admin, Booking Agent,
or Dispatcher; most other management, dashboard, and report actions still use
`[Authorize]` without an Admin role policy.

JWT bearer validation checks the configured issuer, audience, signing key, and
expiration. The backend uses the role claim for authorization, and the token
contains role names derived from the user's active roles.

## Phase 9 Courier workflow and API mapping

| Workflow step | API / data source | Verified behavior and constraints |
| ------------- | ----------------- | --------------------------------- |
| Courier dashboard and assigned parcels | `GET /api/dashboard/courier` | Courier-role only; uses JWT `NameIdentifier` to find the active, non-deleted Courier profile and returns only its assignments and recorded successful deliveries. |
| Parcel details | `CourierAssignedParcelDto` from the dashboard | Includes parcel and assignment dates, weight, sender/recipient contact and address fields (including recipient email), route/hub names, and status; no separate parcel-details endpoint is required. |
| Tracking history | `GET /api/parcel-tracking/{trackingNumber}` | Courier reads are server-side filtered to active assignments or parcels successfully delivered by that authenticated Courier; inaccessible or unknown parcels return 404. |
| Courier status changes | `PUT /api/parcel-tracking/{parcelId}/courier-transition` | Accepts `statusCode` and optional `notes`; resolves the active status row by code, verifies the active Courier profile and assignment, validates the matrix below, and atomically updates the parcel plus its tracking event. |
| Failed-attempt review | Tracking events with `DELIVERY_ATTEMPTED` | Uses existing `StatusEvent.Notes`, `EventAt`, `CourierId`, and `CreatedBy` fields. The Dispatcher Tracking page displays the attempt; no delivery-attempt table or schema change is used. |
| Dispatcher retry/reassignment | `PUT /api/parcel-assignments/{id}/reassign` (or existing unassign then assign workflow) | Mutations are restricted to Admin/Dispatcher. Courier may start a subsequent attempt only when its active assignment was created after the latest failed-attempt event. |
| Successful delivery | Courier transition to `DELIVERED` | Sets existing `Parcel.DeliveredAt`, writes the status event, ends the active assignment, and marks the Courier available. `DELIVERED` remains `IsTerminal = false` in the database but is treated as final by application validation. |
| Delivery history | `CourierDashboardResponseDto.DeliveryHistory` | Derived from this Courier's parcel assignments with `DeliveredAt` set. Failed attempts remain visible in parcel tracking history, not in the successful-delivery list. |

### Approved Courier status transition matrix

| Current status | Next status | Actor | Required validation |
| -------------- | ------------ | ----- | ------------------- |
| `BOOKED` | `PICKED_UP` | Assigned Courier | Active parcel, profile, and assignment |
| `PICKED_UP` | `IN_TRANSIT` | Assigned Courier | Active parcel, profile, and assignment |
| `AT_DESTINATION_HUB` | `OUT_FOR_DELIVERY` | Assigned Courier | Destination-hub status must already have been recorded by the existing hub/Dispatcher workflow |
| `DELIVERY_ATTEMPTED` | `OUT_FOR_DELIVERY` | Assigned Courier | Dispatcher/Admin must create or reassign an active assignment after the latest failed-attempt event; assignments created within the event's stored second are timestamped one second later |
| `OUT_FOR_DELIVERY` | `DELIVERED` | Assigned Courier | Active assignment; records `DeliveredAt` and releases the Courier |
| `OUT_FOR_DELIVERY` | `DELIVERY_ATTEMPTED` | Assigned Courier | Active assignment; optional remarks are stored in the existing tracking event `Notes` |

Courier cannot set `AT_ORIGIN_HUB`, `AT_DESTINATION_HUB`, `ON_HOLD`,
`RETURNED`, or `CANCELLED`. The existing generic status endpoint remains
available to Admin, Booking Agent, and Dispatcher, but delivered/terminal
parcels are rejected by application logic. No status IDs or database statuses
are hardcoded or changed.

## Request DTO fields

Names below are serialized as camelCase in frontend JSON/query parameters.
Nullable fields are marked `?`; required-field validation and business checks
remain defined by each backend DTO/service.

| DTO                                     | Fields                                                                                                                                                                                                                                                          |
| --------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `LoginRequestDto`                       | `userNameOrEmail`, `password`                                                                                                                                                                                                                                   |
| `BootstrapAdminRequestDto`              | `bootstrapKey`, `userName`, `email`, `password`, `fullName`, `phone?`                                                                                                                                                                                           |
| `RegisterStaffRequestDto`               | `userName`, `email`, `password`, `fullName`, `phone?`, `roleId`, `hubId?` (required for Courier role)                                                                                                                                                             |
| `RegisterCustomerRequestDto`            | `userName`, `email`, `password`, `fullName`, `phone`, `addressLine?`, `city?`, `province?`, `postalCode?`                                                                                                                                                       |
| `UpdateUserRequestDto`                  | `userName`, `email`, `fullName`, `phone?`, `roleId`, `isActive`                                                                                                                                                                                                 |
| `UpdateCourierRequestDto`               | Courier fields including required `hubId`, plus `isAvailable`, `isActive`                                                                                                                                                                                      |
| `CreateCustomerRequestDto`              | `fullName`, `email?`, `phone`, `addressLine?`, `city?`, `province?`, `postalCode?`                                                                                                                                                                              |
| `UpdateCustomerRequestDto`              | Create fields plus `isActive`                                                                                                                                                                                                                                   |
| `CreateHubRequestDto`                   | `hubName`, `addressLine`, `city`, `province?`, `postalCode?`, `country`, `contactPhone?`                                                                                                                                                                        |
| `UpdateHubRequestDto`                   | Create fields plus `isActive`                                                                                                                                                                                                                                   |
| `CreateParcelRequestDto`                | `senderCustomerId`, `routeId?`, `currentStatusId`, `expectedDeliveryDate?`, `totalWeightKg`, `declaredValue?`, `specialInstructions?`                                                                                                                           |
| `CreateParcelBookingRequestDto`         | `senderCustomerId`, `routeId?`, `totalWeightKg`, `declaredValue?`, `expectedDeliveryDate?`, `specialInstructions?`, `recipientName`, `recipientPhone`, `recipientEmail?`, `recipientAddressLine`, `recipientCity`, `recipientProvince?`, `recipientPostalCode?` |
| `CreateParcelStatusRequestDto`          | `statusName`, `description?`, `displayOrder`, `isTerminal`                                                                                                                                                                                                      |
| `UpdateParcelStatusRequestDto`          | Update fields plus `isActive`                                                                                                                                                                                                                                   |
| `AssignParcelRequestDto`                | `parcelId`, `courierId`, `assignmentNotes?`                                                                                                                                                                                                                     |
| `ReassignParcelRequestDto`              | `newCourierId`, `assignmentNotes?`                                                                                                                                                                                                                              |
| `CreateStatusEventRequestDto`           | `parcelId`, `parcelStatusId`, `hubId?`, `courierId?`, `notes?`                                                                                                                                                                                                  |
| Tracking `UpdateParcelStatusRequestDto` | `parcelStatusId`, `hubId?`, `courierId?`, `notes?`                                                                                                                                                                                                              |
| `UpdateCourierParcelStatusRequestDto`   | `statusCode`, `notes?` (maximum 500 characters)                                                                                                                                                                                                                 |
| `CreateRouteRequestDto`                 | `routeName`, `originHubId`, `destinationHubId`, `distanceKm?`, `estimatedHours?`, `description?`                                                                                                                                                                |
| `UpdateRouteRequestDto`                 | Create fields plus `isActive`                                                                                                                                                                                                                                   |
| `PaginationRequestDto`                  | `page` (default 1, range 1–2147483647), `pageSize` (default 10, range 1–100)                                                                                                                                                                                    |
| `ReportRequestDto`                      | `fromDate?`, `toDate?`                                                                                                                                                                                                                                          |

Important response DTO shapes include:

- `LoginResponseDto`: `userId`, `userName`, `fullName`, `email`, `roles`,
  `roleIds`, `token`, `expiresAt`.
- `AdminDashboardResponseDto`: totals for parcels, customers, couriers,
  available/busy couriers, hubs, routes, plus parcel-status summary and recent
  bookings.
- `DispatcherDashboardResponseDto`: parcel/assignment/courier totals, recent
  assignments, and recent unassigned parcels.
- `CourierDashboardResponseDto`: courier identity, assigned/in-transit/
  delivered totals, active assigned parcels, and delivery history.
- `PagedResponseDto<T>` preserves `items`, `page`, `pageSize`, `totalRecords`,
  and `totalPages` without flattening the page metadata.

## Backend behavior relevant to clients

- Read/list filtering is repository-specific. Users and parcels exclude inactive
  and deleted records; courier, customer, hub, route, status, and booking queries
  commonly exclude deleted records but do not uniformly filter inactive ones.
- Courier, customer, hub, status, route, and booking delete actions mark records
  inactive and deleted. Parcels have no update/delete controller actions.
- Parcel assignment history is paged without a soft-delete filter; the `active`
  endpoint means `UnassignedAt == null`.
- Parcel statuses are a fixed database catalog: the existing status codes are
  resolved by code and IDs are not hardcoded. Status creation and soft deletion
  are disabled; workflow status code, active state, and terminal state cannot
  be changed. General updates validate business transitions, while Courier
  transitions remain limited to the existing pickup/delivery workflow.
- Tracking history and paged tracking results exclude inactive/deleted parcels.
- New bookings create an initial `BOOKED` tracking event in the same transaction
  as the parcel and recipient. Existing bookings with no events are not backfilled.
- Courier tracking reads are additionally scoped to active assignments or
  deliveries recorded for the authenticated active Courier profile.
- Parcel detail responses expose the persisted `DeliveredAt`; when it is null,
  the details query uses the latest existing `DELIVERED` status event timestamp
  when available. It does not create an event or synthesize a timestamp.
- Courier registration and updates validate an active hub. Dispatch options are
  parcel-specific; assignments and reassignments validate the courier against
  the parcel's current hub, or route origin/destination when no current hub is set.
- Reports and dashboards are not paged by these controllers.
- The backend has no separate generic “booking” controller; the module is
  `ParcelBookingController`.
- The backend's `README-API.md` endpoint table is incomplete and lists
  `/api/parcel-statuses`, while the controller route is `/api/ParcelStatus`.
  This mapping follows controller attributes and action signatures.
