import EmptyState from '../common/EmptyState'
import '../../style/components/components.css'

function DataTable({
  caption,
  columns = [],
  emptyState,
  getRowKey,
  isLoading = false,
  rowActions,
  rows = [],
}) {
  if (isLoading) {
    return <div aria-live="polite" className="data-table__loading" role="status">Loading data…</div>
  }

  if (rows.length === 0) {
    return emptyState || <EmptyState />
  }

  return (
    <div className="data-table__scroll">
      <table className="data-table">
        {caption && <caption className="visually-hidden">{caption}</caption>}
        <thead>
          <tr>
            {columns.map((column) => (
              <th className={column.className} key={column.key} scope="col">
                {column.header}
              </th>
            ))}
            {rowActions && <th scope="col">Actions</th>}
          </tr>
        </thead>
        <tbody>
          {rows.map((row, rowIndex) => (
            <tr key={getRowKey ? getRowKey(row) : row.id ?? rowIndex}>
              {columns.map((column) => (
                <td className={column.className} key={column.key}>
                  {column.render ? column.render(row) : row[column.key]}
                </td>
              ))}
              {rowActions && <td className="data-table__actions">{rowActions(row)}</td>}
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}

export default DataTable
