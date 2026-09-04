import { useState } from "react";
import {
  Navigate,
  Routes,
  Route,
  useLocation,
  useNavigate,
} from "react-router-dom";
import "./App.css";

import LandingPage from "./pages/LandingPage";
import HomePage from "./pages/HomePage";
import TestPage from "./pages/TestPage";
import QuizesPage from "./pages/QuizesPage";
import AdminPage from "./pages/admin/AdminPage";
import AdminWordDetailsPage from "./pages/admin/AdminWordDetailsPage";
import AdminCreateWordPage from "./pages/admin/AdminCreateWordPage";

import Navbar from "./components/Navbar";
import ProtectedRoute from "./components/ProtectedRoute";
import AdminRoute from "./components/AdminRoute";
import LoginForm from "./components/auth/LoginForm";
import RegisterForm from "./components/auth/RegisterForm";
import { useAuth } from "./context/AuthContext";

function App() {
  // null = ingen autentiseringsruta är öppen.
  const [authView, setAuthView] = useState(null);

  const location = useLocation();
  const navigate = useNavigate();
  const { isAuthenticated, authLoading } = useAuth();

  // ProtectedRoute skickar med information när en oinloggad användare
  // försöker nå en skyddad sida. App läser den direkt i stället för att
  // kopiera den till flera state-variabler i en useEffect.
  const loginRequired = location.state?.requireAuth === true;
  const requestedRoute = loginRequired
    ? (location.state.from ?? null)
    : null;
  const routeAuthView = loginRequired
    ? (location.state.authView ?? "login")
    : null;
  const displayedAuthView = routeAuthView ?? authView;
  const authMessage = loginRequired ? "Logga in för att nå den sidan." : "";

  function handleAuthSuccess() {
    setAuthView(null);

    // Om användaren först försökte nå en skyddad sida
    // skickas hen tillbaka dit efter inloggningen.
    if (requestedRoute) {
      navigate(requestedRoute, { replace: true });
      return;
    }

    // Vanlig inloggning från landningssidan leder till Home-page.
    navigate("/home");
  }

  function handleAuthClose() {
    setAuthView(null);

    if (loginRequired) {
      navigate(location.pathname + location.search, {
        replace: true,
        state: null,
      });
    }
  }

  function handleAuthSwitch(nextView) {
    if (loginRequired) {
      navigate(location.pathname + location.search, {
        replace: true,
        state: {
          ...location.state,
          authView: nextView,
        },
      });
      return;
    }

    setAuthView(nextView);
  }

  return (
    <>
      <Navbar onLoginClick={() => setAuthView("login")} />

      <Routes>
        <Route
          path="/"
          element={
            authLoading ? null : isAuthenticated ? (
              <Navigate to={requestedRoute ?? "/home"} replace />
            ) : (
              <LandingPage onOpenRegister={() => handleAuthSwitch("register")} />
            )
          }
        />
        <Route path="/test" element={<TestPage />} />

        <Route
          path="/home"
          element={
            <ProtectedRoute>
              <HomePage />
            </ProtectedRoute>
          }
        />

        <Route
          path="/quiz"
          element={
            <ProtectedRoute>
              <QuizesPage />
            </ProtectedRoute>
          }
        />
        
        <Route
          path="/admin"
          element={
            <AdminRoute>
              <AdminPage />
            </AdminRoute>
          }
        />
        
        <Route
          path="/admin/words/:id"
          element={
            <AdminRoute>
              <AdminWordDetailsPage />
            </AdminRoute>
        }
        />

        <Route
          path="/admin/words/new"
          element={
            <AdminRoute>
              <AdminCreateWordPage />
            </AdminRoute>
        }
        />
      </Routes>

      {/* Formulären ligger utanför Routes så att de kan öppnas över alla sidor. */}
      {displayedAuthView === "login" && (
        <LoginForm
          message={authMessage}
          onSuccess={handleAuthSuccess}
          onClose={handleAuthClose}
          onSwitch={() => handleAuthSwitch("register")}
        />
      )}

      {displayedAuthView === "register" && (
        <RegisterForm
          onSuccess={handleAuthSuccess}
          onClose={handleAuthClose}
          onSwitch={() => handleAuthSwitch("login")}
        />
      )}
    </>
  );
}

export default App;
