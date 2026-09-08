import { apiFetch } from "./client";

export function getProfile(options = {}) {
  return apiFetch("/api/profile/me", {
    method: "GET",
    ...options,
  });
}

export function updateUserName(newUserName, options = {}) {
  return apiFetch("/api/profile/me/username", {
    method: "PUT",
    body: JSON.stringify({ newUserName }),
    ...options,
  });
}

export function changePassword(currentPassword, newPassword, options = {}) {
  return apiFetch("/api/profile/me/password", {
    method: "PUT",
    body: JSON.stringify({ currentPassword, newPassword }),
    ...options,
  });
}
