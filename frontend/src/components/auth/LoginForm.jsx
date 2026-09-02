import { useEffect, useState } from "react";
import { useAuth } from "../../context/AuthContext";
import "./AuthForm.css";

// Propsen sätts av sidan som visar formuläret, t.ex. landningssidan
function LoginForm({ onSuccess, onSwitch, onClose }) {
  const { login } = useAuth();

  // Escape stänger rutan. Samma mönster som mobilmenyn i Navbar.
  useEffect(() => {
    function handleKeyDown(event) {
      if (event.key === "Escape") {
        onClose?.();
      }
    }

    window.addEventListener("keydown", handleKeyDown);

    return () => window.removeEventListener("keydown", handleKeyDown);
  }, [onClose]);

  const [userName, setUserName] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);

  async function handleSubmit(event) {
    event.preventDefault();
    setError("");
    setLoading(true);

    try {
      await login(userName, password);
      onSuccess?.();
    } catch (err) {
      setError(err.message);
    } finally {
      // finally, annars sitter knappen låst när något gått fel
      setLoading(false);
    }
  }

  // Stänger bara om man klickar på det mörka, inte inuti rutan
  function handleOverlayClick(event) {
    if (event.target === event.currentTarget) {
      onClose?.();
    }
  }

  return (
    <div className="auth-overlay" onClick={handleOverlayClick}>
      <form className="auth-card" onSubmit={handleSubmit}>
        {/* Platshållare tills vi har loggan som fil */}
        <button
          type="button"
          className="auth-close"
          onClick={() => onClose?.()}
          aria-label="Stäng"
        >
          <svg
            viewBox="0 0 24 24"
            width="18"
            height="18"
            fill="none"
            stroke="currentColor"
            strokeWidth="2"
            strokeLinecap="round"
            aria-hidden="true"
          >
            <path d="M6 6l12 12" />
            <path d="M18 6L6 18" />
          </svg>
        </button>

        <img
          className="auth-logo"
          src="/images/vibecheck-logo.png"
          alt="VibeCheck"
          width="655"
          height="140"
        />

        <h2 className="auth-title">Logga in</h2>
        <p className="auth-subtitle">Välkommen tillbaka!</p>

        {error && <p className="auth-error-box">{error}</p>}

        <div className="auth-field">
          {/* Backend loggar in på användarnamn. Figman visar e-post, den ska ändras. */}
          <label className="auth-label" htmlFor="login-username">
            Användarnamn
          </label>
          <input
            className="auth-input"
            id="login-username"
            type="text"
            placeholder="Ditt användarnamn"
            value={userName}
            onChange={(e) => setUserName(e.target.value)}
            required
          />
        </div>

        <div className="auth-field">
          <label className="auth-label" htmlFor="login-password">
            Lösenord
          </label>
          <input
            className="auth-input"
            id="login-password"
            type="password"
            placeholder="Ditt lösenord"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            required
          />
        </div>

        <div className="auth-row">
          <a className="auth-link" href="#">
            Glömt lösenord?
          </a>
        </div>

        <button className="auth-submit" type="submit" disabled={loading}>
          {loading ? "Loggar in..." : "Logga in"}
          <svg
            className="auth-icon"
            viewBox="0 0 24 24"
            fill="none"
            stroke="currentColor"
            strokeWidth="2"
          >
            <circle cx="12" cy="8" r="4" />
            <path d="M4 21c0-4 3.6-7 8-7s8 3 8 7" />
          </svg>
        </button>

        <p className="auth-footer">
          Har du inget konto?{" "}
          <a
            className="auth-link"
            href="#"
            onClick={(e) => {
              e.preventDefault();
              onSwitch?.();
            }}
          >
            Skapa ett konto
          </a>
        </p>
      </form>
    </div>
  );
}

export default LoginForm;
