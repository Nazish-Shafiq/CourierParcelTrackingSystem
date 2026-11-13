import { getCustomers } from './customerService'
import { getHubs } from './hubService'
import { getParcelStatuses } from './parcelStatusService'
import { getRoutes } from './routeService'

async function getAllPagedRecords(loadPage, signal) {
  const firstPage = await loadPage({ page: 1, pageSize: 100, signal })
  if (!firstPage || !Array.isArray(firstPage.items) || !Number.isInteger(firstPage.totalPages)) {
    throw new Error('The lookup response did not contain valid pagination data.')
  }

  const records = [...firstPage.items]
  for (let page = 2; page <= firstPage.totalPages; page += 1) {
    const result = await loadPage({ page, pageSize: 100, signal })
    if (!result || !Array.isArray(result.items)) {
      throw new Error(`The lookup response for page ${page} was invalid.`)
    }
    records.push(...result.items)
  }

  return records
}

function toOptions(records, idField, labelForRecord) {
  return records.map((record) => ({
    value: record[idField],
    label: labelForRecord(record),
  }))
}

export async function getRouteFormOptions(signal) {
  const hubs = await getAllPagedRecords(getHubs, signal)
  return {
    hubs: toOptions(
      hubs.filter((hub) => hub.isActive),
      'hubId',
      (hub) => `${hub.hubName} (${hub.hubCode})`,
    ),
  }
}

export async function getParcelFormOptions(signal) {
  const [customers, routes, statuses] = await Promise.all([
    getAllPagedRecords(getCustomers, signal),
    getAllPagedRecords(getRoutes, signal),
    getAllPagedRecords(getParcelStatuses, signal),
  ])

  return {
    customers: toOptions(
      customers.filter((customer) => customer.isActive),
      'customerId',
      (customer) => `${customer.fullName} (${customer.customerCode})`,
    ),
    routes: toOptions(
      routes.filter((route) => route.isActive),
      'routeId',
      (route) => `${route.routeName} (${route.routeCode})`,
    ),
    statuses: toOptions(
      statuses.filter((status) => status.isActive),
      'parcelStatusId',
      (status) => `${status.statusName} (${status.statusCode})`,
    ),
  }
}
