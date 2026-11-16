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
| Auth           | `POST /api/Auth/register-staff`              | —                                     | `RegisterStaffRequestDto`       | `string`                                                               | Admin; Courier profile is created and linked atomically   |
| Auth           | `POST /api/Auth/register-customer`           | —                                     | `RegisterCustomerRequestDto`    | `string`                                                               | Booking Agent                                             |
| Auth           | `POST /api/Auth/logout`                      | —                                     | —                               | `string`                                                               | Authenticated                                             |
| Auth           | `GET /api/Auth/me`                           | —                                     | —                               | Current-user fields: userId, userName, fullName, email, roles, roleIds | Authenticated                                             |
| Users          | `GET /api/users`                             | —                                     | —                               | `UserResponseDto[]`                                                    | Authenticated                                             |
| Users          | `GET /api/users/{id}`                        | `id: int`                             | —                               | `UserResponseDto`; 404 when absent                                     | Authenticated                                             |
| Couriers       | `GET /api/couriers`                          | `page`, `pageSize`, optional `search` | —                               | `PagedResponseDto<CourierResponseDto>`                                 | Admin                                                     |
| Couriers       | `GET /api/couriers/eligible-users`            | optional `excludeCourierId`           | —                               | Active Courier login accounts without another linked profile           | Admin                                                     |
| Couriers       | `GET /api/couriers/{id}`                     | `id: int`                             | —                               | `CourierResponseDto`; 404 when absent                                  | Admin                                                     |
| Couriers       | `POST /api/couriers`                         | —                                     | `CreateCourierRequestDto` with required `userId` | `CourierResponseDto` (201)                         | Admin; links an existing active Courier login             |
| Couriers       | `PUT /api/couriers/{id}`                     | `id: int`                             | `UpdateCourierRequestDto`       | `CourierResponseDto`; 404 when absent                                  | Admin; null `userId` does not clear an existing link      |
| Couriers       | `DELETE /api/couriers/{id}`                  | `id: int`                             | —                               | `bool`                                                                 | Admin; soft delete                                        |
| Customers      | `GET /api/customers`                         | `page`, `pageSize`, optional `search` | —                               | `PagedResponseDto<CustomerResponseDto>`                                | Authenticated                                             |
| Customers      | `GET /api/customers/{id}`                    | `id: int`                             | —                               | `CustomerResponseDto`; 404 when absent                                 | Authenticated                                             |
| Customers      | `POST /api/customers`                        | —                                     | `CreateCustomerRequestDto`      | `CustomerResponseDto` (201)                                            | Authenticated                                             |
| Customers      | `PUT /api/customers/{id}`                    | `id: int`                             | `UpdateCustomerRequestDto`      | `CustomerResponseDto`; 404 when absent                                 | Authenticated                                             |
| Customers      | `DELETE /api/customers/{id}`                 | `id: int`                             | —                               | `bool`                                                                 | Authenticated; soft delete                                |
| Hubs           | `GET /api/hubs`                              | `page`, `pageSize`, optional `search` | —                               | `PagedResponseDto<HubResponseDto>`                                     | Authenticated                                             |
| Hubs           | `GET /api/hubs/{id}`                         | `id: int`                             | —                               | `HubResponseDto`; 404 when absent                                      | Authenticated                                             |
| Hubs           | `POST /api/hubs`                             | —                                     | `CreateHubRequestDto`           | `HubResponseDto` (201)                                                 | Authenticated                                             |
| Hubs           | `PUT /api/hubs/{id}`                         | `id: int`                             | `UpdateHubRequestDto`           | `HubResponseDto`; 404 when absent                                      | Authenticated                                             |
| Hubs           | `DELETE /api/hubs/{id}`                      | `id: int`                             | —                               | `bool`                                                                 | Authenticated; soft delete                                |
| Parcels        | `GET /api/parcels`                           | —                                     | —                               | `ParcelResponseDto[]`                                                  | Authenticated                                             |
| Parcels        | `GET /api/parcels/{id}`                      | `id: long`                            | —                               | `ParcelResponseDto`; 404 when absent                                   | Authenticated                                             |
| Parcels        | `POST /api/parcels`                          | —                                     | `CreateParcelRequestDto`        | `ParcelResponseDto` (201)                                              | Authenticated                                             |
| Parcel status  | `GET /api/ParcelStatus`                      | `page`, `pageSize`, optional `search` | —                               | `PagedResponseDto<ParcelStatusResponseDto>`                            | Authenticated                                             |
| Parcel status  | `GET /api/ParcelStatus/{id}`                 | `id: int`                             | —                               | `ParcelStatusResponseDto`; 404 when absent                             | Authenticated                                             |
| Parcel status  | `POST /api/ParcelStatus`                     | —                                     | `CreateParcelStatusRequestDto`  | `ParcelStatusResponseDto` (201)                                        | Authenticated                                             |
| Parcel status  | `PUT /api/ParcelStatus/{id}`                 | `id: int`                             | `UpdateParcelStatusRequestDto`  | `ParcelStatusResponseDto`; 404 when absent                             | Authenticated                                             |
| Parcel status  | `DELETE /api/ParcelStatus/{id}`              | `id: int`                             | —                               | `bool`                                                                 | Authenticated; soft delete                                |
| Parcel booking | `GET /api/ParcelBooking`                     | `page`, `pageSize`, optional `search` | —                               | `PagedResponseDto<ParcelBookingResponseDto>`                           | Authenticated                                             |
| Parcel booking | `GET /api/ParcelBooking/{id}`                | `id: long`                            | —                               | `ParcelBookingResponseDto`; 404 when absent                            | Authenticated                                             |
| Parcel booking | `POST /api/ParcelBooking`                    | —                                     | `CreateParcelBookingRequestDto` | `ParcelBookingResponseDto` (201)                                       | Authenticated                                             |
| Assignments    | `GET /api/parcel-assignments`                | `page`, `pageSize`                    | —                               | `PagedResponseDto<ParcelAssignmentResponseDto>`                        | Authenticated                                             |
| Assignments    | `GET /api/parcel-assignments/active`         | —                                     | —                               | `ParcelAssignmentResponseDto[]`                                        | Authenticated                                             |
| Assignments    | `GET /api/parcel-assignments/{id}`           | `id: long`                            | —                               | `ParcelAssignmentResponseDto`; 404 when absent                         | Authenticated                                             |
| Assignments    | `POST /api/parcel-assignments`               | —                                     | `AssignParcelRequestDto`        | `ParcelAssignmentResponseDto` (201)                                    | Authenticated                                             |
| Assignments    | `PUT /api/parcel-assignments/{id}/reassign`  | `id: long`                            | `ReassignParcelRequestDto`      | `ParcelAssignmentResponseDto`                                          | Authenticated                                             |
| Assignments    | `PUT /api/parcel-assignments/{id}/unassign`  | `id: long`                            | —                               | `string`                                                               | Authenticated                                             |
| Tracking       | `GET /api/parcel-tracking`                   | `page`, `pageSize`                    | —                               | `PagedResponseDto<ParcelTrackingResponseDto>`                          | Authenticated                                             |
| Tracking       | `GET /api/parcel-tracking/{trackingNumber}`  | `trackingNumber: string`              | —                               | `ParcelTrackingResponseDto[]`                                          | Authenticated                                             |
| Tracking       | `POST /api/parcel-tracking`                  | —                                     | `CreateStatusEventRequestDto`   | `ParcelTrackingResponseDto`                                            | Authenticated                                             |
| Tracking       | `PUT /api/parcel-tracking/{parcelId}/status` | `parcelId: long`                      | `UpdateParcelStatusRequestDto`  | `ParcelTrackingResponseDto`                                            | Authenticated                                             |
| Routes         | `GET /api/Route`                             | `page`, `pageSize`, optional `search` | —                               | `PagedResponseDto<RouteResponseDto>`                                   | Authenticated                                             |
| Routes         | `GET /api/Route/{id}`                        | `id: int`                             | —                               | `RouteResponseDto`; 404 when absent                                    | Authenticated                                             |
| Routes         | `POST /api/Route`                            | —                                     | `CreateRouteRequestDto`         | `RouteResponseDto` (201)                                               | Authenticated                                             |
| Routes         | `PUT /api/Route/{id}`                        | `id: int`                             | `UpdateRouteRequestDto`         | `RouteResponseDto`; 404 when absent                                    | Authenticated                                             |
| Routes         | `DELETE /api/Route/{id}`                     | `id: int`                             | —                               | `bool`                                                                 | Authenticated; soft delete                                |
| Dashboard      | `GET /api/dashboard/admin`                   | —                                     | —                               | `AdminDashboardResponseDto`                                            | Authenticated                                             |
| Dashboard      | `GET /api/dashboard/dispatcher`              | —                                     | —                               | `DispatcherDashboardResponseDto`                                       | Authenticated                                             |
| Dashboard      | `GET /api/dashboard/courier`                 | —                                     | —                               | `CourierDashboardResponseDto`                                          | Authenticated                                             |
| Reports        | `GET /api/reports/parcel-bookings`           | optional `fromDate`, `toDate`         | —                               | `ParcelBookingReportDto[]`                                             | Authenticated                                             |
| Reports        | `GET /api/reports/parcel-status`             | optional `fromDate`, `toDate`         | —                               | `ParcelStatusReportDto[]`                                              | Authenticated                                             |
| Reports        | `GET /api/reports/courier-performance`       | optional `fromDate`, `toDate`         | —                               | `CourierPerformanceReportDto[]`                                        | Authenticated                                             |

