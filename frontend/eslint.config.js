import js from "@eslint/js";
import globals from "globals";
import reactHooks from "eslint-plugin-react-hooks";
import reactRefresh from "eslint-plugin-react-refresh";
import { defineConfig, globalIgnores } from "eslint/config";

export default defineConfig([
  // dist är byggt, resten genereras av Playwright. Inget av det är skrivet
  // för hand, så det finns ingenting att granska där.
  globalIgnores(["dist", ".features-gen", "playwright-report", "test-results"]),

  {
    files: ["**/*.{js,jsx}"],
    extends: [
      js.configs.recommended,
      reactHooks.configs.flat.recommended,
      reactRefresh.configs.vite,
    ],
    languageOptions: {
      globals: globals.browser,
      parserOptions: { ecmaFeatures: { jsx: true } },
    },
  },

  // Filer som körs av Node och inte av webbläsaren: konfigurationerna i
  // roten och Playwright-stegen. Utan det här känner ESLint inte igen
  // process, __dirname och resten av Nodes globaler.
  {
    files: ["*.config.js", "features/**/*.js"],
    languageOptions: {
      globals: globals.node,
    },
  },
]);
