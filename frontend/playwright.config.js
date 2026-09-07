import { defineConfig, devices } from "@playwright/test";
import { defineBddConfig } from "playwright-bdd";

// Läser .feature-filerna och genererar Playwright-tester av dem.
// Det genererade hamnar i .features-gen/ och ska inte checkas in.
const testDir = defineBddConfig({
  features: "features/**/*.feature",
  steps: "features/steps/**/*.js",
});

export default defineConfig({
  testDir,
  reporter: [["list"], ["html", { open: "never" }]],

  use: {
    baseURL: "http://localhost:5173",

    // Backend kör på https://localhost:7226 med .NET:s utvecklingscertifikat.
    // Webbläsaren Playwright startar litar inte på det, så varje anrop till
    // API:et hade avvisats utan den här raden.
    ignoreHTTPSErrors: true,

    // Sparas bara när ett test failar. Då kan du öppna spårningen och se
    // exakt vad som hände, klick för klick.
    trace: "retain-on-failure",
    screenshot: "only-on-failure",
  },

  projects: [{ name: "chromium", use: { ...devices["Desktop Chrome"] } }],

  // Startar Vite åt dig. Kör du redan npm run dev återanvänds den servern.
  webServer: {
    command: "npm run dev",
    url: "http://localhost:5173",
    reuseExistingServer: !process.env.CI,
    timeout: 60_000,
  },
});