The authentication staff-role and staff-registration endpoints, and all Courier
management endpoints, enforce Admin authorization at the controller/action level.
Other `[Authorize]` controllers accept any authenticated user; role-based
restrictions should not be inferred on the frontend.

## Admin UI scope and backend gaps

| Admin screen    | Backend operations used                                                                                      | Supported operations                                                                                                                      |
| --------------- | ------------------------------------------------------------------------------------------------------------ | ----------------------------------------------------------------------------------------------------------------------------------------- |
| Dashboard       | `GET /api/dashboard/admin`, `GET /api/reports/courier-performance`, `GET /api/users`                         | Dashboard metrics, status summary, recent bookings, courier performance, and booking-agent/dispatcher totals derived from user role names |
| Users           | `GET /api/users`, `GET /api/users/{id}`, `GET /api/Auth/staff-roles`, `POST /api/Auth/register-staff`        | View/search/details; create login accounts using active backend staff roles; Courier registration also creates its linked profile         |
| Customers       | `GET/POST/PUT/DELETE /api/customers`                                                                         | List, create, update, and soft-delete                                                                                                     |
| Couriers        | `GET /api/couriers/eligible-users`, `GET/POST/PUT/DELETE /api/couriers`                                       | Admin-only profile management; Add Courier links an existing eligible Courier login without creating another account                        |
| Hubs            | `GET/POST/PUT/DELETE /api/hubs`                                                                              | Paginated/searchable list, create, update, and soft-delete                                                                                |
| Parcels         | `GET /api/parcels`, `GET /api/parcels/{id}`, `POST /api/parcels`                                             | View, search, and create; no update or delete endpoint exists                                                                             |
| Parcel statuses | `GET/POST/PUT/DELETE /api/ParcelStatus`                                                                      | Paginated/searchable list, create, update, and soft-delete                                                                                |
| Routes          | `GET/POST/PUT/DELETE /api/Route`                                                                             | Paginated/searchable list, create, update, and soft-delete                                                                                |
| Reports         | `GET /api/reports/parcel-bookings`, `GET /api/reports/parcel-status`, `GET /api/reports/courier-performance` | Read reports with optional date-range filters                                                                                             |

