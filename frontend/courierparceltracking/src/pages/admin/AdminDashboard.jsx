import { useCallback, useState } from 'react'
import { Building2, Package, Route, Users } from 'lucide-react'
import { Link, useNavigate } from 'react-router-dom'
import DashboardStatCard from '../../components/cards/DashboardStatCard'
import DashboardSection from '../../components/common/DashboardSection'
import DashboardViewAll, { DASHBOARD_PREVIEW_LIMIT } from '../../components/common/DashboardViewAll'
import EmptyState from '../../components/common/EmptyState'
import ErrorMessage from '../../components/common/ErrorMessage'
import LoadingSpinner from '../../components/common/LoadingSpinner'
import PageHeader from '../../components/common/PageHeader'
import RefreshButton from '../../components/common/RefreshButton'
import StatusBadge from '../../components/common/StatusBadge'
import DataTable from '../../components/tables/DataTable'
import { ROLE_NAMES, ROLE_NAVIGATION } from '../../constants/navigation'
import { useDashboardData } from '../../hooks/useDashboardData'
import {
  getAdminDashboard,
  getCourierPerformanceReport,
} from '../../services/dashboardService'
import { getUsers } from '../../services/userService'
import {
  getAdminDelayModelStatus,
  trainParcelDelayModel,
} from '../../services/parcelDelayPredictionService'
import '../../style/admin/admin-dashboard.css'
import '../../style/admin/ai-operations.css'

function formatDate(value) {
  return value ? new Date(value).toLocaleDateString() : '—'
}

