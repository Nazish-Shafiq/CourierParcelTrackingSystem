import { useCallback, useMemo, useState } from "react";
import { Link, useParams, useSearchParams } from "react-router-dom";
import Button from "../../components/common/Button";
import ConfirmDialog from "../../components/common/ConfirmDialog";
import EmptyState from "../../components/common/EmptyState";
import ErrorMessage from "../../components/common/ErrorMessage";
import LoadingSpinner from "../../components/common/LoadingSpinner";
import PageHeader from "../../components/common/PageHeader";
import RefreshButton from "../../components/common/RefreshButton";
import SearchBar from "../../components/common/SearchBar";
import StatusBadge from "../../components/common/StatusBadge";
import FormSelect from "../../components/forms/FormSelect";
import FormTextArea from "../../components/forms/FormTextArea";
import DataTable from "../../components/tables/DataTable";
import Pagination from "../../components/tables/Pagination";
import { useDashboardData } from "../../hooks/useDashboardData";
import { getCourierDashboard } from "../../services/dashboardService";
import {
  getTrackingHistory,
  updateCourierParcelStatus,
} from "../../services/trackingService";
import "../../style/courier/courier-pages.css";

const PAGE_SIZE = 10;

function getOwnAssignedParcel(dashboard, parcelId) {
  const assignedParcels = [
    ...(Array.isArray(dashboard?.assignedParcels)
      ? dashboard.assignedParcels
      : []),
    ...(Array.isArray(dashboard?.deliveryHistory)
      ? dashboard.deliveryHistory
      : []),
  ];
  const parcel = assignedParcels.find(
    (item) => String(item.parcelId) === String(parcelId),
  );

  if (!parcel) {
    throw new Error(
      "This parcel is not assigned to your courier account and is not in its delivery history.",
    );
  }

  return parcel;
}

function formatAddress(...parts) {
  const address = parts.filter(Boolean).join(", ");
  return address || "—";
}

function formatDate(value) {
  if (!value) return "—";
  const date = new Date(value);
  return Number.isNaN(date.getTime()) ? "—" : date.toLocaleString();
}

function formatWeight(value) {
  if (value === null || value === undefined || value === "") return "—";
  const weight = Number(value);
  return Number.isFinite(weight) ? `${weight} kg` : "—";
}

function getCurrentStatusCode(parcel) {
  const statusCode = typeof parcel?.currentStatusCode === 'string'
    ? parcel.currentStatusCode.trim()
    : ''
  const status = statusCode || parcel?.currentStatus
  return typeof status === "string"
    ? status.trim().replace(/\s+/g, "_").toUpperCase()
    : "";
}

function getCourierActions(parcel, events) {
  const statusCode = getCurrentStatusCode(parcel);

  if (statusCode === "BOOKED") {
    return [{ label: "Pick up parcel", statusCode: "PICKED_UP" }];
  }
  if (statusCode === "PICKED_UP") {
    return [{ label: "Mark in transit", statusCode: "IN_TRANSIT" }];
  }
  if (statusCode === "AT_DESTINATION_HUB") {
    return [{ label: "Start delivery", statusCode: "OUT_FOR_DELIVERY" }];
  }
  if (statusCode === "OUT_FOR_DELIVERY") {
    return [
      { label: "Confirm delivery", statusCode: "DELIVERED" },
      { label: "Record failed delivery", statusCode: "DELIVERY_ATTEMPTED" },
    ];
  }
  if (statusCode === "DELIVERY_ATTEMPTED") {
    const latestAttempt = events
      .filter(
        (event) => event.statusCode?.toUpperCase() === "DELIVERY_ATTEMPTED",
      )
      .reduce(
        (latest, event) =>
          !latest || new Date(event.eventAt) > new Date(latest.eventAt)
            ? event
            : latest,
        null,
      );
    const assignmentAt = new Date(parcel.assignedAt).getTime();
    const attemptAt = latestAttempt
      ? new Date(latestAttempt.eventAt).getTime()
      : Number.NaN;

    if (
      Number.isFinite(assignmentAt) &&
      Number.isFinite(attemptAt) &&
      assignmentAt > attemptAt
    ) {
      return [{ label: "Start delivery", statusCode: "OUT_FOR_DELIVERY" }];
    }
  }

  return [];
}

