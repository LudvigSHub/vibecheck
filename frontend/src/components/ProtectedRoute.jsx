import { Navigate, useLocation } from "react-router-dom";

import { useAuth } from "../context/AuthContext";

function ProtectedRoute({ children }) {
  const { isAuthenticated, authLoading } = useAuth();
  const location = useLocation();

  if (authLoading) {
    return null;
  }

  if (isAuthenticated) {
    return children;
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
