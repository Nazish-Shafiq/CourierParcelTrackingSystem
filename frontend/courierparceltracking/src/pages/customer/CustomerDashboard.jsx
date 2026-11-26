import { useCallback, useState } from 'react'
import { Package, PackageCheck } from 'lucide-react'
import { Link, useNavigate } from 'react-router-dom'
import DashboardStatCard from '../../components/cards/DashboardStatCard'
import Button from '../../components/common/Button'
import DashboardSection from '../../components/common/DashboardSection'
import DashboardViewAll, { DASHBOARD_PREVIEW_LIMIT } from '../../components/common/DashboardViewAll'
import EmptyState from '../../components/common/EmptyState'
import ErrorMessage from '../../components/common/ErrorMessage'
import LoadingSpinner from '../../components/common/LoadingSpinner'
import PageHeader from '../../components/common/PageHeader'
import RefreshButton from '../../components/common/RefreshButton'
import SearchBar from '../../components/common/SearchBar'
import StatusBadge from '../../components/common/StatusBadge'
import FormInput from '../../components/forms/FormInput'
import DataTable from '../../components/tables/DataTable'
import { useDashboardData } from '../../hooks/useDashboardData'
import {
  getCustomerParcelByTrackingNumber,
  getCustomerDashboardStats,
  getCustomerParcels,
} from '../../services/customerDashboardService'
import '../../style/customer/customer-dashboard.css'

const PAGE_SIZE = DASHBOARD_PREVIEW_LIMIT

function formatDate(value) {
  return value ? new Date(value).toLocaleDateString() : '—'
}

function CustomerDashboard() {
  const navigate = useNavigate()
  const [filter, setFilter] = useState('')
  const [search, setSearch] = useState('')
  const [trackingNumber, setTrackingNumber] = useState('')
  const [trackError, setTrackError] = useState(null)
  const [isTracking, setIsTracking] = useState(false)

  const loadDashboard = useCallback(async (signal) => {
    const [summary, parcels] = await Promise.all([
      getCustomerDashboardStats(signal),
      getCustomerParcels({
        page: 1,
        pageSize: PAGE_SIZE,
        search,
        statusCode: filter || undefined,
        signal,
      }),
    ])

    return { summary, parcels }
  }, [filter, search])

  const dashboard = useDashboardData(loadDashboard)
  const summary = dashboard.data?.summary
  const parcels = dashboard.data?.parcels

  function applyFilter(statusCode) {
    setFilter(statusCode)
  }

  async function handleTrackParcel(event) {
    event.preventDefault()
    const value = trackingNumber.trim()
    if (!value) {
      setTrackError(new Error('Enter a tracking number to track your parcel.'))
      return
    }

    setTrackError(null)
    setIsTracking(true)
    try {
      const parcel = await getCustomerParcelByTrackingNumber(value)
      navigate(`/dashboard/customer/parcels/${parcel.parcelId}`)
    } catch (error) {
      setTrackError(error)
    } finally {
      setIsTracking(false)
    }
  }

  const viewAllParams = new URLSearchParams()
  if (search.trim()) viewAllParams.set('search', search.trim())
  if (filter) viewAllParams.set('statusCode', filter)
  const viewAllQuery = viewAllParams.toString()
  const viewAllPath = `/customer/parcels${viewAllQuery ? `?${viewAllQuery}` : ''}`

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

      <DashboardSection
        description="Enter a tracking number to view its current status and complete history."
        title="Track Parcel"
      >
        <form className="customer-dashboard__track-form" onSubmit={handleTrackParcel}>
          <FormInput
            autoComplete="off"
            id="customer-tracking-number"
            label="Tracking number"
            onChange={(event) => setTrackingNumber(event.target.value)}
            placeholder="Enter your tracking number"
            value={trackingNumber}
          />
          <Button isLoading={isTracking} type="submit">
            Track Parcel
          </Button>
        </form>
        {trackError && (
          <ErrorMessage
            error={trackError}
            title="Unable to track parcel"
          />
        )}
      </DashboardSection>

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
              { key: 'originHubName', header: 'Origin Hub', render: (row) => row.originHubName || 'Not Available' },
              { key: 'destinationHubName', header: 'Destination Hub', render: (row) => row.destinationHubName || 'Not Available' },
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
            rows={(parcels.items ?? []).slice(0, DASHBOARD_PREVIEW_LIMIT)}
          />
          <DashboardViewAll
            count={parcels.totalRecords}
            label="View all matching parcels"
            to={viewAllPath}
          />
        </DashboardSection>
      )}
    </div>
  );
}

export default CustomerDashboard;