function AdminDashboard() {
  const navigate = useNavigate()
  const loadDashboard = useCallback((signal) => getAdminDashboard(signal), [])
  const loadPerformanceReport = useCallback(
    (signal) => getCourierPerformanceReport({ signal }),
    [],
  )
  const loadUsers = useCallback((signal) => getUsers(signal), [])
  const dashboard = useDashboardData(loadDashboard)
  const performanceReport = useDashboardData(loadPerformanceReport)
  const users = useDashboardData(loadUsers)

  function refreshAll() {
    dashboard.refresh()
    performanceReport.refresh()
    users.refresh()
  }

  function countUsersForRole(roleName) {
    if (!Array.isArray(users.data)) return '—'
    return users.data.filter((user) => (
      user.isActive !== false
      &&
      user.roles.some((role) => role.toLowerCase() === roleName.toLowerCase())
    )).length
  }

  return (
    <div className="role-dashboard role-dashboard--admin">
      <PageHeader
        actions={(
          <RefreshButton
            isLoading={dashboard.isLoading || performanceReport.isLoading || users.isLoading}
            onRefresh={refreshAll}
          />
        )}
        description="Operational metrics, parcel activity, and courier performance from the backend."
        eyebrow="Administration"
        title="Admin Dashboard"
      />

      {dashboard.isLoading && !dashboard.data && <LoadingSpinner label="Loading admin dashboard" />}
      {dashboard.error && <ErrorMessage error={dashboard.error} onRetry={dashboard.refresh} />}
      {users.error && (
        <ErrorMessage error={users.error} onRetry={users.refresh} />
      )}

      {dashboard.data && (
        <>
          <DashboardSection
            description="Delegate operational screen responsibilities to active Booking Agents, Dispatchers, and Couriers."
            title="Screen assignment"
          >
            <Link className="btn btn-outline-primary" to="/admin/screen-assignments">
              Manage screen assignments
            </Link>
          </DashboardSection>
          <div className="dashboard-stat-grid">
            <DashboardStatCard
              icon={Package}
              label="Total parcels"
              onClick={() => navigate('/admin/parcels')}
              value={dashboard.data.totalParcels}
            />
            <DashboardStatCard
              accent="green"
              icon={Users}
              label="Total customers"
              onClick={() => navigate('/admin/customers?active=true')}
              value={dashboard.data.totalCustomers}
            />
            <DashboardStatCard
              accent="violet"
              icon={Users}
              label="Total couriers"
              onClick={() => navigate('/admin/couriers')}
              value={dashboard.data.totalCouriers}
            />
            <DashboardStatCard
              accent="amber"
              icon={Building2}
              label="Total hubs"
              onClick={() => navigate('/admin/hubs?active=true')}
              value={dashboard.data.totalHubs}
            />
            <DashboardStatCard
              icon={Route}
              label="Total routes"
              onClick={() => navigate('/admin/routes?active=true')}
              value={dashboard.data.totalRoutes}
            />
            <DashboardStatCard
              icon={Users}
              label="Active booking agents"
              onClick={() => navigate('/admin/users?role=Booking%20Agent&active=true')}
              value={countUsersForRole(ROLE_NAMES.BOOKING_AGENT)}
            />
            <DashboardStatCard
              accent="violet"
              icon={Users}
              label="Active dispatchers"
              onClick={() => navigate('/admin/users?role=Dispatcher&active=true')}
              value={countUsersForRole(ROLE_NAMES.DISPATCHER)}
            />
            <DashboardStatCard
              accent="green"
              label="Available couriers"
              onClick={() => navigate('/admin/couriers?availability=available&active=true')}
              value={dashboard.data.availableCouriers}
            />
            <DashboardStatCard
              accent="amber"
              label="Busy couriers"
              onClick={() => navigate('/admin/couriers?availability=busy&active=true')}
              value={dashboard.data.busyCouriers}
            />
          </div>

          <DashboardSection
            description="Counts for each active parcel status returned by the API."
            title="Operational summary"
          >
            {dashboard.data.parcelStatusSummary.length === 0 ? (
              <p className="dashboard-inline-empty">No parcel status data is available.</p>
            ) : (
              <div className="dashboard-status-grid">
                {dashboard.data.parcelStatusSummary.map((status) => (
                  <button
                    className="dashboard-status-card stat-card--interactive"
                    key={status.parcelStatusId}
                    onClick={() => navigate(`/admin/parcels?status=${encodeURIComponent(status.statusName)}`)}
                    type="button"
                  >
                    <StatusBadge status={status.statusName} />
                    <strong>{status.totalParcels}</strong>
                  </button>
                ))}
              </div>
            )}
          </DashboardSection>

          <DashboardSection title="Recent parcel bookings">
            {dashboard.data.recentBookings.length === 0
              ? <EmptyState description="No recent parcel bookings were returned." title="No recent bookings" />
              : (
                <>
                  <DataTable
                    caption="Recent parcel bookings"
                    columns={[
                      { key: 'trackingNumber', header: 'Tracking number' },
                      { key: 'senderName', header: 'Sender' },
                      { key: 'statusName', header: 'Status', render: (row) => <StatusBadge status={row.statusName} /> },
                      { key: 'bookingDate', header: 'Booked', render: (row) => formatDate(row.bookingDate) },
                      { key: 'expectedDeliveryDate', header: 'Expected delivery', render: (row) => formatDate(row.expectedDeliveryDate) },
                    ]}
                    rows={dashboard.data.recentBookings.slice(0, DASHBOARD_PREVIEW_LIMIT)}
                    getRowKey={(row) => row.parcelId}
                  />
                  <DashboardViewAll
                    count={dashboard.data.totalParcels}
                    label="View all parcel bookings"
                    to="/admin/parcels"
                  />
                </>
              )}
          </DashboardSection>
        </>
      )}

      <AiOperationsSection />

      <DashboardSection
        description="Assignment and delivery totals from the courier-performance report."
        title="Courier performance overview"
      >
        {performanceReport.isLoading && !performanceReport.data && (
          <LoadingSpinner label="Loading courier performance" size="small" />
        )}
        {performanceReport.error && (
          <ErrorMessage error={performanceReport.error} onRetry={performanceReport.refresh} />
        )}
        {performanceReport.data && (
          performanceReport.data.length === 0
            ? <EmptyState description="No courier performance records were returned." title="No performance data" />
            : (
              <>
                <DataTable
                  caption="Courier assignment and delivery totals"
                  columns={[
                    { key: 'courierName', header: 'Courier' },
                    { key: 'employeeCode', header: 'Employee code' },
                    { key: 'totalAssignedParcels', header: 'Assigned parcels' },
                    { key: 'deliveredParcels', header: 'Delivered parcels' },
                  ]}
                  rows={performanceReport.data.slice(0, DASHBOARD_PREVIEW_LIMIT)}
                  getRowKey={(row) => row.courierId}
                />
                <DashboardViewAll
                  count={performanceReport.data.length}
                  label="View all courier performance"
                  to="/admin/reports?report=couriers"
                />
              </>
            )
        )}
      </DashboardSection>

      <DashboardSection title="Quick navigation">
        <div className="admin-quick-links">
          {ROLE_NAVIGATION[ROLE_NAMES.ADMIN].map(({ icon: Icon, label, path }) => (
            <Link className="admin-quick-link" key={path} to={path}>
              <span className="admin-quick-link__icon">
                <Icon aria-hidden="true" size={19} />
              </span>
              <span>{label}</span>
            </Link>
          ))}
        </div>
      </DashboardSection>
    </div>
  )
}

