export default function FilterPill({
  active = false,
  disabled = false,
  onClick,
  children,
}) {
  return (
    <button
      type="button"
      className={`filter-pill${active ? " filter-pill--active" : ""}`}
      onClick={onClick}
      disabled={disabled}
      aria-pressed={active}
    >
      {children}
    </button>
  );
}
