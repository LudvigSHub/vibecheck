import { expect } from "@playwright/test";
import { createBdd } from "playwright-bdd";
import { ordkort, detaljer } from "./gemensamt.js";

const { Given, When, Then } = createBdd();

// SearchInput sätter aria-label till samma text som placeholder,
// så fältet går att hitta på det man ser i det.
const sökfält = (page) => page.getByLabel("Sök efter ett slangord...");

Given("att jag är på WordStash", async ({ page }) => {
  await page.goto("/wordstash");
});

When("jag söker efter {string}", async ({ page }, term) => {
  await sökfält(page).fill(term);
});

When("jag väljer ordet {string} i listan", async ({ page }, ord) => {
  await ordkort(page, ord).click();
});

Then("ska jag se sökfältet för slangord", async ({ page }) => {
  await expect(sökfält(page)).toBeVisible();
});

Then("jag ska se ordet {string} i listan", async ({ page }, ord) => {
  await expect(ordkort(page, ord)).toBeVisible();
});

Then("ska listan bara innehålla ordet {string}", async ({ page }, ord) => {
  // Sökningen går till servern, så listan uppdateras asynkront.
  // toHaveCount väntar in det åt oss.
  await expect(page.locator(".word-card")).toHaveCount(1);
  await expect(ordkort(page, ord)).toBeVisible();
});

Then("ska jag se detaljerna för {string}", async ({ page }, ord) => {
  await expect(
    detaljer(page).getByRole("heading", { name: ord, exact: true }),
  ).toBeVisible();
});

Then("detaljerna ska innehålla minst ett exempel", async ({ page }) => {
  await expect(
    detaljer(page).getByRole("heading", { name: "Exempel" }),
  ).toBeVisible();

  const antal = await detaljer(page)
    .locator(".word-details__examples p")
    .count();

  expect(antal).toBeGreaterThan(0);
});
