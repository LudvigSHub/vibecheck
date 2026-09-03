import { apiFetch } from "./client";

// Alla endpoints kräver inloggning, så ingen av dem sätter auth: false.
// apiFetch lägger på Authorization-headern automatiskt.

export function getQuizzes(options = {}) {
  return apiFetch("/api/quiz", {
    method: "GET",
    ...options,
  });
}

export function startQuizAttempt(quizId, options = {}) {
  return apiFetch(`/api/quiz/${quizId}/attempts`, {
    method: "POST",
    ...options,
  });
}

export function submitAnswer(
  attemptId,
  questionId,
  alternativeId,
  options = {},
) {
  return apiFetch(`/api/quiz/attempts/${attemptId}/answers`, {
    method: "POST",
    body: JSON.stringify({ questionId, alternativeId }),
    ...options,
  });
}

export function completeQuizAttempt(attemptId, options = {}) {
  return apiFetch(`/api/quiz/attempts/${attemptId}/complete`, {
    method: "POST",
    ...options,
  });
}

export function abandonQuizAttempt(attemptId, options = {}) {
  return apiFetch(`/api/quiz/attempts/${attemptId}`, {
    method: "DELETE",
    ...options,
  });
}
