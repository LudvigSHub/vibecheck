import { useState } from "react";
import Modal from "../ui/Modal";
import { updateUserName, changePassword } from "../../api/profile";
import "../auth/AuthForm.css";

function EditAccountForm({ onClose, onSuccess }) {
  const [newUserName, setNewUserName] = useState("");
  const [currentPassword, setCurrentPassword] = useState("");
  const [newPassword, setNewPassword] = useState("");
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);

  async function handleSubmit(event) {
    event.preventDefault();

    const hasUserName = newUserName.trim() !== "";
    const hasCurrentPassword = currentPassword !== "";
    const hasNewPassword = newPassword !== "";

    if (!hasUserName && !hasCurrentPassword && !hasNewPassword) {
      setError("Fyll i minst ett fält.");
      return;
    }

    if (hasCurrentPassword !== hasNewPassword) {
      setError("Fyll i både nuvarande och nytt lösenord.");
      return;
    }

    setError("");
    setLoading(true);

    const errors = [];

    if (hasUserName) {
      try {
        await updateUserName(newUserName.trim());
      } catch (err) {
        errors.push(err.message);
      }
    }

    if (hasCurrentPassword && hasNewPassword) {
      try {
        await changePassword(currentPassword, newPassword);
      } catch (err) {
        errors.push(err.message);
      }
    }

    setLoading(false);

    if (errors.length > 0) {
      setError(errors.join(" "));
      return;
    }

    onSuccess?.();
  }

  return (
    <Modal onClose={onClose}>
      <form onSubmit={handleSubmit}>
        <img
          className="auth-logo"
          src="/images/vibecheck-logo.png"
          alt="VibeCheck"
          width="655"
          height="140"
        />

        <h2 className="auth-title">Redigera Konto</h2>

        {error && <p className="auth-error-box">{error}</p>}

        <div className="auth-field">
          <label className="auth-label" htmlFor="edit-account-username">
            Nytt användarnamn
          </label>
          <input
            className="auth-input"
            id="edit-account-username"
            type="text"
            placeholder="Nytt användarnamn"
            value={newUserName}
            onChange={(e) => setNewUserName(e.target.value)}
          />
        </div>

        <div className="auth-field">
          <label className="auth-label" htmlFor="edit-account-current-password">
            Lösenord
          </label>
          <input
            className="auth-input"
            id="edit-account-current-password"
            type="password"
            placeholder="Ditt lösenord"
            value={currentPassword}
            onChange={(e) => setCurrentPassword(e.target.value)}
          />
        </div>

        <div className="auth-field">
          <label className="auth-label" htmlFor="edit-account-new-password">
            Nytt lösenord
          </label>
          <input
            className="auth-input"
            id="edit-account-new-password"
            type="password"
            placeholder="Nytt lösenord"
            value={newPassword}
            onChange={(e) => setNewPassword(e.target.value)}
          />
          <p className="auth-hint">
            Minst 8 tecken, en stor bokstav och en siffra.
          </p>
        </div>

        <button className="auth-submit" type="submit" disabled={loading}>
          {loading ? "Sparar..." : "Redigera Konto"}
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
      </form>
    </Modal>
  );
}

export default EditAccountForm;
