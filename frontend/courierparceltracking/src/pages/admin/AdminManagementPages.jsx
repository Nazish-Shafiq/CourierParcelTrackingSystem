import AdminEntityManager from '../../components/admin/AdminEntityManager'
import {
  getAssignableRoles,
  getStaffRoles,
  registerStaff,
} from '../../services/authService'
import { getActiveHubOptions } from '../../services/adminLookupService'
import {
  couriersConfig,
  customersConfig,
  hubsConfig,
  parcelsConfig,
  parcelStatusesConfig,
  routesConfig,
  usersConfig,
} from './adminResourceConfigs'

async function loadStaffRegistrationOptions(signal) {
  const [staffRoles, roles, hubOptions] = await Promise.all([
    getStaffRoles(signal),
    getAssignableRoles(signal),
    getActiveHubOptions(signal),
  ])
  if (!staffRoles.length) {
    throw new Error('No active staff roles are available for registration.')
  }
  return { staffRoles, roles, ...hubOptions }
}

function isCourierRoleSelected(values, options) {
  return options.staffRoles?.some(
    (role) => String(role.value) === String(values.roleId)
      && role.label.toLowerCase() === 'courier',
  ) ?? false
}

const staffUsersConfig = {
  ...usersConfig,
  description: 'View users and create staff accounts using active staff roles.',
  create: registerStaff,
  loadFormOptions: loadStaffRegistrationOptions,
  fields: [
    { name: 'userName', label: 'Username', required: true, minLength: 3, maxLength: 80 },
    { name: 'email', label: 'Email', type: 'email', required: true, maxLength: 256 },
    { name: 'password', label: 'Temporary password', type: 'password', required: true, minLength: 8, maxLength: 100, createOnly: true },
    { name: 'fullName', label: 'Full name', required: true, maxLength: 150 },
    {
      name: 'phone',
      label: 'Phone',
      type: 'tel',
      nullable: true,
      maxLength: 30,
      minLength: 7,
      requiredWhen: isCourierRoleSelected,
    },
    {
      name: 'roleId',
      label: 'Role',
      type: 'select',
      required: true,
      integer: true,
      numeric: true,
      optionsKey: 'staffRoles',
      optionsKeyByMode: { create: 'staffRoles', edit: 'roles' },
    },
    {
      name: 'isActive',
      label: 'Active',
      type: 'checkbox',
      editOnly: true,
    },
    {
      name: 'hubId',
      label: 'Assigned hub',
      type: 'select',
      numeric: true,
      optionsKey: 'hubs',
      placeholder: 'Select a hub',
      visibleWhen: isCourierRoleSelected,
      requiredWhen: isCourierRoleSelected,
      createOnly: true,
    },
    {
      name: 'vehicleType',
      label: 'Vehicle type',
      nullable: true,
      maxLength: 50,
      visibleWhen: isCourierRoleSelected,
      createOnly: true,
    },
    {
      name: 'vehicleNumber',
      label: 'Vehicle number',
      nullable: true,
      maxLength: 30,
      visibleWhen: isCourierRoleSelected,
      createOnly: true,
    },
  ],
}

export function AdminUsersPage() {
  return <AdminEntityManager config={staffUsersConfig} />
}

export function AdminCustomersPage() {
  return <AdminEntityManager config={customersConfig} />
}

export function AdminCouriersPage() {
  return <AdminEntityManager config={couriersConfig} />
}

export function AdminHubsPage() {
  return <AdminEntityManager config={hubsConfig} />
}

export function AdminParcelsPage() {
  return <AdminEntityManager config={parcelsConfig} />
}

export function AdminParcelStatusesPage() {
  return <AdminEntityManager config={parcelStatusesConfig} />
}

export function AdminRoutesPage() {
  return <AdminEntityManager config={routesConfig} />
}