function getStatusActionLabel(statusCode) {
  return (
    {
      PICKED_UP: "Picked up",
      IN_TRANSIT: "In transit",
      OUT_FOR_DELIVERY: "Out for delivery",
      DELIVERED: "Delivered",
      DELIVERY_ATTEMPTED: "Delivery attempted",
    }[statusCode] ?? statusCode
  );
}

function getRequestErrorMessage(error) {
  return (
    error?.response?.data?.message ||
    error?.message ||
    "The status update could not be completed."
  );
}

function CourierParcelWorkflow({ onUpdated, parcel }) {
  const [pendingAction, setPendingAction] = useState(null);
  const [actionNotes, setActionNotes] = useState("");
  const [actionError, setActionError] = useState("");
  const [actionSuccess, setActionSuccess] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);
  const loadHistory = useCallback(
    async (signal) => {
      const events = await getTrackingHistory(parcel.trackingNumber, signal);
      if (!Array.isArray(events)) {
        throw new Error("The tracking history response was not a list.");
      }
      return events
        .slice()
        .sort(
          (left, right) => new Date(left.eventAt) - new Date(right.eventAt),
        );
    },
    [parcel.trackingNumber],
  );
  const history = useDashboardData(loadHistory);
  const events = Array.isArray(history.data) ? history.data : [];
  const actions = getCourierActions(parcel, events);
  const currentStatusCode = getCurrentStatusCode(parcel);

  async function handleConfirm() {
    if (!pendingAction || isSubmitting) return;
    setActionError("");
    setActionSuccess("");
    setIsSubmitting(true);
    try {
      await updateCourierParcelStatus(parcel.parcelId, {
        statusCode: pendingAction.statusCode,
        ...(actionNotes.trim() ? { notes: actionNotes.trim() } : {}),
      });
      setActionSuccess(
        `${getStatusActionLabel(pendingAction.statusCode)} status recorded.`,
      );
      setPendingAction(null);
      setActionNotes("");
      history.refresh();
      onUpdated();
    } catch (error) {
      setActionError(getRequestErrorMessage(error));
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <section className="courier-workflow">
      <div className="courier-workflow__actions">
        <div>
          <h2>Delivery actions</h2>
          <p>
            Only status changes allowed for this parcel and assignment are
            available.
          </p>
        </div>
        {history.isLoading && !history.data && (
          <LoadingSpinner label="Checking parcel tracking history" />
        )}
        {actionError && (
          <p className="courier-workflow__error" role="alert">
            {actionError}
          </p>
        )}
        {actionSuccess && (
          <p className="courier-workflow__success" role="status">
            {actionSuccess}
          </p>
        )}
        {actions.length > 0 ? (
          <div className="courier-workflow__buttons">
            {actions.map((action) => (
              <Button
                disabled={isSubmitting || history.isLoading}
                key={action.statusCode}
                onClick={() => {
                  setActionError("");
                  setPendingAction(action);
                }}
                variant={
                  action.statusCode === "DELIVERY_ATTEMPTED"
                    ? "danger"
                    : "primary"
                }
              >
                {action.label}
              </Button>
            ))}
          </div>
        ) : (
          <p className="courier-api-note">
            {history.error && currentStatusCode === "DELIVERY_ATTEMPTED"
              ? "Tracking history could not be verified. Refresh it before starting another delivery attempt."
              : currentStatusCode === "DELIVERY_ATTEMPTED"
                ? "A Dispatcher must assign or reassign this parcel after the failed attempt before another delivery can start."
                : currentStatusCode === "DELIVERED"
                  ? "Delivery is complete. Delivered parcels cannot be changed."
                  : "No Courier status action is available for the current parcel status."}
          </p>
        )}
      </div>

      <div className="courier-workflow__history">
        <h2>Tracking history</h2>
        {history.error && (
          <ErrorMessage error={history.error} onRetry={history.refresh} />
        )}
        {history.data && events.length === 0 && (
          <EmptyState
            description="No tracking events have been recorded for this parcel."
            title="No tracking history"
          />
        )}
        {events.length > 0 && (
          <div className="courier-tracking-timeline">
            {events.map((event) => (
              <article
                className="courier-tracking-event"
                key={event.statusEventId}
              >
                <div className="courier-tracking-event__marker" />
                <div className="courier-tracking-event__content">
                  <div className="courier-tracking-event__heading">
                    <StatusBadge status={event.statusName} />
                    <time dateTime={event.eventAt}>
                      {formatDate(event.eventAt)}
                    </time>
                  </div>
                  <h2>
                    {event.statusName} <span>({event.statusCode})</span>
                  </h2>
                  {event.notes && <p>{event.notes}</p>}
                  {event.hubName && <p>Hub: {event.hubName}</p>}
                  {event.courierName && <p>Courier: {event.courierName}</p>}
                </div>
              </article>
            ))}
          </div>
        )}
      </div>

      {pendingAction && (
        <ConfirmDialog
          confirmLabel={pendingAction.label}
          confirmVariant={
            pendingAction.statusCode === "DELIVERY_ATTEMPTED"
              ? "danger"
              : "primary"
          }
          description="This will update the parcel status and add an entry to its tracking history."
          isConfirming={isSubmitting}
          onCancel={() => {
            if (!isSubmitting) {
              setPendingAction(null);
              setActionNotes("");
            }
          }}
          onConfirm={handleConfirm}
          title={`Confirm: ${pendingAction.label}`}
        >
          {pendingAction.statusCode === "DELIVERY_ATTEMPTED" && (
            <FormTextArea
              id="courier-delivery-attempt-notes"
              label="Remarks (optional)"
              maxLength={500}
              onChange={(event) => setActionNotes(event.target.value)}
              placeholder="Add delivery attempt details"
              rows={3}
              value={actionNotes}
            />
          )}
        </ConfirmDialog>
      )}
    </section>
  );
}

