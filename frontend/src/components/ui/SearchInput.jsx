import { SearchIcon } from "../Icons";
import Card from "./Card";

export default function SearchInput({ value, onChange, placeholder }) {
  return (
    <Card className="search-input">
      <SearchIcon className="search-input__icon" />
      <input
        type="search"
        className="search-input__field"
        value={value}
        onChange={onChange}
        placeholder={placeholder}
        aria-label={placeholder}
      />
    </Card>
  );
}