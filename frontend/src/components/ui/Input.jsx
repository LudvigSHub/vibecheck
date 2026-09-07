export default function Input({ className = "", error = false, ...props }) {
  return (
    <input
      className={`input${error ? " input--error" : ""} ${className}`.trim()}
      {...props}
    />
  );
}
