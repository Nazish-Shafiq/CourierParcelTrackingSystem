import { Search, X } from 'lucide-react'
import { useId } from 'react'
import '../../style/components/components.css'

function SearchBar({
  id,
  label = 'Search',
  onChange,
  onClear,
  placeholder = 'Search…',
  value = '',
}) {
  const generatedId = useId()
  const inputId = id || generatedId

  function clearSearch() {
    onChange?.('')
    onClear?.()
  }

  return (
    <div className="ui-search" role="search">
      <label className="visually-hidden" htmlFor={inputId}>{label}</label>
      <Search aria-hidden="true" className="ui-search__icon" size={18} />
      <input
        className="ui-search__input"
        id={inputId}
        onChange={(event) => onChange?.(event.target.value)}
        placeholder={placeholder}
        type="search"
        value={value}
      />
      {value && (
        <button
          aria-label="Clear search"
          className="ui-search__clear"
          onClick={clearSearch}
          type="button"
        >
          <X aria-hidden="true" size={17} />
        </button>
      )}
    </div>
  )
}

export default SearchBar
