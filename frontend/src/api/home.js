import { apiFetch } from "./client";

export function getHomeSummary(options = {}) {
  return apiFetch("/api/home", {
    method: "GET",
    ...options,
  });
}
