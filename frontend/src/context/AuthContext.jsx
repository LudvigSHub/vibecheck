import {
  createContext,
  useContext,
  useEffect,
  useState,
} from "react";

import {
  loginUser,
  registerUser,
  getCurrentUser,
} from "../api/auth";

import {
  getToken,
  saveToken,
  removeToken,
} from "../api/client";

const AuthContext = createContext(null);

export function AuthProvider({ children }) {
  const [user, setUser] = useState(null);
  const [authLoading, setAuthLoading] = useState(true);

  function setSession(data) {
    saveToken(data.token);

    setUser({
      userName: data.userName,
      roles: data.roles,
    });
  }

  function logout() {
    removeToken();
    setUser(null);
  }

  async function login(userName, password) {
    const data = await loginUser(userName, password);

    setSession(data);
  }

  async function register(userName, email, password) {
    const data = await registerUser(
      userName,
      email,
      password
    );

    setSession(data);
  }

  useEffect(() => {
    function handleUnauthorized() {
      logout();
    }

    window.addEventListener(
      "auth:unauthorized",
      handleUnauthorized
    );

    async function restoreSession() {
      const token = getToken();

      if (!token) {
        setAuthLoading(false);
        return;
      }

      try {
        const currentUser = await getCurrentUser();

        setUser(currentUser);
      } catch (error) {
        if (error.status === 401) {
          logout();
        }
      } finally {
        setAuthLoading(false);
      }
    }

    restoreSession();

    return () => {
      window.removeEventListener(
        "auth:unauthorized",
        handleUnauthorized
      );
    };
  }, []);

  const isAuthenticated = user !== null;

  const isAdmin =
    user?.roles?.some(
      (role) => role.toLowerCase() === "admin"
    ) ?? false;

  const value = {
    user,
    authLoading,
    isAuthenticated,
    isAdmin,
    login,
    register,
    logout,
  };

  return (
    <AuthContext.Provider value={value}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const context = useContext(AuthContext);

  if (!context) {
    throw new Error(
      "useAuth must be used inside AuthProvider"
    );
  }

  return context;
}