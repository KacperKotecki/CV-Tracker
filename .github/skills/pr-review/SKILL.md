---
name: pr-review
description: Principal-level PR review pass for CV Tracker — verify diff, run mandates, post material findings only.
---

# PR Review Skill for CV Tracker

This skill performs a comprehensive, Principal-level review of a GitHub pull request against CV Tracker's architectural mandates and .NET 10 / React 19 conventions. It focuses on **material findings** only — architectural integrity, maintainability, performance, and long-term health.

## Core Mandates for Review

Follow these mandates to review every changed file between the verified source and target.

### 0. General Principles

- Always consider the **Principal-level perspective**: focus on architectural integrity, maintainability, performance, and long-term health of the codebase.
- Be proactive in identifying **potential issues** relevant to CV Tracker conventions (no service layer, enum HasConversion, DTO locations, TypeScript mirror).
- Be positive and constructive; provide **concrete suggestions** for fixes with code snippets.
- Skip style/formatting (EditorConfig handles this). Skip nits. Material findings only.

### 1. Architecture & Dependency Injection

**What to flag:**

- **Service or Repository class introduced** — controllers must call `AppDbContext` directly. Any `IJobApplicationService`, `IJobOfferRepository`, or similar is a hard violation.
- **Optional constructor parameters** (`Type? type = null`) on DI-registered services — dependencies must be explicit.
- **Manual checks** replaceable by native .NET primitives (e.g., custom URL validation when `Uri.TryCreate` exists).
- **DI scope mismatch** — `Scoped` per HTTP request for `AppDbContext`, `Singleton` for stateless helpers, `Transient` only when explicitly required.
- Any **`.Result` or `.Wait()`** on async methods — use `await` consistently.
- Any **`HttpClient` created directly** (`new HttpClient()`) — use `IHttpClientFactory`.
- Missing **`CancellationToken`** parameters on async controller actions.
- Unnecessary interface abstractions where a concrete class suffices.

**Severity:** Blocker (Service/Repository class, DI scope mismatch, `.Result/.Wait`). Major (direct `HttpClient`, missing `CancellationToken`). Minor (unnecessary interface).

### 2. Concurrency & Memory Management

**What to flag:**

- **ConcurrentDictionary / locks** without cleanup strategy — prefer `SemaphoreSlim` over `lock` for async operations.
- **Shared mutable state** in singleton service fields. Singletons must be thread-safe or immutable.
- **`IDisposable` / `IAsyncDisposable`** not wrapped in `using` / `await using` (MemoryStream, StreamWriter, manual DbContext creation).
- **Unbounded collections** in long-lived services (memory leaks in cache, event handlers not unsubscribed).

**Severity:** Major (shared mutable state in singleton, IDisposable not disposed, unbounded growth). Minor (lock instead of SemaphoreSlim in async code).

### 3. Null Safety & Side Effects

**What to flag:**

- **Null dereference without guard** — every `FirstOrDefaultAsync` / `FindAsync` / `T?` return must have `if (x is null) return NotFound()` (or `BadRequest`) before dereference.
- **I/O in constructors** — constructors must not call database, HTTP, or filesystem. Use factory methods or lazy initialization.
- **`DateTime.UtcNow` directly in production code** — inject `TimeProvider` for determinism.
- **`HttpClient` created directly** — use `IHttpClientFactory` to avoid port exhaustion.

**Severity:** Major.

### 4. CV Tracker-Specific Rules

#### No Service / Repository Layer

- **Any class named `*Service` or `*Repository`** in `CvTracker.Api/` — controllers call `AppDbContext` directly. No exceptions.
- **Injecting a custom interface** into a controller when the interface wraps only `AppDbContext` calls — remove the indirection.

**Severity:** Blocker.

#### Enum HasConversion

- **New enum property on an EF Core entity without `.HasConversion<string>()`** in `AppDbContext.OnModelCreating` — the value will be stored as an integer, breaking the SQLite schema and the JSON serialization contract.
- **New enum added but not registered** in the model builder — check `AppDbContext.OnModelCreating` for the entity's configuration block.

**Severity:** Blocker.

#### DTO and Entity Locations

- **DTO placed outside `CvTracker.Api/Controllers/Models/DTOs/`** — wrong folder breaks the established layout.
- **Entity placed outside `CvTracker.Api/Controllers/Models/`** — same issue.

**Severity:** Major.

#### TypeScript Model Mirror