function CourierAssignedParcelsPage() {
  const [searchParams, setSearchParams] = useSearchParams();
  const [search, setSearch] = useState("");
  const [currentPage, setCurrentPage] = useState(1);
  const statusFilter = searchParams.get("status") ?? "";
  const loadDashboard = useCallback(
    (signal) => getCourierDashboard(signal),
    [],
  );
  const dashboard = useDashboardData(loadDashboard);
  const assignedParcels = useMemo(
    () =>
      Array.isArray(dashboard.data?.assignedParcels)
        ? dashboard.data.assignedParcels
        : [],
    [dashboard.data],
  );

  const statusOptions = useMemo(
    () =>
      [
        ...new Set(
          assignedParcels.map((parcel) => parcel.currentStatus).filter(Boolean),
        ),
      ]
        .sort((left, right) => left.localeCompare(right))
        .map((status) => ({ value: status, label: status })),
    [assignedParcels],
  );

  const filteredParcels = useMemo(() => {
    const query = search.trim().toLowerCase();
    return assignedParcels.filter((parcel) => {
      const matchesStatus =
        !statusFilter || parcel.currentStatus === statusFilter;
      const searchable = [
        parcel.trackingNumber,
        parcel.senderName,
        parcel.recipientName,
        parcel.recipientCity,
        parcel.currentStatus,
      ]
        .filter(Boolean)
        .join(" ")
        .toLowerCase();

      return matchesStatus && (!query || searchable.includes(query));
    });
  }, [assignedParcels, search, statusFilter]);

  const pageCount = Math.ceil(filteredParcels.length / PAGE_SIZE);
  const pageRows = filteredParcels.slice(
    (currentPage - 1) * PAGE_SIZE,
    currentPage * PAGE_SIZE,
  );

  function updateStatus(event) {
    const nextSearchParams = new URLSearchParams(searchParams);
    if (event.target.value) nextSearchParams.set("status", event.target.value);
    else nextSearchParams.delete("status");
    setSearchParams(nextSearchParams, { replace: true });
    setCurrentPage(1);
  }

  return (
    <div className="role-dashboard role-dashboard--courier">
      <PageHeader
        actions={
          <RefreshButton
            isLoading={dashboard.isLoading}
            onRefresh={dashboard.refresh}
          />
        }
        description="Only active assignments returned for your authenticated courier account are shown."
        eyebrow="My work"
        title="My Assigned Parcels"
      />
      {dashboard.isLoading && !dashboard.data && (
        <LoadingSpinner label="Loading your assigned parcels" />
      )}
      {dashboard.error && (
        <ErrorMessage error={dashboard.error} onRetry={dashboard.refresh} />
      )}
      {dashboard.data && !dashboard.error && (
        <>
          <div className="courier-parcel-toolbar">
            <SearchBar
              label="Search assigned parcels"
              onChange={(value) => {
                setSearch(value);
                setCurrentPage(1);
              }}
              placeholder="Tracking number, sender, recipient, city, or status"
              value={search}
            />
            <FormSelect
              id="courier-assigned-parcel-status"
              label="Filter by status"
              onChange={updateStatus}
              options={statusOptions}
              placeholder="All statuses"
              value={statusFilter}
            />
          </div>
          <section className="courier-parcel-list-card">
            <DataTable
              caption="Parcels assigned to the signed-in courier"
              columns={[
                { key: "trackingNumber", header: "Tracking number" },
                { key: "senderName", header: "Sender" },
                {
                  key: "recipientName",
                  header: "Recipient",
                  render: (row) => row.recipientName || "—",
                },
                {
                  key: "recipientCity",
                  header: "Destination",
                  render: (row) => row.recipientCity || "—",
                },
                {
                  key: "totalWeightKg",
                  header: "Weight",
                  render: (row) => formatWeight(row.totalWeightKg),
                },
                {
                  key: "currentStatus",
                  header: "Status",
                  render: (row) => <StatusBadge status={row.currentStatus} />,
                },
                {
                  key: "assignedAt",
                  header: "Assigned",
                  render: (row) => formatDate(row.assignedAt),
                },
              ]}
              emptyState={
                <EmptyState
                  description={
                    search || statusFilter
                      ? "Adjust the search or status filter and try again."
                      : "There are no active parcel assignments linked to your courier account."
                  }
                  title="No assigned parcels found"
                />
              }
              getRowKey={(row) => row.parcelAssignmentId}
              rowActions={(row) => (
                <Link
                  aria-label={`View details for ${row.trackingNumber}`}
                  className="ui-button ui-button--outline"
                  to={`/courier/assigned-parcels/${row.parcelId}`}
                >
                  Details & actions
                </Link>
              )}
              rows={pageRows}
            />
            <Pagination
              currentPage={currentPage}
              onPageChange={setCurrentPage}
              pageCount={pageCount}
              pageSize={PAGE_SIZE}
              totalRecords={filteredParcels.length}
            />
          </section>
        </>
      )}
    </div>
  );
}

