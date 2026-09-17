import { apiFetch } from "./client";

// Bara själva quizet här. Hämtningen av topplistan hör till din kollegas
// sida – lägger vi den också här blir det en merge-konflikt i onödan.

export function startRankedAttempt(options = {}) {
  return apiFetch("/api/ranked/attempts", {
    method: "POST",
    ...options,
  });
}

export function submitRankedAnswer(
  attemptId,
  questionId,
  alternativeId,
  options = {},
) {
  return apiFetch(`/api/ranked/attempts/${attemptId}/answers`, {
    method: "POST",
    body: JSON.stringify({ questionId, alternativeId }),
    ...options,
  });
}

export function completeRankedAttempt(attemptId, options = {}) {
  return apiFetch(`/api/ranked/attempts/${attemptId}/complete`, {
    method: "POST",
    ...options,
  });
}
