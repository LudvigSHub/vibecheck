import { SearchIcon } from "../Icons";
import Card from "./Card";

export default function SearchInput({
  value,
  onChange,
  onKeyDown,
  placeholder,
}) {
  return (
    <Card className="search-input">
      <SearchIcon className="search-input__icon" />
      <input
        type="search"
        className="search-input__field"
        value={value}
        onChange={onChange}
        onKeyDown={onKeyDown}
        placeholder={placeholder}
        aria-label={placeholder}
      />
    </Card>
  );
}