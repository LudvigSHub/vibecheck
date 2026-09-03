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

