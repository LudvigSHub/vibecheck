import { Navigate } from "react-router-dom";

import ProtectedRoute from "./ProtectedRoute";
import { useAuth } from "../context/AuthContext";

function AdminRoute({ children }) {
  const { isAdmin } = useAuth();

  return (
    <ProtectedRoute>
      {isAdmin ? children : <Navigate to="/home" replace />}
    </ProtectedRoute>
  );
}

export default AdminRoute;