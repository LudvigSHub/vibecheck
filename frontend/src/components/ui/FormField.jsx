import Input from "./Input";

export default function FormField({ id, label, hint, error, ...inputProps }) {
  return (
    <div className="form-field">
      <label className="form-field__label" htmlFor={id}>
        {label}
      </label>
      <Input id={id} error={Boolean(error)} {...inputProps} />
      {hint && !error && <p className="form-field__hint">{hint}</p>}
      {error && <p className="form-field__error">{error}</p>}
    </div>
  );
}