- **C# DTO changed (field added/removed/renamed/retyped) but `CvTracker.Client/models/` not updated** — frontend will silently break at runtime.
- **Enum value in TypeScript not matching C# enum name exactly** (e.g., `"active"` instead of `"Active"`) — `JsonStringEnumConverter` serializes with exact C# name casing.

**Severity:** Blocker (missing field sync, wrong enum casing). Major (extra field in TypeScript not in C#).

#### No Authentication Attributes

- **`[Authorize]`, `RequireAuthorization()`, `[AllowAnonymous]`** anywhere — CV Tracker has no auth. Adding these breaks the app.

**Severity:** Blocker.

#### JSON Serialization

- **`Newtonsoft.Json`** introduced — project uses `System.Text.Json` with `JsonStringEnumConverter` registered globally. If a library forces Newtonsoft, isolate to that boundary.
- **Enum serialized as integer** — `JsonStringEnumConverter` is registered globally; all enums must serialize as strings. A new enum bypassing `HasConversion<string>()` will also mismatch here.

**Severity:** Major (Newtonsoft outside forced boundary, integer enum in JSON).

#### Secrets

- **API keys or connection strings hardcoded** — use .NET User Secrets for dev (`dotnet user-secrets set`). Never commit to `appsettings.Development.json`.

**Severity:** Blocker.

### 5. Frontend (TypeScript / React)

**What to flag:**

- **Unused imports or variables** — `noUnusedLocals` and `noUnusedParameters` are enabled; build will fail.
- **`any` type introduced** — defeats TypeScript safety; use proper types or `unknown`.
- **`fetch` call to wrong port** — API dev port is `http://localhost:5161`. Frontend dev port is `http://localhost:5173`.
- **React Router `<Link>` / `useNavigate`** used correctly (v7 API from `react-router-dom`).

**Severity:** Major (unused import/var — breaks build, `any` type). Minor (wrong port in dev-only code).

---

## Workflow

1. **Verify PR metadata**:
   ```bash
   gh pr view <num> --json number,title,baseRefName,headRefName,files,additions,deletions
   ```
   Confirm the PR is targeting the correct base branch.

2. **Get the diff**:
   ```bash
   gh pr diff <num>
   ```
   Review every changed file against the mandates above.

3. **Apply mandates per file**:
   - For each file in the diff, check sections 0-5.
   - If a mandate is violated, note the file, line number, mandate category, and suggested fix.

4. **Post material findings**:
   - Each material finding (Blocker/Major severity) must be posted as a **separate comment** on the PR:
     ```bash
     gh pr review <num> --comment -b "**[Blocker] Service class introduced**\n\nFile: CvTracker.Api/Services/JobApplicationService.cs\n\nControllers must call AppDbContext directly. No service/repository layer allowed.\n\n**Suggested fix:** Move logic back into the controller action and inject AppDbContext directly."
     ```

5. **Skip style-only / nit feedback**:
   - Do not comment on whitespace, member ordering, or subjective naming if EditorConfig/analyzers passed.
   - Do not comment on pre-existing code outside the PR scope unless it's a critical bug introduced by the change.

6. **If no material findings**:
   ```bash
   gh pr review <num> --approve -b "✅ No material findings. All mandates verified:\n- Architecture & DI: PASS\n- Concurrency & memory: PASS\n- Null safety & side effects: PASS\n- CV Tracker rules (no service layer, HasConversion, DTO locations, TS mirror): PASS\n- Frontend TypeScript: PASS"
   ```

---

## Severity Convention

| Severity | Definition | Action |
|---|---|---|
| **Blocker** | Violates architectural invariant (Service/Repository class, missing HasConversion, missing TS model sync, secret in code, auth attribute added). | Blocks merge. Request changes. |
| **Major** | Correctness risk or significant maintainability issue (DI scope, `.Result/.Wait`, wrong DTO folder, `any` type). | Should fix before merge. |
| **Minor** | Low risk, low impact. Can be addressed in a follow-up. | Comment, do not block. |

---

## Example Finding

```
[Blocker] Missing HasConversion<string>() for new enum

File: CvTracker.Api/Controllers/Models/JobOffer.cs:18
File: CvTracker.Api/Data/AppDbContext.cs

JobOffer.Priority is a new enum property. AppDbContext.OnModelCreating has no
.HasConversion<string>() for it — SQLite will store integers and JSON will
serialize integers, breaking the frontend TypeScript model.

Suggested fix — add to AppDbContext.OnModelCreating:
  entity.Property(e => e.Priority).HasConversion<string>();
```

---

**Note:** This skill reads the PR diff and posts findings via `gh`. It does NOT commit changes or merge the PR.
