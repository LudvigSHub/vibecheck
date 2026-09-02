import { Link } from "react-router-dom";

export default function LinkButton({ variant = "primary", className = "", ...props }) {
  return <Link className={`btn btn--${variant} ${className}`.trim()} {...props} />;
}