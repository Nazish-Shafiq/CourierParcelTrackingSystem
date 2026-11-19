import { useEffect, useId, useMemo, useRef, useState } from "react";
import { useSearchParams } from "react-router-dom";
import { Eye, Pencil, Plus, Trash2 } from "lucide-react";
import Button from "../common/Button";
import EmptyState from "../common/EmptyState";
import ErrorMessage from "../common/ErrorMessage";
import FormInput from "../forms/FormInput";
import FormSelect from "../forms/FormSelect";
import FormTextArea from "../forms/FormTextArea";
import ConfirmationModal from "../modals/ConfirmationModal";
import DataTable from "../tables/DataTable";
import Pagination from "../tables/Pagination";
import PageHeader from "../common/PageHeader";
import RefreshButton from "../common/RefreshButton";
import SearchBar from "../common/SearchBar";
import "../../style/admin/admin-management.css";

const PAGE_SIZE_OPTIONS = [10, 25, 50, 100].map((pageSize) => ({
  label: String(pageSize),
  value: pageSize,
}));

function getEmptyValues(fields, isEditing = false) {
  return Object.fromEntries(
    fields
      .filter((field) => isEditing
        ? !field.createOnly
        : !field.editOnly)
      .map((field) => [
        field.name,
        field.defaultValue ?? (field.type === "checkbox" ? false : ""),
      ]),
  );
}

function getRecordValues(record, fields) {
  return Object.fromEntries(
    fields
      .filter((field) =>
        (!field.editOnly || record) && (!field.createOnly || !record),
      )
      .map((field) => {
        const value = record[field.name];
        return [
          field.name,
          field.type === "date" && value
            ? String(value).slice(0, 10)
            : (value ?? (field.type === "checkbox" ? false : "")),
        ];
      }),
  );
}

function buildRequest(values, fields, isEditing) {
  return Object.fromEntries(
    fields
      .filter((field) => isEditing
        ? !field.createOnly
        : !field.editOnly)
      .map((field) => {
        const value = values[field.name];
        if (field.type === "checkbox") return [field.name, Boolean(value)];
        if (field.type === "number" || field.numeric) {
          return [
            field.name,
            value === "" && field.nullable
              ? null
              : field.integer || field.numeric
                ? Number.parseInt(value, 10)
                : Number(value),
          ];
        }
        if (field.nullable && typeof value === "string") {
          return [field.name, value.trim() || null];
        }
        return [field.name, value];
      }),
  );
}

function isFieldVisible(field, values, options) {
  return !field.visibleWhen || field.visibleWhen(values, options);
}

function isFieldRequired(field, values, options) {
  return field.required
    || field.requiredWhen?.(values, options)
    || false;
}

function getSearchValue(record, searchFields) {
  return searchFields
    .map((field) => record[field])
    .map((value) => (Array.isArray(value) ? value.join(" ") : value))
    .filter((value) => value != null)
    .join(" ")
    .toLowerCase();
}

function formatDetailValue(value) {
  if (value == null || value === "") return "—";
  if (typeof value === "boolean") return value ? "Yes" : "No";
  if (Array.isArray(value)) return value.join(", ") || "—";
  return String(value);
}

