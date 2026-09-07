import { expect } from "@playwright/test";
import { createBdd } from "playwright-bdd";

const { Given, When } = createBdd();

// Hjälpare som pekar ut delar av sidan. De ligger här för att flera
// stegfiler behöver dem – och för att en ändring i markupen då bara
// ska behöva rättas på ett ställe.
export const meny = (page) =>
  page.getByRole("navigation", { name: "Huvudmeny" });

export const inloggningsformulär = (page) =>
  page.locator("form").filter({ has: page.getByLabel("Användarnamn") });

export const ordkort = (page, ord) =>
  page.getByRole("button").filter({
    has: page.getByRole("heading", { name: ord, exact: true }),
  });

export const detaljer = (page) => page.locator(".word-details");

Given("att jag är på startsidan", async ({ page }) => {
  await page.goto("/");
});

// Kontrollerar förutsättningen i stället för att bara påstå den.
// Varje test får ett tomt webbläsarsammanhang, så ingen token finns –
// men om det någonsin skulle sluta stämma vill vi veta det här,
// inte tre steg senare när något annat failar av fel anledning.
Given("att jag inte är inloggad", async ({ page }) => {
  await page.goto("/");
  await expect(
    meny(page).getByRole("button", { name: "Logga in" }),
  ).toBeVisible();
});

When("jag går till {string}", async ({ page }, sökväg) => {
  await page.goto(sökväg);
});
