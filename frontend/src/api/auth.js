import { apiFetch } from "./client";

export function loginUser(userName, password) {
  return apiFetch("/api/auth/login", {
    method: "POST",
    auth: false,
    body: JSON.stringify({
      userName,
      password,
    }),
  });
}

export function registerUser(userName, email, password) {
  return apiFetch("/api/auth/register", {
    method: "POST",
    auth: false,
    body: JSON.stringify({
      userName,
      email,
      password,
    }),
  });
}

export function getCurrentUser() {
  return apiFetch("/api/auth/me", {
    method: "GET",
  });
}