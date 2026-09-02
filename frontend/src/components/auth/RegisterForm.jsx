import { useState } from 'react';
import { useAuth } from '../../context/AuthContext';
import './AuthForm.css';

// Samma upplägg som LoginForm, bara fler fält
function RegisterForm({ onSuccess, onSwitch, onClose }) {
  const { register } = useAuth();

  const [userName, setUserName] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [confirm, setConfirm] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  const passwordsMatch = confirm === '' || password === confirm;

  async function handleSubmit(event) {
    event.preventDefault();

    // Bara frontend kollar det här, backend vet inget om bekräftelsefältet
    if (password !== confirm) {
      setError('Lösenorden är inte lika.');
      return;
    }

    setError('');
    setLoading(true);

    try {
      await register(userName, email, password);
      onSuccess?.();
    } catch (err) {
      setError(err.message);
    } finally {
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
        <p className="auth-logo">VibeCheck</p>

        <h2 className="auth-title">Skapa konto</h2>
        <p className="auth-subtitle">Häng med i snacket!</p>

        {error && <p className="auth-error-box">{error}</p>}

        <div className="auth-field">
          <label className="auth-label" htmlFor="register-username">Användarnamn</label>
          <input
            className="auth-input"
            id="register-username"
            type="text"
            placeholder="Användarnamn"
            value={userName}
            onChange={(e) => setUserName(e.target.value)}
            required
          />
        </div>

        <div className="auth-field">
          <label className="auth-label" htmlFor="register-email">E-post</label>
          <input
            className="auth-input"
            id="register-email"
            type="email"
            placeholder="Namn@exempel.se"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            required
          />
        </div>

        <div className="auth-field">
          <label className="auth-label" htmlFor="register-password">Lösenord</label>
          <input
            className="auth-input"
            id="register-password"
            type="password"
            placeholder="Ditt lösenord"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            required
          />
          <p className="auth-hint">Minst 8 tecken, en stor bokstav och en siffra.</p>
        </div>

        <div className="auth-field">
          <label className="auth-label" htmlFor="register-confirm">Bekräfta lösenord</label>
          <input
            className={`auth-input${passwordsMatch ? '' : ' auth-input--error'}`}
            id="register-confirm"
            type="password"
            placeholder="Bekräfta lösenord"
            value={confirm}
            onChange={(e) => setConfirm(e.target.value)}
            required
          />
          {!passwordsMatch && (
            <p className="auth-error">Lösenorden är inte lika.</p>
          )}
        </div>

        <button
          className="auth-submit"
          type="submit"
          disabled={loading}
          style={{ marginTop: 6 }}
        >
          {loading ? 'Skapar konto...' : 'Skapa konto'}
          <svg className="auth-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
            <circle cx="12" cy="8" r="4" />
            <path d="M4 21c0-4 3.6-7 8-7s8 3 8 7" />
          </svg>
        </button>

        <p className="auth-footer">
          Har redan ett konto?{' '}
          <a
            className="auth-link"
            href="#"
            onClick={(e) => {
              e.preventDefault();
              onSwitch?.();
            }}
          >
            Logga in
          </a>
        </p>
      </form>
    </div>
  );
}

export default RegisterForm;
