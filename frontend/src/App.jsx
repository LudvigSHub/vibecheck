import { useEffect, useState } from "react";
import { Routes, Route, useLocation, useNavigate } from "react-router-dom";
import "./App.css";

import LandingPage from "./pages/LandingPage";
import TestPage from "./pages/TestPage";
import Navbar from "./components/Navbar";
import ProtectedRoute from "./components/ProtectedRoute";
import LoginForm from "./components/auth/LoginForm";
import RegisterForm from "./components/auth/RegisterForm";
import QuizesPage from "./pages/QuizesPage";

function App() {
  // null = ingen ruta öppen. Ligger här så knappen i navbaren når den.
  const [authView, setAuthView] = useState(null);
  const [authMessage, setAuthMessage] = useState("");

  // Vart användaren var på väg när hen blockerades.
  const [redirectTo, setRedirectTo] = useState(null);

  const location = useLocation();
  const navigate = useNavigate();

  // ProtectedRoute skickar hit via Navigate-state när någon blockerats.
  useEffect(() => {
    if (!location.state?.requireAuth) {
      return;
    }

    setAuthView("login");
    setAuthMessage("Logga in för att nå den sidan.");
    setRedirectTo(location.state.from ?? null);

    // Rensa historikposten. Utan det ligger requireAuth kvar, och rutan
    // poppar upp igen varje gång användaren laddar om startsidan.
    navigate(location.pathname, { replace: true, state: null });
  }, [location, navigate]);

  function handleAuthSuccess() {
    setAuthView(null);
    setAuthMessage("");

    if (redirectTo) {
      navigate(redirectTo, { replace: true });
      setRedirectTo(null);
    }
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
        <Route path="/" element={<LandingPage onOpenRegister={() => setAuthView("register")} />}/>
        <Route path="/test" element={<TestPage />} />

        {/* Skyddade sidor: wrappa elementet, inte routen.
            <Route> tar bara emot <Route> som barn. */}
        
        <Route
          path="/quiz"
          element={
            <ProtectedRoute>
              <QuizesPage />
            </ProtectedRoute>
          }
        />
       
      </Routes>

      {/* Ligger utanför Routes så de kan öppnas från vilken sida som helst */}
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
