export default function Avatar({ initials, size = "md" }) {
  const sizeClass = size === "lg" ? " avatar--lg" : "";
  return (
    <span className={`avatar${sizeClass}`} aria-hidden="true">
      {initials}
    </span>
  );
}