The Admin dashboard response does not provide booking-agent or dispatcher
counts, so those two figures are derived from roles in `GET /api/users`.
Parcel status totals are shown as returned; there is no separate backend
statistic for pending parcels, failed deliveries, or recent parcel activities.

`GET /api/Auth/staff-roles` provides the active assignable staff roles to Admins;
the Admin registration form uses those actual role records rather than the
authenticated Admin user's own role list. Courier registration requires a phone
number and optionally accepts vehicle type and number. The backend creates its
Courier profile with the generated user ID in the same transaction as the user
and role assignment. User update/deactivation endpoints do not exist. Parcel
update/delete endpoints do not exist either.

`UpdateCustomerRequestDto` contains `isActive`, but `CustomerService.UpdateAsync`
does not apply that field. Customer deactivation is therefore exposed only via
the existing soft-delete endpoint.

Frontend routes for Admin screens are restricted by the Admin role from
AuthContext. Courier management and staff role/registration endpoints also
enforce `[Authorize(Roles = "Admin")]` on the backend. Other management,
dashboard, and report controllers still use `[Authorize]` without an Admin role
policy.

JWT bearer validation checks the configured issuer, audience, signing key, and
expiration. The backend uses the role claim for authorization, and the token
contains role names derived from the user's active roles.

