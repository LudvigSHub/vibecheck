import { useEffect, useState } from "react";
import { Link, NavLink, useNavigate } from "react-router-dom";

import { useAuth } from "../context/AuthContext";

import "../styles/Navbar.css";

/*
  Navbar – oinloggat state.
  Länkarna ligger i en array så att vi bara behöver ändra på ett ställe
  när vi lägger till fler sidor.
*/
const NAV_LINKS = [
  { label: "Ordbok", to: "/ordbok" },
  { label: "Quiz", to: "/quiz" },
  { label: "Topplistor", to: "/topplistor" },
  { label: "Om oss", to: "/om-oss" },
];

function UserIcon() {
  return (
    <svg
      className="navbar__login-icon"
      viewBox="0 0 24 24"
      width="18"
      height="18"
      fill="none"
      stroke="currentColor"
      strokeWidth="2"
      strokeLinecap="round"
      strokeLinejoin="round"
      aria-hidden="true"
      focusable="false"
    >
      <circle cx="12" cy="8" r="4" />
      <path d="M4 21c0-4.2 3.6-7 8-7s8 2.8 8 7" />
    </svg>
  );
}

// onLoginClick öppnar inloggningsrutan, den ligger i App
function Navbar({ onLoginClick }) {
  const [menuOpen, setMenuOpen] = useState(false);
  const { isAuthenticated, authLoading, user, logout } = useAuth();
  const navigate = useNavigate();
  const homePath = isAuthenticated ? "/home" : "/";

  function closeMenu() {
    setMenuOpen(false);
  }

  //När man loggar ut så blir man navigerad till startsida
  function handleLogout() {
    closeMenu();
    logout();
    navigate("/", { replace: true });
  }

  function handleLogin() {
    closeMenu();
    onLoginClick();
  }

  // Escape stänger mobilmenyn.
  useEffect(() => {
    if (!menuOpen) return;

    function handleKeyDown(event) {
      if (event.key === "Escape") {
        setMenuOpen(false);
      }
    }

    window.addEventListener("keydown", handleKeyDown);

    return () => {
      window.removeEventListener("keydown", handleKeyDown);
    };
  }, [menuOpen]);

  return (
    <header className="navbar">
      <nav className="navbar__inner" aria-label="Huvudmeny">
        <Link
          to={homePath}
          className="navbar__brand"
          onClick={closeMenu}
          aria-label={
            isAuthenticated
              ? "VibeCheck – till din startsida"
              : "VibeCheck – till startsidan"
          }
        >
          <img
            src="/images/vibecheck-logo.png"
            alt="VibeCheck"
            className="navbar__logo"
            width="655"
            height="140"
          />
        </Link>

        <button
          type="button"
          className="navbar__toggle"
          aria-label={menuOpen ? "Stäng meny" : "Öppna meny"}
          aria-expanded={menuOpen}
          aria-controls="navbar-menu"
          onClick={() => setMenuOpen((open) => !open)}
        >
          <span className="navbar__burger" aria-hidden="true" />
        </button>

        <div
          id="navbar-menu"
          className={
            menuOpen ? "navbar__menu navbar__menu--open" : "navbar__menu"
          }
        >
          <ul className="navbar__links">
            <li>
              <NavLink
                to={homePath}
                end
                onClick={closeMenu}
                className={({ isActive }) =>
                  isActive
                    ? "navbar__link navbar__link--active"
                    : "navbar__link"
                }
              >
                Hem
              </NavLink>
            </li>

            {NAV_LINKS.map((link) => (
              <li key={link.to}>
                <NavLink
                  to={link.to}
                  onClick={closeMenu}
                  className={({ isActive }) =>
                    isActive
                      ? "navbar__link navbar__link--active"
                      : "navbar__link"
                  }
                >
                  {link.label}
                </NavLink>
              </li>
            ))}
          </ul>

          {/* Knappar och inte länkar, eftersom inloggningen är en popup.
              authLoading gör att knappen inte hinner blinka förbi vid omladdning. */}
          {authLoading ? null : isAuthenticated ? (
            <div className="navbar__user">
              <span className="navbar__avatar" aria-hidden="true">
                {user.userName.charAt(0).toUpperCase()}
              </span>
              <button type="button" className="navbar__login" onClick={handleLogout}>
                <UserIcon />
                <span>Logga ut</span>
              </button>
            </div>
          ) : (
            <button type="button" className="navbar__login" onClick={handleLogin}>
              <UserIcon />
              <span>Logga in</span>
            </button>
          )}
        </div>
      </nav>
    </header>
  );
}

export default Navbar;
