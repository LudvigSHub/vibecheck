import { apiFetch } from "./client";

export function getAdminWords(options = {}) {
  return apiFetch("/api/admin/words", {
    method: "GET",
    ...options,
  });
}

export function getAdminTags(options = {}) {
  return apiFetch("/api/admin/tags", {
    method: "GET",
    ...options,
  });
}

export function getAdminWord(id, options = {}) {
  return apiFetch(`/api/admin/words/${id}`, {
    method: "GET",
    ...options,
  });
}

export function updateAdminWord(id, data, options = {}) {
  return apiFetch(`/api/admin/words/${id}`, {
    method: "PUT",
    body: JSON.stringify(data),
    ...options,
  });
}

export function deleteAdminWord(id, options = {}) {
  return apiFetch(`/api/admin/words/${id}`, {
    method: "DELETE",
    ...options,
  });
}

export function createAdminWord(data, options = {}) {
  return apiFetch("/api/admin/words", {
    method: "POST",
    body: JSON.stringify(data),
    ...options,
  });
}