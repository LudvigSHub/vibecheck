export default function Avatar({ initials }) {
  return (
    <span className="avatar" aria-hidden="true">
      {initials}
    </span>
  );
}