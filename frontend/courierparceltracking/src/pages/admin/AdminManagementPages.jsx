import { useMemo } from 'react'
import AdminEntityManager from '../../components/admin/AdminEntityManager'
import { useAuth } from '../../hooks/useAuth'
import { registerStaff } from '../../services/authService'
import {
  couriersConfig,
  customersConfig,
  hubsConfig,
  parcelsConfig,
  parcelStatusesConfig,
  routesConfig,
  usersConfig,
} from './adminResourceConfigs'

export function AdminUsersPage() {
  const { user } = useAuth()
  const availableRoles = useMemo(() => {
    if (
      !Array.isArray(user?.roles)
      || !Array.isArray(user?.roleIds)
      || user.roles.length !== user.roleIds.length
    ) {
      return []
    }

    return user.roles
      .map((role, index) => ({
        label: role,
        value: Number(user.roleIds[index]),
      }))
      .filter((role) => Number.isInteger(role.value) && role.value > 0)
  }, [user])
  const config = useMemo(() => ({
    ...usersConfig,
    description: availableRoles.length
      ? 'View users and create staff accounts using roles returned for your authenticated Admin account.'
      : 'View users. Staff creation requires valid role IDs from the backend role list.',
    create: availableRoles.length ? registerStaff : undefined,
    fields: availableRoles.length
      ? [
        { name: 'userName', label: 'Username', required: true, minLength: 3, maxLength: 80 },
        { name: 'email', label: 'Email', type: 'email', required: true, maxLength: 256 },
        { name: 'password', label: 'Temporary password', type: 'password', required: true, minLength: 8, maxLength: 100 },
        { name: 'fullName', label: 'Full name', required: true, maxLength: 150 },
        { name: 'phone', label: 'Phone', type: 'tel', nullable: true, maxLength: 30 },
        { name: 'roleId', label: 'Role', type: 'select', required: true, integer: true, numeric: true, options: availableRoles },
      ]
      : [],
  }), [availableRoles])

  return <AdminEntityManager config={config} />
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
