import { useCallback, useState } from 'react'
import { Package, PackageCheck } from 'lucide-react'
import { Link } from 'react-router-dom'
import DashboardStatCard from '../../components/cards/DashboardStatCard'
import DashboardSection from '../../components/common/DashboardSection'
import EmptyState from '../../components/common/EmptyState'
import ErrorMessage from '../../components/common/ErrorMessage'
import LoadingSpinner from '../../components/common/LoadingSpinner'
import PageHeader from '../../components/common/PageHeader'
import RefreshButton from '../../components/common/RefreshButton'
import SearchBar from '../../components/common/SearchBar'
import StatusBadge from '../../components/common/StatusBadge'
import DataTable from '../../components/tables/DataTable'
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

function CustomerDashboard() {
  const [filter, setFilter] = useState('')
  const [page, setPage] = useState(1)
  const [search, setSearch] = useState('')

  const loadDashboard = useCallback(async (signal) => {
    const [summary, parcels] = await Promise.all([
      getCustomerDashboardStats(signal),
      getCustomerParcels({
        page,
        pageSize: PAGE_SIZE,
        search,
        statusCode: filter || undefined,
        signal,
      }),
    ])

    return { summary, parcels }
  }, [filter, page, search])

  const dashboard = useDashboardData(loadDashboard)
  const summary = dashboard.data?.summary
  const parcels = dashboard.data?.parcels

  function applyFilter(statusCode) {
    setPage(1)
    setFilter(statusCode)
  }

  return (
    <div className="role-dashboard role-dashboard--customer">
      <PageHeader
        actions={<RefreshButton isLoading={dashboard.isLoading} onRefresh={dashboard.refresh} />}
        description="Track your parcel bookings and follow their delivery progress."
        eyebrow="Customer Viewer"
        title="Customer Dashboard"
      />

      {dashboard.isLoading && !dashboard.data && <LoadingSpinner label="Loading your parcels" />}
      {dashboard.error && <ErrorMessage error={dashboard.error} onRetry={dashboard.refresh} />}

      {summary && (
        <div className="dashboard-stat-grid">
          <DashboardStatCard
            icon={Package}
            label="Total Parcels"
            onClick={() => applyFilter('')}
            value={summary.totalParcels}
          />
          <DashboardStatCard
            accent="amber"
            icon={Package}
            label="Booked Parcels"
            onClick={() => applyFilter('BOOKED')}
            value={summary.bookedParcels}
          />
          <DashboardStatCard
            accent="green"
            icon={PackageCheck}
            label="Delivered"
            onClick={() => applyFilter('DELIVERED')}
            value={summary.deliveredParcels}
          />
        </div>
      )}

      {parcels && !dashboard.error && (
        <DashboardSection
          description="Parcels booked under your customer account."
          title={filter ? `${filter === 'BOOKED' ? 'Booked' : 'Delivered'} Parcels` : 'My Parcels'}
        >
          <div className="customer-dashboard__toolbar">
            <SearchBar
              label="Search your parcels"
              onChange={(value) => {
                setPage(1)
                setSearch(value)
              }}
              placeholder="Search tracking number, hub, or status"
              value={search}
            />
            {filter && (
              <button
                className="customer-dashboard__all-button"
                onClick={() => applyFilter('')}
                type="button"
              >
                All Parcels
              </button>
            )}
          </div>

          <DataTable
            caption="Your parcels"
            columns={[
              { key: 'trackingNumber', header: 'Tracking Number' },
              { key: 'bookingDate', header: 'Booking Date', render: (row) => formatDate(row.bookingDate) },
              { key: 'originHubName', header: 'Origin Hub', render: (row) => row.originHubName || '—' },
              { key: 'destinationHubName', header: 'Destination Hub', render: (row) => row.destinationHubName || '—' },
              { key: 'totalWeightKg', header: 'Total Weight', render: (row) => `${row.totalWeightKg} kg` },
              { key: 'currentStatusName', header: 'Current Status', render: (row) => <StatusBadge status={row.currentStatusName} /> },
              { key: 'expectedDeliveryDate', header: 'Expected Delivery', render: (row) => formatDate(row.expectedDeliveryDate) },
            ]}
            emptyState={(
              <EmptyState
                description={search ? 'Try another tracking number, hub, or status.' : 'No parcels match this selection.'}
                title="No parcels found"
              />
            )}
            getRowKey={(row) => row.parcelId}
            isLoading={dashboard.isLoading}
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
        </DashboardSection>
      )}
    </div>
  );
}

export default CustomerDashboard;
