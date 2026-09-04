import { useEffect, useState } from "react";
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
import AdminPage from "./pages/AdminPage";

import Navbar from "./components/Navbar";
import ProtectedRoute from "./components/ProtectedRoute";
import AdminRoute from "./components/AdminRoute";
import LoginForm from "./components/auth/LoginForm";
import RegisterForm from "./components/auth/RegisterForm";
import { useAuth } from "./context/AuthContext";

function App() {
  // null = ingen autentiseringsruta är öppen.
  const [authView, setAuthView] = useState(null);
  const [authMessage, setAuthMessage] = useState("");

  // Sidan användaren försökte nå innan ProtectedRoute stoppade navigeringen.
  const [redirectTo, setRedirectTo] = useState(null);

  const location = useLocation();
  const navigate = useNavigate();
  const { isAuthenticated, authLoading } = useAuth();

  // ProtectedRoute skickar information via Navigate-state
  // när en oinloggad användare försöker nå en skyddad sida.
  useEffect(() => {
    if (!location.state?.requireAuth) {
      return;
    }

    setAuthView("login");
    setAuthMessage("Logga in för att nå den sidan.");
    setRedirectTo(location.state.from ?? null);

    // Rensa state så att inloggningsrutan inte öppnas igen
    // om användaren laddar om landningssidan.
    navigate(location.pathname, {
      replace: true,
      state: null,
    });
  }, [location, navigate]);

  function handleAuthSuccess() {
    setAuthView(null);
    setAuthMessage("");

    // Om användaren först försökte nå en skyddad sida
    // skickas hen tillbaka dit efter inloggningen.
    if (redirectTo) {
      navigate(redirectTo, { replace: true });
      setRedirectTo(null);
      return;
    }

    // Vanlig inloggning från landningssidan leder till Home-page.
    navigate("/home");
  }

  function handleAuthClose() {
    setAuthView(null);
    setAuthMessage("");
    setRedirectTo(null);
  }

  return (
    <>
      <Navbar onLoginClick={() => setAuthView("login")} />

      <Routes>
        <Route
          path="/"
          element={
            authLoading ? null : isAuthenticated ? (
              <Navigate to="/home" replace />
            ) : (
              <LandingPage onOpenRegister={() => setAuthView("register")} />
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
      </Routes>

      {/* Formulären ligger utanför Routes så att de kan öppnas över alla sidor. */}
      {authView === "login" && (
        <LoginForm
          message={authMessage}
          onSuccess={handleAuthSuccess}
          onClose={handleAuthClose}
          onSwitch={() => setAuthView("register")}
        />
      )}

      {authView === "register" && (
        <RegisterForm
          onSuccess={handleAuthSuccess}
          onClose={handleAuthClose}
          onSwitch={() => setAuthView("login")}
        />
      )}
    </>
  );
}

export default App;