function CourierDeliveryHistoryPage() {
  const [search, setSearch] = useState("");
  const [statusFilter, setStatusFilter] = useState("");
  const [currentPage, setCurrentPage] = useState(1);
  const loadDashboard = useCallback(
    (signal) => getCourierDashboard(signal),
    [],
  );
  const dashboard = useDashboardData(loadDashboard);
  const deliveries = useMemo(
    () =>
      Array.isArray(dashboard.data?.deliveryHistory)
        ? dashboard.data.deliveryHistory
        : [],
    [dashboard.data],
  );
  const statusOptions = useMemo(
    () =>
      [
        ...new Set(
          deliveries.map((parcel) => parcel.currentStatus).filter(Boolean),
        ),
      ]
        .sort((left, right) => left.localeCompare(right))
        .map((status) => ({ value: status, label: status })),
    [deliveries],
  );
  const filteredDeliveries = useMemo(() => {
    const query = search.trim().toLowerCase();
    return deliveries.filter((parcel) => {
      const matchesStatus =
        !statusFilter || parcel.currentStatus === statusFilter;
      const searchable = [
        parcel.trackingNumber,
        parcel.recipientName,
        parcel.recipientCity,
        parcel.currentStatus,
      ]
        .filter(Boolean)
        .join(" ")
        .toLowerCase();
      return matchesStatus && (!query || searchable.includes(query));
    });
  }, [deliveries, search, statusFilter]);
  const pageCount = Math.ceil(filteredDeliveries.length / PAGE_SIZE);
  const pageRows = filteredDeliveries.slice(
    (currentPage - 1) * PAGE_SIZE,
    currentPage * PAGE_SIZE,
  );

  return (
    <div className="role-dashboard role-dashboard--courier">
      <PageHeader
        actions={
          <RefreshButton
            isLoading={dashboard.isLoading}
            onRefresh={dashboard.refresh}
          />
        }
        description="Completed deliveries recorded for your authenticated Courier profile."
        eyebrow="My work"
        title="Delivery History"
      />
      {dashboard.isLoading && !dashboard.data && (
        <LoadingSpinner label="Loading your delivery history" />
      )}
      {dashboard.error && (
        <ErrorMessage error={dashboard.error} onRetry={dashboard.refresh} />
      )}
      {dashboard.data && !dashboard.error && (
        <>
          <div className="courier-parcel-toolbar">
            <SearchBar
              label="Search delivery history"
              onChange={(value) => {
                setSearch(value);
                setCurrentPage(1);
              }}
              placeholder="Tracking number, recipient, city, or status"
              value={search}
            />
            <FormSelect
              id="courier-delivery-status"
              label="Filter by status"
              onChange={(event) => {
                setStatusFilter(event.target.value);
                setCurrentPage(1);
              }}
              options={statusOptions}
              placeholder="All statuses"
              value={statusFilter}
            />
          </div>
          <section className="courier-parcel-list-card">
            <DataTable
              caption="Completed deliveries for the signed-in Courier"
              columns={[
                { key: "trackingNumber", header: "Tracking number" },
                {
                  key: "recipientName",
                  header: "Recipient",
                  render: (row) => row.recipientName || "—",
                },
                {
                  key: "recipientCity",
                  header: "Destination",
                  render: (row) => row.recipientCity || "—",
                },
                {
                  key: "totalWeightKg",
                  header: "Weight",
                  render: (row) => formatWeight(row.totalWeightKg),
                },
                {
                  key: "currentStatus",
                  header: "Current status",
                  render: (row) => <StatusBadge status={row.currentStatus} />,
                },
                {
                  key: "deliveredAt",
                  header: "Delivered",
                  render: (row) => formatDate(row.deliveredAt),
                },
              ]}
              emptyState={
                <EmptyState
                  description={
                    search || statusFilter
                      ? "Adjust the search or status filter and try again."
                      : "No successful deliveries have been recorded for your Courier profile."
                  }
                  title="No completed deliveries found"
                />
              }
              getRowKey={(row) => row.parcelAssignmentId}
              rowActions={(row) => (
                <Link
                  aria-label={`View details for ${row.trackingNumber}`}
                  className="ui-button ui-button--outline"
                  to={`/courier/assigned-parcels/${row.parcelId}`}
                >
                  View details
                </Link>
              )}
              rows={pageRows}
            />
            <Pagination
              currentPage={currentPage}
              onPageChange={setCurrentPage}
              pageCount={pageCount}
              pageSize={PAGE_SIZE}
              totalRecords={filteredDeliveries.length}
            />
          </section>
          <p className="courier-api-note">
            This list shows successful deliveries. Unsuccessful attempts are
            recorded in parcel tracking history.
          </p>
        </>
      )}
    </div>
  );
}

