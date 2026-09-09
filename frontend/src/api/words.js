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

export function getQuizDemoQuestions(count = 10, options = {}) {
  return apiFetch(`/api/words/quiz-demo?count=${count}`, {
    method: "GET",
    auth: false,
    ...options,
  });
}

// Wordstash

export function getWords({ search = "", tags = [] } = {}) {
  const params = new URLSearchParams();

  if (search) params.set("search", search);

  // append och inte set: set skriver över, append lägger till.
  // Resultatet blir ?tags=beröm&tags=kritik
  for (const tag of tags) {
    params.append("tags", tag);
  }

  const query = params.toString();

  return apiFetch(`/api/words${query ? `?${query}` : ""}`, {
    method: "GET",
    optionalAuth: true,
  });
}

export function getWordById(id) {
  return apiFetch(`/api/words/${id}`, {
    method: "GET",
    optionalAuth: true,
  });
}

export function getTags() {
  return apiFetch("/api/words/tags", {
    method: "GET",
    auth: false,
  });
}

export function voteWord(wordId, isPositive) {
  return apiFetch(`/api/words/${wordId}/vote`, {
    method: "POST",
    body: JSON.stringify({
      isPositive,
    }),
  });
}

export function removeVote(wordId) {
  return apiFetch(`/api/words/${wordId}/vote`, {
    method: "DELETE",
  });
}
