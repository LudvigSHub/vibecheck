import { SearchIcon } from "../Icons";

export default function SearchInput({ value, onChange, placeholder }) {
  return (
    <div className="search-input card">
      <SearchIcon className="search-input__icon" />
      <input
        type="search"
        className="search-input__field"
        value={value}
        onChange={onChange}
        placeholder={placeholder}
        aria-label={placeholder}
      />
    </div>
  );
}