function AiOperationsSection() {
  const [isTraining, setIsTraining] = useState(false)
  const [trainingError, setTrainingError] = useState(null)
  const loadStatus = useCallback((signal) => getAdminDelayModelStatus(signal), [])
  const status = useDashboardData(loadStatus)

  async function handleTrain() {
    setIsTraining(true)
    setTrainingError(null)
    try {
      await trainParcelDelayModel()
      status.refresh()
    } catch (error) {
      setTrainingError(error)
    } finally {
      setIsTraining(false)
    }
  }

  const modelStatus = status.data
  const trainingDisabled = !modelStatus?.trainingReady || isTraining || status.isLoading

  return (
    <DashboardSection
      action={(
        <button
          className="btn btn-primary"
          disabled={trainingDisabled}
          onClick={handleTrain}
          type="button"
        >
          {isTraining ? 'Training…' : 'Train / re-evaluate model'}
        </button>
      )}
      description="Model readiness, held-out evaluation, and recorded prediction activity."
      title="AI operations"
    >
      {status.isLoading && !modelStatus && (
        <LoadingSpinner label="Loading AI model status" size="small" />
      )}
      {status.error && (
        <ErrorMessage error={status.error} onRetry={status.refresh} />
      )}
      {trainingError && <ErrorMessage error={trainingError} />}
      {modelStatus && (
        <>
          <p className="ai-operations__readiness" role="status">
            {modelStatus.isModelAvailable
              ? modelStatus.evaluationAvailable
                ? `Evaluated model available. ${modelStatus.algorithm}.`
                : `Trained model available without a held-out evaluation. ${modelStatus.algorithm}.`
              : 'No trained delay model is currently available.'}
            {' '}
            {modelStatus.readinessMessage}
            {modelStatus.lastTrainingStatus && (
              <>
                {' '}
                Latest training attempt: {modelStatus.lastTrainingStatus.toLowerCase()}.
                {modelStatus.lastTrainingMessage && ` ${modelStatus.lastTrainingMessage}`}
              </>
            )}
          </p>
          <p className="ai-operations__evaluation" role="status">
            {modelStatus.evaluationMessage}
          </p>
          <div className="ai-operations__grid">
            <div><span>Total completed deliveries</span><strong>{modelStatus.totalCompletedDeliveries}</strong></div>
            <div><span>On-time deliveries</span><strong>{modelStatus.onTimeDeliveries}</strong></div>
            <div><span>Delayed deliveries</span><strong>{modelStatus.delayedDeliveries}</strong></div>
            <div><span>On-time delivery rate</span><strong>{formatPercent(modelStatus.onTimeDeliveryPercentage)}</strong></div>
            <div><span>Delayed delivery rate</span><strong>{formatPercent(modelStatus.delayedDeliveryPercentage)}</strong></div>
            <div><span>Average delivery duration</span><strong>{formatDuration(modelStatus.averageDeliveryDurationHours)}</strong></div>
          </div>
          <h3 className="ai-operations__subheading">Training and evaluation</h3>
          <div className="ai-operations__grid">
            <div><span>Valid training records</span><strong>{modelStatus.validTrainingSamples}</strong></div>
            <div><span>On-time / delayed</span><strong>{modelStatus.onTimeSamples} / {modelStatus.delayedSamples}</strong></div>
            <div><span>Completed predictions</span><strong>{modelStatus.completedPredictionCount}</strong></div>
            <div><span>Training status</span><strong>{modelStatus.lastTrainingStatus ?? 'Not attempted'}</strong></div>
            <div><span>Last trained</span><strong>{formatDate(modelStatus.lastTrainedAt)}</strong></div>
            <div><span>Accuracy</span><strong>{formatEvaluationMetric(modelStatus.accuracy, modelStatus.evaluationAvailable)}</strong></div>
            <div><span>ROC AUC</span><strong>{formatEvaluationMetric(modelStatus.areaUnderRocCurve, modelStatus.evaluationAvailable)}</strong></div>
            <div><span>F1 score</span><strong>{formatEvaluationMetric(modelStatus.f1Score, modelStatus.evaluationAvailable)}</strong></div>
            <div><span>Precision / recall</span><strong>{formatEvaluationMetric(modelStatus.precision, modelStatus.evaluationAvailable)} / {formatEvaluationMetric(modelStatus.recall, modelStatus.evaluationAvailable)}</strong></div>
          </div>
        </>
      )}
    </DashboardSection>
  )
}

function formatMetric(value) {
  return Number.isFinite(value) ? `${(value * 100).toFixed(1)}%` : '—'
}

function formatEvaluationMetric(value, evaluationAvailable) {
  return evaluationAvailable ? formatMetric(value) : 'Unavailable'
}

function formatPercent(value) {
  return Number.isFinite(value) ? `${value.toFixed(1)}%` : '—'
}

function formatDuration(value) {
  return Number.isFinite(value) ? `${value.toFixed(1)} hours` : '—'
}

export default AdminDashboard
