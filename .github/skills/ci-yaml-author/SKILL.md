---
name: ci-yaml-author
description: Author or modify GitHub Actions workflows — verify triggers, concurrency, permissions, branch protection integration, secrets best practices, OIDC over PAT.
---

# CI YAML Author Skill for CV Tracker

This skill creates or modifies GitHub Actions workflow files (`.github/workflows/*.yml`) for CV Tracker (.NET 10 Web API + React 19 TypeScript, no test project). It ensures every workflow follows GitHub Actions security best practices and covers both the .NET API and the React frontend.

## Core Rules

### 1. Triggers
- Use `pull_request` for validation workflows (build, lint).
- Use `push` on `main` for deployment or build artefact workflows.
- Always scope `paths` to avoid running CI on irrelevant file changes:
  - Backend: `['CvTracker.Api/**', 'CvTracker.Api/*.csproj', 'CV Tracker.sln']`
  - Frontend: `['CvTracker.Client/**']`

### 2. Concurrency
- Every workflow **must** have a `concurrency` block to cancel in-flight runs:
  ```yaml
  concurrency:
    group: ${{ github.workflow }}-${{ github.ref }}
    cancel-in-progress: true
  ```

### 3. Permissions
- Set top-level permissions to `read-all` or the minimal needed set.
- For workflows that post PR comments (e.g., review bots): add `pull-requests: write`.
- For OIDC-based deployments: add `id-token: write` at the job level.
- **Never** grant `write-all` globally.

### 4. Secrets and OIDC
- Prefer **OIDC** over long-lived PATs for cloud deployments (no expiry, no secret rotation).
- `GITHUB_TOKEN` is sufficient for most CI tasks — no PAT needed.
- The OpenRouter API key must be stored as a secret (`OPENROUTER_API_KEY`) — never hardcoded in workflow YAML.
- Reference secrets only from environments that need them.

### 5. Build steps for CV Tracker

**Backend (.NET 10):**
```yaml
- name: Setup .NET
  uses: actions/setup-dotnet@v4
  with:
    dotnet-version: '10.x'

- name: Restore
  run: dotnet restore "CV Tracker.sln"

- name: Build
  run: dotnet build "CV Tracker.sln" --no-restore --nologo -c Release
```
> ⚠️ No `dotnet test` step — CV Tracker has no test project.

**Frontend (React + TypeScript + Vite):**
```yaml
- name: Setup Node.js
  uses: actions/setup-node@v4
  with:
    node-version: '22'
    cache: 'npm'
    cache-dependency-path: CvTracker.Client/package-lock.json

- name: Install frontend deps
  working-directory: CvTracker.Client
  run: npm ci

- name: Build frontend
  working-directory: CvTracker.Client
  run: npm run build

- name: Lint frontend
  working-directory: CvTracker.Client
  run: npm run lint
```

### 6. Caching
- Cache NuGet packages with `actions/cache` using the lock file as the cache key:
  ```yaml
  - uses: actions/cache@v4
    with:
      path: ~/.nuget/packages
      key: ${{ runner.os }}-nuget-${{ hashFiles('**/*.csproj') }}
      restore-keys: |
        ${{ runner.os }}-nuget-
  ```
- Cache npm via `actions/setup-node` `cache: 'npm'` (see frontend build step above).

### 7. Branch Protection Integration
- Status checks must be named exactly as they appear in the workflow's `jobs.<job-id>` key.
- Recommend requiring these checks in branch protection:
  - `build-api` — .NET build
  - `build-client` — React build + lint
- Do not use matrix strategies for this project — the two builds are independent jobs.

### 8. Common Mistakes to Avoid
- `dotnet test` without a test project — there is no test project in CV Tracker.
- Running npm commands from the repo root — always use `working-directory: CvTracker.Client`.
- Hardcoding the API key — use `${{ secrets.OPENROUTER_API_KEY }}`.
- Granting `write-all` permissions.
- Missing `concurrency` block (in-flight runs pile up on rapid pushes).

---

## Reference Workflow — Full CI

```yaml
name: CI

on:
  pull_request:
    branches: [main]

concurrency:
  group: ${{ github.workflow }}-${{ github.ref }}
  cancel-in-progress: true

permissions:
  contents: read

jobs:
  build-api:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4

      - uses: actions/cache@v4
        with:
          path: ~/.nuget/packages
          key: ${{ runner.os }}-nuget-${{ hashFiles('**/*.csproj') }}
          restore-keys: |
            ${{ runner.os }}-nuget-

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.x'

      - name: Restore
        run: dotnet restore "CV Tracker.sln"

      - name: Build
        run: dotnet build "CV Tracker.sln" --no-restore --nologo -c Release

  build-client:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4

      - name: Setup Node.js
        uses: actions/setup-node@v4
        with:
          node-version: '22'
          cache: 'npm'
          cache-dependency-path: CvTracker.Client/package-lock.json

      - name: Install deps
        working-directory: CvTracker.Client
        run: npm ci

      - name: Build
        working-directory: CvTracker.Client
        run: npm run build

      - name: Lint
        working-directory: CvTracker.Client
        run: npm run lint
```

---

## Example Output from This Skill

When asked to add a workflow, the skill will:
1. Identify the purpose (build CI, deploy, etc.).
2. Draft the YAML following all rules above.
3. Output the path and complete content ready to commit to `.github/workflows/`.
4. Flag any conflicts with branch protection rules or missing secrets.
