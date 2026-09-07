import { apiFetch } from "./client";

export function getProfile(options = {}) {
  return apiFetch("/api/profile/me", {
    method: "GET",
    ...options,
  });
}
