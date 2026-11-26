import { useCallback, useState } from 'react'
import { Link, useSearchParams } from 'react-router-dom'
import DataTable from '../../components/tables/DataTable'
import EmptyState from '../../components/common/EmptyState'
import ErrorMessage from '../../components/common/ErrorMessage'
import LoadingSpinner from '../../components/common/LoadingSpinner'
import PageHeader from '../../components/common/PageHeader'
import RefreshButton from '../../components/common/RefreshButton'
import SearchBar from '../../components/common/SearchBar'
import StatusBadge from '../../components/common/StatusBadge'
import FormSelect from '../../components/forms/FormSelect'
import Pagination from '../../components/tables/Pagination'
import { useDashboardData } from '../../hooks/useDashboardData'
import {
  getCustomerDashboardStats,
  getCustomerParcels,
} from '../../services/customerDashboardService'
import '../../style/customer/customer-dashboard.css'

const PAGE_SIZE = 10

function formatDate(value) {
  return value ? new Date(value).toLocaleDateString() : '—'
}

function CustomerParcelsPage() {
  const [searchParams] = useSearchParams()
  const [page, setPage] = useState(1)
  const [search, setSearch] = useState(() => searchParams.get('search') ?? '')
  const [statusCode, setStatusCode] = useState(() => searchParams.get('statusCode') ?? '')

  const loadParcelsPage = useCallback(async (signal) => {
    const [summary, parcels] = await Promise.all([
      getCustomerDashboardStats(signal),
      getCustomerParcels({
        page,
        pageSize: PAGE_SIZE,
        search,
        statusCode: statusCode || undefined,
        signal,
      }),
    ])

    return { summary, parcels }
  }, [page, search, statusCode])

  const results = useDashboardData(loadParcelsPage)
  const summary = results.data?.summary
  const parcels = results.data?.parcels

  function updateSearch(value) {
    setPage(1)
    setSearch(value)
  }

  function updateStatus(event) {
    setPage(1)
    setStatusCode(event.target.value)
  }

  function refresh() {
    results.refresh()
  }

  return (
    <div className="role-dashboard role-dashboard--customer">
      <PageHeader
        actions={<RefreshButton isLoading={results.isLoading} onRefresh={refresh} />}
        breadcrumbs={[
          { label: 'Customer Dashboard', to: '/dashboard/customer' },
          { label: 'My Parcels' },
        ]}
        description="View your parcels and open their details to follow delivery progress and tracking history."
        eyebrow="Customer Viewer"
        title="My Parcels"
      />

      {results.isLoading && !results.data && (
        <LoadingSpinner label="Loading your parcels" />
      )}
      {results.error && (
        <ErrorMessage error={results.error} onRetry={refresh} />
      )}

      {parcels && !results.error && (
        <section className="customer-dashboard__detail-card">
          <div className="customer-dashboard__toolbar">
            <SearchBar
              label="Search your parcels by tracking number"
              onChange={updateSearch}
              placeholder="Search tracking number"
              value={search}
            />
            <FormSelect
              id="customer-parcels-status"
              label="Filter by status"
              onChange={updateStatus}
              options={(summary?.availableStatuses ?? []).map((status) => ({
                value: status.statusCode,
                label: status.statusName,
              }))}
              placeholder="All statuses"
              value={statusCode}
            />
          </div>

          <p className="customer-dashboard__results-count" role="status">
            {parcels.totalRecords} {parcels.totalRecords === 1 ? 'parcel' : 'parcels'} found
          </p>

          <DataTable
            caption="Your parcels"
            columns={[
              { key: 'trackingNumber', header: 'Tracking Number' },
              { key: 'senderName', header: 'Sender' },
              { key: 'receiverName', header: 'Receiver', render: (row) => row.receiverName || '—' },
              { key: 'originHubName', header: 'Origin Hub', render: (row) => row.originHubName || '—' },
              { key: 'destinationHubName', header: 'Destination Hub', render: (row) => row.destinationHubName || '—' },
              { key: 'bookingDate', header: 'Booking Date', render: (row) => formatDate(row.bookingDate) },
              { key: 'expectedDeliveryDate', header: 'Expected Delivery', render: (row) => formatDate(row.expectedDeliveryDate) },
              { key: 'currentStatusName', header: 'Current Status', render: (row) => <StatusBadge status={row.currentStatusName} /> },
            ]}
            emptyState={(
              <EmptyState
                description={search || statusCode
                  ? 'Try another tracking number or status.'
                  : 'No parcels are associated with your customer account yet.'}
                title="No parcels found"
              />
            )}
            getRowKey={(row) => row.parcelId}
            isLoading={results.isLoading}
            rowActions={(row) => (
              <Link
                className="customer-dashboard__view-link"
                to={`/dashboard/customer/parcels/${row.parcelId}`}
              >
                View Details
              </Link>
            )}
            rows={parcels.items ?? []}
          />
          <Pagination
            currentPage={page}
            onPageChange={setPage}
            pageCount={parcels.totalPages}
            pageSize={parcels.pageSize}
            totalRecords={parcels.totalRecords}
          />
        </section>
      )}
    </div>
  )
}

export default CustomerParcelsPage
