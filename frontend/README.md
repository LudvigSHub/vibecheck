# React + Vite

This template provides a minimal setup to get React working in Vite with HMR and some ESLint rules.

Currently, two official plugins are available:

- [@vitejs/plugin-react](https://github.com/vitejs/vite-plugin-react/blob/main/packages/plugin-react) uses [Oxc](https://oxc.rs)
- [@vitejs/plugin-react-swc](https://github.com/vitejs/vite-plugin-react/blob/main/packages/plugin-react-swc) uses [SWC](https://swc.rs/)

## React Compiler

The React Compiler is not enabled on this template because of its impact on dev & build performances. To add it, see [this documentation](https://react.dev/learn/react-compiler/installation).

## Expanding the ESLint configuration

If you are developing a production application, we recommend using TypeScript with type-aware lint rules enabled. Check out the [TS template](https://github.com/vitejs/vite/tree/main/packages/create-vite/template-react-ts) for information on how to integrate TypeScript and [`typescript-eslint`](https://typescript-eslint.io) in your project.


## Database Migrations

We use Entity Framework Core migrations to keep the database schema synchronized between team members.

### Migration Rules

To avoid migration conflicts, follow these rules:

1. **Always pull before creating a migration**

   Before creating a new migration, make sure your local branch is up to date:

   ```bash
   git pull
   ```

2. **Only one person creates a migration at a time**

   Migrations should not be created by multiple team members simultaneously.

3. **Tell the team before creating a migration**

   Write in the team chat that you are about to create a migration.

   Example:

   > "I'm creating a migration for the new Quiz changes."

   Wait until everyone knows that you are working on the migration before creating it.

4. **Use a descriptive migration name**

   Migration names should describe the database change.

   Example:

   ```text
   AddQuizCategories
   ```

5. **Commit the migration together with the model changes**

   The migration should be committed along with the code changes that require it.

   ```bash
   git add .
   git commit -m "Add quiz categories"
   git push
   ```

### If a Migration Conflict Occurs

If someone else has created a migration while you were working:

**Do not create another migration on top of it immediately.**

First:

1. Stop and tell the team in the chat.

2. Pull the latest changes:

   ```bash
   git pull
   ```

3. Check the current models and migrations.

4. If your model changes are still needed, create a new migration **after** pulling the latest migration.

5. If Git reports conflicts in the migration files, do not blindly resolve them. Ask the team and resolve the database changes together.

### General Rule

> **Pull → Tell the team → One person creates the migration → Commit & push → Everyone pulls**

Migrations are shared database history, so they should be treated as a coordinated team change rather than something each developer creates independently.