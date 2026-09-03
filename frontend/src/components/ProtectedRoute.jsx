import { useEffect, useRef } from "react";
import { Navigate, useLocation } from "react-router-dom";

import { useAuth } from "../context/AuthContext";

function ProtectedRoute({ children }) {
  const { isAuthenticated, authLoading } = useAuth();
  const location = useLocation();

  // Sant så snart sidan visats för en inloggad användare. Skiljer
  // "blockerad på väg in" från "var inne och loggade ut".
  const hadAccess = useRef(false);

  useEffect(() => {
    if (isAuthenticated) {
      hadAccess.current = true;
    }
  }, [isAuthenticated]);

  if (authLoading) {
    return null;
  }

  if (isAuthenticated) {
    return children;
  }

  // Användaren var inne och har loggat ut. Till startsidan, utan
  // uppmaning att logga in igen.
  if (hadAccess.current) {
    return <Navigate to="/" replace />;
  }

  return (
    <Navigate
      to="/"
      replace
      state={{
        requireAuth: true,
        from: location.pathname + location.search,
      }}
    />
  );
}

export default ProtectedRoute;
