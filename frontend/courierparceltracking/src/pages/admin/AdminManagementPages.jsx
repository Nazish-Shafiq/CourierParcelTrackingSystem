import AdminEntityManager from '../../components/admin/AdminEntityManager'
import { getStaffRoles, registerStaff } from '../../services/authService'
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
  const staffRoles = await getStaffRoles(signal)
  if (!staffRoles.length) {
    throw new Error('No active staff roles are available for registration.')
  }
  return { staffRoles }
}

function isCourierRoleSelected(values, options) {
  return options.staffRoles?.some(
    (role) => String(role.value) === String(values.roleId) && role.label === 'Courier',
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
    { name: 'password', label: 'Temporary password', type: 'password', required: true, minLength: 8, maxLength: 100 },
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
    },
    {
      name: 'vehicleType',
      label: 'Vehicle type',
      nullable: true,
      maxLength: 50,
      visibleWhen: isCourierRoleSelected,
    },
    {
      name: 'vehicleNumber',
      label: 'Vehicle number',
      nullable: true,
      maxLength: 30,
      visibleWhen: isCourierRoleSelected,
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
