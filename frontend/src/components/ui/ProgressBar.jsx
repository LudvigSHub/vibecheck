export default function ProgressBar({ value, className = "", ...props }) {
  const clamped = Math.min(100, Math.max(0, value));
  return (
    <div
      className={`progress-bar ${className}`.trim()}
      role="progressbar"
      aria-valuenow={clamped}
      aria-valuemin={0}
      aria-valuemax={100}
      {...props}
    >
      <div className="progress-bar__fill" style={{ width: `${clamped}%` }} />
    </div>
  );
}
