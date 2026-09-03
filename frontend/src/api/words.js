import { apiFetch } from "./client";

// auth: false – endpointen är publik, och en utgången token skulle
// annars trigga utloggning bara för att landningssidan laddades.

export function getWordOfTheDay(options = {}) {
  return apiFetch("/api/words/word-of-the-day", {
    method: "GET",
    auth: false,
    ...options,
  });
}

// Wordstash

export function getWords({ search = "", tag = "" } = {}) {
  const params = new URLSearchParams();

  if (search) {
    params.set("search", search);
  }

  if (tag) {
    params.set("tag", tag);
  }

  const query = params.toString();

  return apiFetch(`/api/words${query ? `?${query}` : ""}`, {
    method: "GET",
    auth: false,
  });
}

export function getWordById(id) {
  return apiFetch(`/api/words/${id}`, {
    method: "GET",
    auth: false,
  });
}

export function getTags() {
  return apiFetch("/api/words/tags", {
    method: "GET",
    auth: false,
  });
}
