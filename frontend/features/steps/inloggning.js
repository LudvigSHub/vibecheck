import { expect } from "@playwright/test";
import { createBdd } from "playwright-bdd";
import { meny, inloggningsformulär } from "./gemensamt.js";

const { When, Then } = createBdd();

When("jag öppnar inloggningsrutan", async ({ page }) => {
  await meny(page).getByRole("button", { name: "Logga in" }).click();
});

When("jag fyller i {string} som användarnamn", async ({ page }, värde) => {
  await page.getByLabel("Användarnamn").fill(värde);
});

When("jag fyller i {string} som lösenord", async ({ page }, värde) => {
  await page.getByLabel("Lösenord").fill(värde);
});

When("jag skickar inloggningsformuläret", async ({ page }) => {
  await inloggningsformulär(page)
    .getByRole("button", { name: "Logga in" })
    .click();
});

Then("ska jag vara inloggad som {string}", async ({ page }, namn) => {
  await expect(page).toHaveURL(/\/home$/);

  await expect(
    page.getByRole("heading", { name: new RegExp(`Hej, ${namn}`) }),
  ).toBeVisible();

  await expect(
    meny(page).getByRole("button", { name: "Logga ut" }),
  ).toBeVisible();
});

Then("ska jag se ett felmeddelande", async ({ page }) => {
  await expect(page.locator(".auth-error-box")).toBeVisible();
});

Then("jag ska fortfarande vara utloggad", async ({ page }) => {
  await expect(
    meny(page).getByRole("button", { name: "Logga in" }),
  ).toBeVisible();
});
