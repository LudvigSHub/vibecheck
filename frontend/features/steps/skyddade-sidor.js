import { expect } from "@playwright/test";
import { createBdd } from "playwright-bdd";
import { inloggningsformulär } from "./gemensamt.js";

const { Then } = createBdd();

Then(
  "ska inloggningsrutan visas med texten {string}",
  async ({ page }, text) => {
    await expect(inloggningsformulär(page)).toBeVisible();
    await expect(page.getByText(text)).toBeVisible();
  },
);

Then("jag ska inte se quizsidan", async ({ page }) => {
  await expect(
    page.getByRole("heading", { name: "Testa dina kunskaper" }),
  ).toBeHidden();
});

Then("jag ska vara kvar på startsidan", async ({ page }) => {
  // Strängen löses mot baseURL i playwright.config.js.
  await expect(page).toHaveURL("/");
});