## Courier UI scope and backend gaps

| Courier screen | API and data source | Supported behavior |
| -------------- | ------------------- | ------------------ |
| Dashboard and assigned parcels | `GET /api/dashboard/courier` | The repository selects the active courier profile linked to the authenticated user ID, then returns that courier's active assignments and the supported totals. |
| Parcel details | `CourierAssignedParcelDto` from the courier dashboard response | Display only fields returned by the courier-scoped dashboard response; there is no courier-scoped parcel-details endpoint. |
| Tracking history | `GET /api/parcel-tracking/{trackingNumber}` after matching a parcel against the courier dashboard response | Read-only history. The tracking controller is `[Authorize]` but does not check assignment ownership, so the UI only requests history for a parcel in the authenticated courier's dashboard response. |
| Pickup and status updates | `PUT /api/parcel-tracking/{parcelId}/status` | Not used for courier actions: the controller has no Courier role/assignment check, and the service accepts any active status without validating transitions or assignment ownership. |
| Delivery attempts and delivery proof | No matching controller, request DTO, or operation was found | Not supported by the current backend. |
| Delivery history | No courier-scoped history endpoint | The dashboard only returns current active assignments; it cannot represent all past courier deliveries. |

The generic parcel, assignment, and status-update APIs are authenticated-only,
not Courier-specific. They must not be used by Courier screens to fetch other
couriers' assignments or modify parcel state. No backend or database changes
are included in the Courier frontend phase.

## Request DTO fields

Names below are serialized as camelCase in frontend JSON/query parameters.
Nullable fields are marked `?`; required-field validation and business checks
remain defined by each backend DTO/service.

| DTO                                     | Fields                                                                                                                                                                                                                                                          |
| --------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `LoginRequestDto`                       | `userNameOrEmail`, `password`                                                                                                                                                                                                                                   |
| `BootstrapAdminRequestDto`              | `bootstrapKey`, `userName`, `email`, `password`, `fullName`, `phone?`                                                                                                                                                                                           |
| `RegisterStaffRequestDto`               | `userName`, `email`, `password`, `fullName`, `phone?`, `roleId`                                                                                                                                                                                                 |
| `RegisterCustomerRequestDto`            | `userName`, `email`, `password`, `fullName`, `phone`, `addressLine?`, `city?`, `province?`, `postalCode?`                                                                                                                                                       |
| `CreateCourierRequestDto`               | `userId?`, `fullName`, `phone`, `email?`, `vehicleType?`, `vehicleNumber?`                                                                                                                                                                                      |
| `UpdateCourierRequestDto`               | Create fields plus `isAvailable`, `isActive`                                                                                                                                                                                                                    |
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
  delivered totals, and assigned parcels.
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
- Tracking history and paged tracking results exclude inactive/deleted parcels.
- Reports and dashboards are not paged by these controllers.
- The backend has no separate generic “booking” controller; the module is
  `ParcelBookingController`.
- The backend's `README-API.md` endpoint table is incomplete and lists
  `/api/parcel-statuses`, while the controller route is `/api/ParcelStatus`.
  This mapping follows controller attributes and action signatures.