function CourierParcelDetailsPage() {
  const { parcelId } = useParams();
  const loadParcel = useCallback(
    async (signal) => {
      const dashboard = await getCourierDashboard(signal);
      return getOwnAssignedParcel(dashboard, parcelId);
    },
    [parcelId],
  );
  const parcelState = useDashboardData(loadParcel);
  const parcel = parcelState.data;

  return (
    <div className="role-dashboard role-dashboard--courier">
      <PageHeader
        actions={
          <>
            <Link
              className="ui-button ui-button--outline"
              to="/courier/assigned-parcels"
            >
              Back to my parcels
            </Link>
            {parcel && (
              <Link
                className="ui-button ui-button--primary"
                to={`/courier/assigned-parcels/${parcel.parcelId}/tracking`}
              >
                Tracking history
              </Link>
            )}
          </>
        }
        description="Details are limited to parcels assigned to your authenticated Courier profile."
        eyebrow="My work"
        title={parcel ? `Parcel ${parcel.trackingNumber}` : "Parcel Details"}
      />
      {parcelState.isLoading && !parcel && (
        <LoadingSpinner label="Loading parcel details" />
      )}
      {parcelState.error && (
        <ErrorMessage error={parcelState.error} onRetry={parcelState.refresh} />
      )}
      {parcel && (
        <section className="courier-parcel-details">
          <div className="courier-parcel-details__status">
            <span>Current status</span>
            <StatusBadge status={parcel.currentStatus} />
          </div>
          <dl className="courier-parcel-details__grid">
            <div>
              <dt>Tracking number</dt>
              <dd>{parcel.trackingNumber}</dd>
            </div>
            <div>
              <dt>Assignment ID</dt>
              <dd>{parcel.parcelAssignmentId}</dd>
            </div>
            <div>
              <dt>Weight</dt>
              <dd>{formatWeight(parcel.totalWeightKg)}</dd>
            </div>
            <div>
              <dt>Booking date</dt>
              <dd>{formatDate(parcel.bookingDate)}</dd>
            </div>
            <div>
              <dt>Expected delivery</dt>
              <dd>{formatDate(parcel.expectedDeliveryDate)}</dd>
            </div>
            <div>
              <dt>Sender</dt>
              <dd>{parcel.senderName || "—"}</dd>
            </div>
            <div>
              <dt>Sender phone</dt>
              <dd>{parcel.senderPhone || "—"}</dd>
            </div>
            <div>
              <dt>Sender address</dt>
              <dd>
                {formatAddress(
                  parcel.senderAddressLine,
                  parcel.senderCity,
                  parcel.senderProvince,
                  parcel.senderPostalCode,
                )}
              </dd>
            </div>
            <div>
              <dt>Recipient</dt>
              <dd>{parcel.recipientName || "—"}</dd>
            </div>
            <div>
              <dt>Recipient phone</dt>
              <dd>{parcel.recipientPhone || "—"}</dd>
            </div>
            <div>
              <dt>Recipient email</dt>
              <dd>{parcel.recipientEmail || "—"}</dd>
            </div>
            <div>
              <dt>Delivery address</dt>
              <dd>
                {formatAddress(
                  parcel.recipientAddressLine,
                  parcel.recipientCity,
                  parcel.recipientProvince,
                  parcel.recipientPostalCode,
                )}
              </dd>
            </div>
            <div>
              <dt>Route</dt>
              <dd>{parcel.routeName || "—"}</dd>
            </div>
            <div>
              <dt>Origin hub</dt>
              <dd>{parcel.originHubName || "—"}</dd>
            </div>
            <div>
              <dt>Destination hub</dt>
              <dd>{parcel.destinationHubName || "—"}</dd>
            </div>
            <div>
              <dt>Assigned</dt>
              <dd>{formatDate(parcel.assignedAt)}</dd>
            </div>
            <div>
              <dt>Delivered</dt>
              <dd>{formatDate(parcel.deliveredAt)}</dd>
            </div>
          </dl>
          {parcel.specialInstructions && (
            <div className="courier-parcel-details__instructions">
              <h2>Special instructions</h2>
              <p>{parcel.specialInstructions}</p>
            </div>
          )}
          {parcel.assignmentNotes && (
            <div className="courier-parcel-details__instructions">
              <h2>Assignment notes</h2>
              <p>{parcel.assignmentNotes}</p>
            </div>
          )}
          <CourierParcelWorkflow
            key={parcel.parcelAssignmentId}
            onUpdated={parcelState.refresh}
            parcel={parcel}
          />
        </section>
      )}
    </div>
  );
}