function AdminEntityManager({ config }) {
  const [searchParams, setSearchParams] = useSearchParams();
  const formId = useId();
  const formController = useRef(null);
  const detailsController = useRef(null);
  const [records, setRecords] = useState([]);
  const [pageData, setPageData] = useState(null);
  const [isLoading, setIsLoading] = useState(true);
  const [loadError, setLoadError] = useState(null);
  const [refreshVersion, setRefreshVersion] = useState(0);
  const [search, setSearch] = useState("");
  const [debouncedSearch, setDebouncedSearch] = useState("");
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);

  const [isFormOpen, setIsFormOpen] = useState(false);
  const [isFormDataLoading, setIsFormDataLoading] = useState(false);
  const [formDataError, setFormDataError] = useState(null);
  const [formOptions, setFormOptions] = useState({});
  const [formValues, setFormValues] = useState({});
  const [editingRecord, setEditingRecord] = useState(null);
  const [isSaving, setIsSaving] = useState(false);
  const [formError, setFormError] = useState(null);

  const [detailsRecord, setDetailsRecord] = useState(null);
  const [isDetailsLoading, setIsDetailsLoading] = useState(false);
  const [detailsError, setDetailsError] = useState(null);
  const [deleteRecord, setDeleteRecord] = useState(null);
  const [deleteError, setDeleteError] = useState(null);
  const [isDeleting, setIsDeleting] = useState(false);
  const [notice, setNotice] = useState("");

  useEffect(
    () => () => {
      formController.current?.abort();
      detailsController.current?.abort();
    },
    [],
  );

  useEffect(() => {
    const timeout = window.setTimeout(() => {
      setDebouncedSearch(search.trim());
      setCurrentPage(1);
    }, 250);
    return () => window.clearTimeout(timeout);
  }, [search]);

  useEffect(() => {
    if (!isFormOpen && !detailsRecord) return undefined;

    function handleDialogKeyDown(event) {
      if (event.key !== "Escape") return;
      if (isFormOpen && !isSaving) {
        formController.current?.abort();
        setIsFormOpen(false);
        setFormError(null);
        setFormDataError(null);
      } else if (detailsRecord) {
        detailsController.current?.abort();
        setDetailsRecord(null);
        setIsDetailsLoading(false);
      }
    }

    window.addEventListener("keydown", handleDialogKeyDown);
    return () => window.removeEventListener("keydown", handleDialogKeyDown);
  }, [detailsRecord, isFormOpen, isSaving]);

  const requestPage = config.clientSide ? 1 : currentPage;
  const requestPageSize = config.clientSide ? 100 : pageSize;
  const requestSearch = config.clientSide ? undefined : debouncedSearch;
  const isWaitingForSearch =
    !config.clientSide && search.trim() !== debouncedSearch;

  useEffect(() => {
    if (isWaitingForSearch) return undefined;

    const controller = new AbortController();
    async function loadRecords() {
      setIsLoading(true);
      setLoadError(null);
      try {
        const result = await config.list({
          page: requestPage,
          pageSize: requestPageSize,
          search: requestSearch,
          signal: controller.signal,
        });

        if (config.clientSide) {
          if (!Array.isArray(result))
            throw new Error("The API response was not a record list.");
          setRecords(result);
          setPageData(null);
        } else {
          if (!result || !Array.isArray(result.items)) {
            throw new Error(
              "The API response did not contain a paged record list.",
            );
          }
          setRecords(result.items);
          setPageData(result);
        }
      } catch (error) {
        if (!controller.signal.aborted) {
          setLoadError(error);
          setRecords([]);
          setPageData(null);
        }
      } finally {
        if (!controller.signal.aborted) setIsLoading(false);
      }
    }

    loadRecords();
    return () => controller.abort();
  }, [
    config,
    isWaitingForSearch,
    refreshVersion,
    requestPage,
    requestPageSize,
    requestSearch,
    searchParams,
  ]);

  const filteredRecords = useMemo(() => {
    let result = records;
    if (config.filterRecord) {
      result = result.filter((record) =>
        config.filterRecord(record, searchParams),
      );
    }
    if (!config.clientSide || !search.trim()) return result;
    const query = search.trim().toLowerCase();
    return result.filter((record) =>
      getSearchValue(record, config.searchFields).includes(query),
    );
  }, [config, records, search, searchParams]);

  const totalRecords = config.clientSide
    ? filteredRecords.length
    : (pageData?.totalRecords ?? 0);
  const totalPages = config.clientSide
    ? Math.ceil(totalRecords / pageSize)
    : (pageData?.totalPages ?? 0);
  const tableRows = config.clientSide
    ? filteredRecords.slice(
        (currentPage - 1) * pageSize,
        currentPage * pageSize,
      )
    : records;

  async function openForm(record = null) {
    if (formController.current) formController.current.abort();
    const controller = new AbortController();
    formController.current = controller;
    setEditingRecord(record);
    setFormValues(getEmptyValues(config.fields, Boolean(record)));
    setFormOptions({});
    setFormError(null);
    setFormDataError(null);
    setIsFormOpen(true);
    setIsFormDataLoading(true);

    try {
      const [fullRecord, options] = await Promise.all([
        record && config.getById
          ? config.getById(record[config.idField], controller.signal)
          : Promise.resolve(record),
        config.loadFormOptions
          ? config.loadFormOptions(controller.signal)
          : Promise.resolve({}),
      ]);
      if (controller.signal.aborted) return;
      if (record && !fullRecord) {
        throw new Error(`The selected ${config.singular} could not be found.`);
      }
      setEditingRecord(fullRecord);
      setFormValues(
        fullRecord
          ? getRecordValues(fullRecord, config.fields)
          : getEmptyValues(config.fields),
      );
      setFormOptions(options);
    } catch (error) {
      if (!controller.signal.aborted) setFormDataError(error);
    } finally {
      if (!controller.signal.aborted) setIsFormDataLoading(false);
    }
  }

  function closeForm() {
    formController.current?.abort();
    setIsFormOpen(false);
    setFormValues({});
    setEditingRecord(null);
    setFormError(null);
    setFormDataError(null);
  }

  async function openDetails(record) {
    if (detailsController.current) detailsController.current.abort();
    const controller = new AbortController();
    detailsController.current = controller;
    setDetailsRecord(record);
    setDetailsError(null);
    setIsDetailsLoading(Boolean(config.getById));
    if (!config.getById) return;

    try {
      const fullRecord = await config.getById(
        record[config.idField],
        controller.signal,
      );
      if (!fullRecord)
        throw new Error(`The selected ${config.singular} could not be found.`);
      if (!controller.signal.aborted) setDetailsRecord(fullRecord);
    } catch (error) {
      if (!controller.signal.aborted) setDetailsError(error);
    } finally {
      if (!controller.signal.aborted) setIsDetailsLoading(false);
    }
  }

  function closeDetails() {
    detailsController.current?.abort();
    setDetailsRecord(null);
    setDetailsError(null);
    setIsDetailsLoading(false);
  }

  function handleFieldChange(name, value) {
    setFormValues((current) => ({ ...current, [name]: value }));
  }

  async function handleSubmit(event) {
    event.preventDefault();
    const validationMessage = config.validate?.(formValues, editingRecord);
    if (validationMessage) {
      setFormError(new Error(validationMessage));
      return;
    }

    setIsSaving(true);
    setFormError(null);
    try {
      const wasEditing = Boolean(editingRecord);
      const visibleFields = config.fields.filter((field) =>
        isFieldVisible(field, formValues, formOptions),
      );
      const request = buildRequest(formValues, visibleFields, wasEditing);
      if (wasEditing) {
        await config.update(editingRecord[config.idField], request);
      } else {
        await config.create(request);
      }
      closeForm();
      setNotice(
        `${config.singular} ${wasEditing ? "updated" : "created"} successfully.`,
      );
      if (!wasEditing) setCurrentPage(1);
      setRefreshVersion((version) => version + 1);
    } catch (error) {
      setFormError(error);
    } finally {
      setIsSaving(false);
    }
  }

  async function handleDelete() {
    if (!deleteRecord || !config.remove) return;
    setIsDeleting(true);
    setDeleteError(null);
    try {
      const deleted = await config.remove(deleteRecord[config.idField]);
      if (deleted === false)
        throw new Error(`The ${config.singular} could not be deactivated.`);
      setNotice(`${config.singular} deactivated successfully.`);
      setDeleteRecord(null);
      setCurrentPage(1);
      setRefreshVersion((version) => version + 1);
    } catch (error) {
      setDeleteError(error);
    } finally {
      setIsDeleting(false);
    }
  }

  function renderField(field) {
    if (field.editOnly && !editingRecord) return null;
    if (field.createOnly && editingRecord) return null;
    const id = `${formId}-${field.name}`;
    const value =
      formValues[field.name] ?? (field.type === "checkbox" ? false : "");
    const required = isFieldRequired(field, formValues, formOptions);

    if (field.type === "checkbox") {
      return (
        <label className="admin-checkbox" htmlFor={id} key={field.name}>
          <input
            checked={Boolean(value)}
            id={id}
            onChange={(event) =>
              handleFieldChange(field.name, event.target.checked)
            }
            type="checkbox"
          />
          <span>{field.label}</span>
        </label>
      );
    }

    if (field.type === "select") {
      const optionsKey = field.optionsKeyByMode?.[
        editingRecord ? "edit" : "create"
      ] ?? field.optionsKey;
      const options = field.optionsKey
        ? (formOptions[optionsKey] ?? [])
        : (field.options ?? []);
      return (
        <FormSelect
          disabled={
            isFormDataLoading || (optionsKey && Boolean(formDataError))
          }
          id={id}
          key={field.name}
          label={field.label}
          onChange={(event) =>
            handleFieldChange(field.name, event.target.value)
          }
          options={options}
          placeholder={field.placeholder}
          required={required}
          value={value}
        />
      );
    }

    const fieldProps = {
      id,
      label: field.label,
      max: field.max,
      maxLength: field.maxLength,
      min: field.min,
      minLength: field.minLength,
      onChange: (event) => handleFieldChange(field.name, event.target.value),
      placeholder: field.placeholder,
      required,
      step: field.step,
      type: field.type ?? "text",
      value,
    };
    if (field.type === "textarea") {
      return (
        <FormTextArea
          id={id}
          label={field.label}
          maxLength={field.maxLength}
          onChange={(event) =>
            handleFieldChange(field.name, event.target.value)
          }
          placeholder={field.placeholder}
          required={required}
          rows={field.rows}
          value={value}
        />
      );
    }
    return <FormInput key={field.name} {...fieldProps} />;
  }

  function renderRowActions(record) {
    return (
      <div className="admin-row-actions">
        <Button
          aria-label={`View ${config.getDisplayName(record)}`}
          className="admin-row-action"
          onClick={() => openDetails(record)}
          title="View details"
          variant="outline"
        >
          <Eye aria-hidden="true" size={16} />
        </Button>
        {config.update && (
          <Button
            aria-label={`Edit ${config.getDisplayName(record)}`}
            className="admin-row-action"
            onClick={() => openForm(record)}
            title="Edit"
            variant="outline"
          >
            <Pencil aria-hidden="true" size={16} />
          </Button>
        )}
        {config.remove && (
          <Button
            aria-label={`Deactivate ${config.getDisplayName(record)}`}
            className="admin-row-action admin-row-action--danger"
            onClick={() => {
              setDeleteError(null);
              setDeleteRecord(record);
            }}
            title="Deactivate"
            variant="outline"
          >
            <Trash2 aria-hidden="true" size={16} />
          </Button>
        )}
      </div>
    );
  }

  const emptyState = (
    <EmptyState
      description={
        search
          ? "Try a different search term."
          : `There are no ${config.title.toLowerCase()} to display.`
      }
      title={`No ${config.title.toLowerCase()} found`}
      action={
        config.create && (
          <Button onClick={() => openForm()} variant="primary">
            <Plus aria-hidden="true" size={16} />
            Add {config.singular}
          </Button>
        )
      }
    />
  );

  return (
    <div className="role-dashboard admin-management">
      <PageHeader
        actions={
          <div className="admin-page-actions">
            {config.create && (
              <Button onClick={() => openForm()} variant="primary">
                <Plus aria-hidden="true" size={17} />
                Add {config.singular}
              </Button>
            )}
            <RefreshButton
              isLoading={isLoading}
              onRefresh={() => {
                setNotice("");
                setRefreshVersion((version) => version + 1);
              }}
            />
          </div>
        }
        description={config.description}
        eyebrow={config.eyebrow ?? "Administration"}
        title={config.title}
      />

      {notice && (
        <p className="admin-notice" role="status">
          {notice}
        </p>
      )}
      {loadError && (
        <ErrorMessage
          error={loadError}
          onRetry={() => setRefreshVersion((version) => version + 1)}
        />
      )}

      <div className="admin-list-toolbar">
        <SearchBar
          label={`Search ${config.title.toLowerCase()}`}
          onChange={setSearch}
          placeholder={`Search ${config.title.toLowerCase()}…`}
          value={search}
        />
        {config.listFilters?.map((filter) => (
          <FormSelect
            id={`admin-filter-${filter.name}`}
            key={filter.name}
            label={filter.label}
            onChange={(event) => {
              const nextParams = new URLSearchParams(searchParams);
              if (event.target.value) {
                nextParams.set(filter.name, event.target.value);
              } else {
                nextParams.delete(filter.name);
              }
              setSearchParams(nextParams);
              setCurrentPage(1);
            }}
            options={filter.options}
            placeholder={filter.placeholder}
            value={searchParams.get(filter.name) ?? ""}
          />
        ))}
        <FormSelect
          id="admin-page-size"
          label="Rows per page"
          onChange={(event) => {
            setPageSize(Number(event.target.value));
            setCurrentPage(1);
          }}
          options={PAGE_SIZE_OPTIONS}
          value={pageSize}
        />
      </div>

      {!loadError && (
        <>
          <DataTable
            caption={`${config.title} records`}
            columns={config.columns}
            emptyState={emptyState}
            getRowKey={(record) => record[config.idField]}
            isLoading={isLoading}
            rowActions={renderRowActions}
            rows={tableRows}
          />
          <Pagination
            currentPage={
              config.clientSide ? currentPage : (pageData?.page ?? currentPage)
            }
            onPageChange={setCurrentPage}
            pageCount={totalPages}
            pageSize={
              config.clientSide ? pageSize : (pageData?.pageSize ?? pageSize)
            }
            totalRecords={totalRecords}
          />
        </>
      )}

      {isFormOpen && (
        <div className="admin-dialog-backdrop">
          <section
            aria-labelledby={`${formId}-title`}
            aria-modal="true"
            className="admin-dialog"
            role="dialog"
          >
            <header className="admin-dialog__header">
              <div>
                <p className="admin-dialog__eyebrow">
                  {editingRecord ? "Edit record" : "New record"}
                </p>
                <h2 className="admin-dialog__title" id={`${formId}-title`}>
                  {editingRecord
                    ? `Edit ${config.singular}`
                    : `Add ${config.singular}`}
                </h2>
              </div>
              <Button disabled={isSaving} onClick={closeForm} variant="outline">
                Close
              </Button>
            </header>
            {isFormDataLoading ? (
              <div className="admin-dialog__loading" role="status">
                Loading form data…
              </div>
            ) : (
              <form className="admin-form" onSubmit={handleSubmit}>
                {formDataError && <ErrorMessage error={formDataError} />}
                {formError && <ErrorMessage error={formError} />}
                <div className="admin-form__grid">
                  {config.fields
                    .filter((field) =>
                      isFieldVisible(field, formValues, formOptions),
                    )
                    .map(renderField)}
                </div>
                <div className="admin-dialog__actions">
                  <Button
                    disabled={isSaving}
                    onClick={closeForm}
                    variant="outline"
                  >
                    Cancel
                  </Button>
                  <Button
                    disabled={Boolean(formDataError)}
                    isLoading={isSaving}
                    type="submit"
                  >
                    {editingRecord
                      ? "Save changes"
                      : `Create ${config.singular}`}
                  </Button>
                </div>
              </form>
            )}
          </section>
        </div>
      )}

      {detailsRecord && (
        <div className="admin-dialog-backdrop">
          <section
            aria-labelledby={`${formId}-details-title`}
            aria-modal="true"
            className="admin-dialog admin-dialog--details"
            role="dialog"
          >
            <header className="admin-dialog__header">
              <div>
                <p className="admin-dialog__eyebrow">{config.title}</p>
                <h2
                  className="admin-dialog__title"
                  id={`${formId}-details-title`}
                >
                  {config.getDisplayName(detailsRecord)}
                </h2>
              </div>
              <Button onClick={closeDetails} variant="outline">
                Close
              </Button>
            </header>
            {isDetailsLoading && (
              <div className="admin-dialog__loading" role="status">
                Loading details…
              </div>
            )}
            {detailsError && <ErrorMessage error={detailsError} />}
            {!isDetailsLoading && !detailsError && (
              <dl className="admin-details">
                {config.detailFields.map((field) => {
                  const value =
                    typeof field.value === "function"
                      ? field.value(detailsRecord)
                      : detailsRecord[field.key];
                  return (
                    <div className="admin-details__item" key={field.label}>
                      <dt>{field.label}</dt>
                      <dd>{formatDetailValue(value)}</dd>
                    </div>
                  );
                })}
              </dl>
            )}
          </section>
        </div>
      )}

      <ConfirmationModal
        confirmLabel="Deactivate"
        isBusy={isDeleting}
        isOpen={Boolean(deleteRecord)}
        message={[
          `Deactivate ${deleteRecord ? config.getDisplayName(deleteRecord) : ""}?`,
          "The backend will retain the record using its soft-delete behavior.",
          deleteError?.message,
        ]
          .filter(Boolean)
          .join(" ")}
        onCancel={() => {
          setDeleteRecord(null);
          setDeleteError(null);
        }}
        onConfirm={handleDelete}
        title={`Deactivate ${config.singular}`}
      />
    </div>
  );
}

export default AdminEntityManager;
