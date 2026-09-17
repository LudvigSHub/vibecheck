import { apiFetch } from "./client";

export function getLeaderboard(week = "current", options = {}) {
  return apiFetch(`/api/leaderboard?week=${week}`, {
    method: "GET",
    ...options,
  });
}
