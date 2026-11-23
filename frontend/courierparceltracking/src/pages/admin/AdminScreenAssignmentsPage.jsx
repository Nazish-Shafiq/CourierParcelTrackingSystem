import { useEffect, useMemo, useState } from 'react'
import Button from '../../components/common/Button'
import DashboardSection from '../../components/common/DashboardSection'
import EmptyState from '../../components/common/EmptyState'
import ErrorMessage from '../../components/common/ErrorMessage'
import LoadingSpinner from '../../components/common/LoadingSpinner'
import PageHeader from '../../components/common/PageHeader'
import RefreshButton from '../../components/common/RefreshButton'
import DataTable from '../../components/tables/DataTable'
import { useDashboardData } from '../../hooks/useDashboardData'
import {
  assignScreen,
  getEligibleScreenUsers,
  getActiveScreens,
  getScreenAssignments,
  reassignScreen,
  revokeScreenAssignment,
} from '../../services/screenAssignmentService'

const EMPTY_ASSIGNMENTS = []

function formatDate(value) {
  return value ? new Date(value).toLocaleString() : '—'
}

function AdminScreenAssignmentsPage() {
  const [assignmentScreenId, setAssignmentScreenId] = useState('')
  const [filterScreenId, setFilterScreenId] = useState('')
  const [selectedUserId, setSelectedUserId] = useState('')
  const [replacementUsers, setReplacementUsers] = useState({})
  const [replacementCandidatesByScreen, setReplacementCandidatesByScreen] = useState({})
  const [loadingReplacementScreenIds, setLoadingReplacementScreenIds] = useState(new Set())
  const [isSaving, setIsSaving] = useState(false)
  const [actionError, setActionError] = useState(null)
  const [success, setSuccess] = useState('')
  const [reloadKey, setReloadKey] = useState(0)
  const [eligibleUserResult, setEligibleUserResult] = useState({
    screenId: '',
    users: [],
    error: null,
    reloadKey: -1,
  })

  const screenData = useDashboardData(getActiveScreens)
  const assignmentData = useDashboardData(getScreenAssignments)
  const screens = screenData.data ?? []
  const assignments = assignmentData.data ?? EMPTY_ASSIGNMENTS
  const hasEligibleUserResult = Boolean(assignmentScreenId) &&
    eligibleUserResult.screenId === assignmentScreenId &&
    eligibleUserResult.reloadKey === reloadKey
  const eligibleUsers = hasEligibleUserResult
    ? eligibleUserResult.users
    : []
  const eligibleUsersError = hasEligibleUserResult
    ? eligibleUserResult.error
    : null
  const isLoadingEligibleUsers = Boolean(assignmentScreenId) &&
    (!hasEligibleUserResult || eligibleUserResult.isLoading)
  const error = actionError || screenData.error || assignmentData.error || eligibleUsersError
  const isLoading = screenData.isLoading || assignmentData.isLoading
  const availableEligibleUsers = eligibleUsers.filter((user) => !assignments.some((assignment) => (
    assignment.isActive &&
    String(assignment.screenId) === assignmentScreenId &&
    assignment.userId === user.userId
  )))
  const selectedScreen = screens.find((screen) => (
    String(screen.screenId) === assignmentScreenId
  ))
  const hasSelectedEligibleUser = availableEligibleUsers.some((user) => (
    String(user.userId) === selectedUserId
  ))
  const canAssign = Boolean(
    selectedScreen &&
    selectedScreen.isAssignable &&
    hasSelectedEligibleUser &&
    !isLoadingEligibleUsers &&
    !isSaving &&
    !screenData.error &&
    !assignmentData.error &&
    !eligibleUsersError
  )

  useEffect(() => {
    if (!assignmentScreenId) return undefined

    const controller = new AbortController()
    getEligibleScreenUsers(assignmentScreenId, controller.signal)
      .then((users) => {
        if (!controller.signal.aborted) {
          setEligibleUserResult({
            screenId: assignmentScreenId,
            users,
            error: null,
            reloadKey,
          })
        }
      })
      .catch((requestError) => {
        if (!controller.signal.aborted) {
          setEligibleUserResult({
            screenId: assignmentScreenId,
            users: [],
            error: requestError,
            reloadKey,
          })
        }
      })
    return () => controller.abort()
  }, [assignmentScreenId, reloadKey])

  const filteredAssignments = useMemo(() => (
    filterScreenId
      ? assignments.filter((assignment) => (
        String(assignment.screenId) === String(filterScreenId)
      ))
      : assignments
  ), [assignments, filterScreenId])

  function reloadCurrentData() {
    setReplacementCandidatesByScreen({})
    screenData.refresh()
    assignmentData.refresh()
    setReloadKey((key) => key + 1)
  }

  function refresh() {
    setSuccess('')
    setActionError(null)
    reloadCurrentData()
  }

  async function handleAssign(event) {
    event.preventDefault()
    setIsSaving(true)
    setActionError(null)
    setSuccess('')
    try {
      await assignScreen({
        screenId: Number(assignmentScreenId),
        userId: Number(selectedUserId),
      })
      setSelectedUserId('')
      setSuccess('Screen assigned successfully.')
      reloadCurrentData()
    } catch (requestError) {
      setActionError(requestError)
    } finally {
      setIsSaving(false)
    }
  }

  async function handleReassign(assignment) {
    const userId = replacementUsers[assignment.permissionId]
    if (!userId) return

    setIsSaving(true)
    setActionError(null)
    setSuccess('')
    try {
      await reassignScreen(assignment.permissionId, Number(userId))
      setReplacementUsers((current) => ({
        ...current,
        [assignment.permissionId]: '',
      }))
      setSuccess(`Responsibility for ${assignment.screenName} reassigned.`)
      reloadCurrentData()
    } catch (requestError) {
      setActionError(requestError)
    } finally {
      setIsSaving(false)
    }
  }

  async function handleRevoke(assignment) {
    setIsSaving(true)
    setActionError(null)
    setSuccess('')
    try {
      await revokeScreenAssignment(assignment.permissionId)
      setSuccess(`Assignment for ${assignment.screenName} revoked.`)
      reloadCurrentData()
    } catch (requestError) {
      setActionError(requestError)
    } finally {
      setIsSaving(false)
    }
  }

  function getReplacementOptions(assignment) {
    return (replacementCandidatesByScreen[assignment.screenId] ?? []).filter((user) => (
      user.userId !== assignment.userId &&
      !assignments.some((other) => (
        other.isActive &&
        other.screenId === assignment.screenId &&
        other.userId === user.userId
      ))
    ))
  }

  async function loadReplacementCandidates(screenId) {
    const key = String(screenId)
    if (
      Object.prototype.hasOwnProperty.call(replacementCandidatesByScreen, key) ||
      loadingReplacementScreenIds.has(key)
    ) {
      return
    }

    setLoadingReplacementScreenIds((current) => new Set(current).add(key))
    try {
      const users = await getEligibleScreenUsers(screenId)
      setReplacementCandidatesByScreen((current) => ({
        ...current,
        [key]: users,
      }))
    } catch (requestError) {
      setActionError(requestError)
    } finally {
      setLoadingReplacementScreenIds((current) => {
        const next = new Set(current)
        next.delete(key)
        return next
      })
    }
  }

  return (
    <div>
      <PageHeader
        actions={(
          <RefreshButton isLoading={isLoading} onRefresh={refresh} />
        )}
        description="Grant, reassign, or revoke individual operational screens for active staff. Existing role dashboards and permissions remain unchanged."
        eyebrow="Administration"
        title="Screen Assignments"
      />

      {error && <ErrorMessage error={error} onRetry={refresh} />}
      {success && <div className="alert alert-success" role="status">{success}</div>}
      {isLoading && <LoadingSpinner label="Loading screens and assignments" />}

      {!isLoading && (
        <>
          <DashboardSection
            description="All active screens are listed by name. Only operational screens can be assigned to active Booking Agent, Dispatcher, or Courier users without original role access. Courier screens additionally require an active Courier profile and remain scoped to that user's courier work."
            title="Assign additional screen access"
          >
            <form className="row g-3 align-items-end" onSubmit={handleAssign}>
              <div className="col-md-5">
                <label className="form-label" htmlFor="screen-assignment-screen">
                  Additional operational screen
                </label>
                <select
                  className="form-select"
                  id="screen-assignment-screen"
                  onChange={(event) => {
                    setAssignmentScreenId(event.target.value)
                    setSelectedUserId('')
                  }}
                  required
                  value={assignmentScreenId}
                >
                  <option value="">Select a screen</option>
                  {screens.map((screen) => (
                    <option
                      disabled={!screen.isAssignable}
                      key={screen.screenId}
                      value={screen.screenId}
                    >
                      {screen.screenName}
                    </option>
                  ))}
                </select>
              </div>
              <div className="col-md-5">
                <label className="form-label" htmlFor="screen-assignment-user">
                  Active user
                </label>
                <select
                  className="form-select"
                  id="screen-assignment-user"
                  disabled={!assignmentScreenId || isLoadingEligibleUsers}
                  onChange={(event) => setSelectedUserId(event.target.value)}
                  required
                  value={selectedUserId}
                >
                  <option value="">Select an eligible user</option>
                  {availableEligibleUsers.map((user) => (
                    <option key={user.userId} value={user.userId}>
                      {user.fullName} ({user.roleNames.join(', ')})
                    </option>
                  ))}
                </select>
              </div>
              <div className="col-md-2">
                <Button
                  className="w-100"
                  disabled={!canAssign}
                  isLoading={isSaving}
                  type="submit"
                >
                  Assign
                </Button>
              </div>
            </form>
            {!screenData.isLoading && !screenData.error && !screens.length && (
              <p className="mt-3 mb-0">No active operational screens are available.</p>
            )}
            {isLoadingEligibleUsers && (
              <p className="mt-3 mb-0" role="status">Loading eligible staff accounts…</p>
            )}
            {hasEligibleUserResult &&
              !eligibleUsersError &&
              !eligibleUsers.length && (
              <p className="mt-3 mb-0">No active eligible staff accounts are available.</p>
            )}
          </DashboardSection>

          <DashboardSection
            description="Assignment history is retained after reassignment or revocation."
            title="Current and historical assignments"
          >
            <div className="mb-3">
              <label className="form-label" htmlFor="screen-assignment-filter">
                Filter by screen
              </label>
              <select
                className="form-select"
                id="screen-assignment-filter"
                onChange={(event) => setFilterScreenId(event.target.value)}
                value={filterScreenId}
              >
                <option value="">All screens</option>
                {screens.map((screen) => (
                  <option key={screen.screenId} value={screen.screenId}>
                    {screen.moduleName} — {screen.screenName}
                  </option>
                ))}
              </select>
            </div>
            {!assignmentData.error && (
              <DataTable
                caption="Screen assignment history"
                columns={[
                  { key: 'screenName', header: 'Screen' },
                  { key: 'assignedUserFullName', header: 'Assigned user' },
                  {
                    key: 'assignedUserRoles',
                    header: 'Original roles',
                    render: (row) => row.assignedUserRoles.join(', ') || '—',
                  },
                  { key: 'assignedAt', header: 'Assigned', render: (row) => formatDate(row.assignedAt) },
                  { key: 'status', header: 'Status' },
                ]}
                emptyState={<EmptyState description="No assignments match this screen." title="No screen assignments" />}
                getRowKey={(row) => row.permissionId}
                rows={filteredAssignments}
                rowActions={(assignment) => (
                  assignment.isActive ? (
                    <div className="d-flex flex-column gap-2">
                      <select
                        aria-label={`Replacement user for ${assignment.screenName}`}
                        className="form-select"
                        onFocus={() => loadReplacementCandidates(assignment.screenId)}
                        onChange={(event) => setReplacementUsers((current) => ({
                          ...current,
                          [assignment.permissionId]: event.target.value,
                        }))}
                        value={replacementUsers[assignment.permissionId] ?? ''}
                      >
                        <option value="">
                          {loadingReplacementScreenIds.has(String(assignment.screenId))
                            ? 'Loading eligible users…'
                            : 'Reassign to…'}
                        </option>
                        {getReplacementOptions(assignment).map((user) => (
                          <option key={user.userId} value={user.userId}>
                            {user.fullName} ({user.roleNames.join(', ')})
                          </option>
                        ))}
                      </select>
                      <Button
                        disabled={!replacementUsers[assignment.permissionId]}
                        isLoading={isSaving}
                        onClick={() => handleReassign(assignment)}
                        variant="outline"
                      >
                        Reassign
                      </Button>
                      <Button
                        isLoading={isSaving}
                        onClick={() => handleRevoke(assignment)}
                        variant="outline"
                      >
                        Revoke
                      </Button>
                    </div>
                  ) : <span>{assignment.revokedAt ? `Revoked ${formatDate(assignment.revokedAt)}` : 'Inactive'}</span>
                )}
              />
            )}
          </DashboardSection>
        </>
      )}
    </div>
  )
}

export default AdminScreenAssignmentsPage
