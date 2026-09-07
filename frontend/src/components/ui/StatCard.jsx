export default function StatCard({ label, value, className = "", ...props }) {
  return (
    <div className={`stat-card ${className}`.trim()} {...props}>
      <dt className="stat-card__label">{label}</dt>
      <dd className="stat-card__value">{value}</dd>
    </div>
  );
}