function CourierParcelTrackingPage() {
  const { parcelId } = useParams();
  const loadTracking = useCallback(
    async (signal) => {
      const dashboard = await getCourierDashboard(signal);
      const parcel = getOwnAssignedParcel(dashboard, parcelId);
      const events = await getTrackingHistory(parcel.trackingNumber, signal);

      if (!Array.isArray(events)) {
        throw new Error("The tracking history response was not a list.");
      }

      return {
        parcel,
        events: events
          .slice()
          .sort(
            (left, right) => new Date(left.eventAt) - new Date(right.eventAt),
          ),
      };
    },
    [parcelId],
  );
  const tracking = useDashboardData(loadTracking);

  return (
    <div className="role-dashboard role-dashboard--courier">
      <PageHeader
        actions={
          <>
            <Link
              className="ui-button ui-button--outline"
              to={`/courier/assigned-parcels/${parcelId}`}
            >
              Parcel details
            </Link>
            <RefreshButton
              isLoading={tracking.isLoading}
              onRefresh={tracking.refresh}
            />
          </>
        }
        description="Read-only tracking events for a parcel currently assigned to your courier account."
        eyebrow="My work"
        title={
          tracking.data
            ? `Tracking · ${tracking.data.parcel.trackingNumber}`
            : "Tracking History"
        }
      />
      {tracking.isLoading && !tracking.data && (
        <LoadingSpinner label="Loading parcel tracking history" />
      )}
      {tracking.error && (
        <ErrorMessage error={tracking.error} onRetry={tracking.refresh} />
      )}
      {tracking.data && tracking.data.events.length === 0 && (
        <EmptyState
          description="No tracking events were returned for this assigned parcel."
          title="No tracking history"
        />
      )}
      {tracking.data?.events.length > 0 && (
        <section
          aria-label="Parcel tracking events"
          className="courier-tracking-timeline"
        >
          {tracking.data.events.map((event) => (
            <article
              className="courier-tracking-event"
              key={event.statusEventId}
            >
              <span
                aria-hidden="true"
                className="courier-tracking-event__marker"
              />
              <div className="courier-tracking-event__content">
                <div className="courier-tracking-event__heading">
                  <StatusBadge status={event.statusName} />
                  <time dateTime={event.eventAt}>
                    {formatDate(event.eventAt)}
                  </time>
                </div>
                <h2>
                  {event.statusName} <span>({event.statusCode})</span>
                </h2>
                {event.hubName && <p>Hub: {event.hubName}</p>}
                {event.courierName && <p>Courier: {event.courierName}</p>}
                {event.notes && <p>{event.notes}</p>}
              </div>
            </article>
          ))}
        </section>
      )}
    </div>
  );
}

export {
  CourierAssignedParcelsPage,
  CourierDeliveryHistoryPage,
  CourierParcelDetailsPage,
  CourierParcelTrackingPage,
};
