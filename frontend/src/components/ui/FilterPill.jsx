export default function FilterPill({ active = false, onClick, children }) {
  return (
    <button
      type="button"
      className={`filter-pill${active ? " filter-pill--active" : ""}`}
      onClick={onClick}
      aria-pressed={active}
    >
      {children}
    </button>
  );
}
