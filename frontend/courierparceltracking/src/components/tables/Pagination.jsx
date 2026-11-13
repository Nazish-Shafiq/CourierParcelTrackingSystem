import { ChevronLeft, ChevronRight } from 'lucide-react'
import '../../style/components/components.css'

function Pagination({
  currentPage = 1,
  onPageChange,
  pageCount,
  pageSize,
  siblingCount = 1,
  totalRecords,
}) {
  const resolvedPageCount = Number.isInteger(pageCount)
    ? pageCount
    : Number.isFinite(totalRecords) && Number.isFinite(pageSize) && pageSize > 0
      ? Math.ceil(totalRecords / pageSize)
      : 0
  const requestedPage = Number.isInteger(currentPage) ? currentPage : 1
  const page = Math.min(Math.max(requestedPage, 1), resolvedPageCount || 1)
  const hasSummary = Number.isFinite(totalRecords)
  const resolvedPageSize = Number.isFinite(pageSize) && pageSize > 0
    ? pageSize
    : hasSummary && resolvedPageCount > 0
      ? Math.ceil(totalRecords / resolvedPageCount)
      : 0

  if (resolvedPageCount <= 0) {
    return hasSummary
      ? <p className="ui-pagination__summary">Showing 0 of {totalRecords} records</p>
      : null
  }

  const firstPage = Math.max(1, page - siblingCount)
  const lastPage = Math.min(resolvedPageCount, page + siblingCount)
  const pages = Array.from({ length: lastPage - firstPage + 1 }, (_, index) => firstPage + index)
  const rangeStart = hasSummary && totalRecords > 0
    ? (page - 1) * resolvedPageSize + 1
    : 0
  const rangeEnd = hasSummary
    ? Math.min(page * resolvedPageSize, totalRecords)
    : 0

  return (
    <div className="ui-pagination-wrapper">
      {hasSummary && (
        <p className="ui-pagination__summary">
          Showing {rangeStart}–{rangeEnd} of {totalRecords} records
        </p>
      )}
      {resolvedPageCount > 1 && (
        <nav aria-label="Pagination" className="ui-pagination">
          <button
            aria-label="Go to previous page"
            className="ui-pagination__button"
            disabled={page <= 1}
            onClick={() => onPageChange?.(page - 1)}
            type="button"
          >
            <ChevronLeft aria-hidden="true" size={17} />
          </button>
          {firstPage > 1 && <span className="ui-pagination__ellipsis">…</span>}
          {pages.map((pageNumber) => (
            <button
              aria-current={page === pageNumber ? 'page' : undefined}
              aria-label={`Go to page ${pageNumber}`}
              className={`ui-pagination__button${page === pageNumber ? ' ui-pagination__button--active' : ''}`}
              key={pageNumber}
              onClick={() => onPageChange?.(pageNumber)}
              type="button"
            >
              {pageNumber}
            </button>
          ))}
          {lastPage < resolvedPageCount && <span className="ui-pagination__ellipsis">…</span>}
          <button
            aria-label="Go to next page"
            className="ui-pagination__button"
            disabled={page >= resolvedPageCount}
            onClick={() => onPageChange?.(page + 1)}
            type="button"
          >
            <ChevronRight aria-hidden="true" size={17} />
          </button>
        </nav>
      )}
    </div>
  )
}

export default Pagination
