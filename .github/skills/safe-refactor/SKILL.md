---
name: safe-refactor
description: Guided refactoring workflow — identify legacy hotspot, verify baseline build, apply small-step transformations, verify build after each step.
---

# Safe Refactor Skill for CV Tracker

This skill implements a disciplined refactoring workflow for CV Tracker (.NET 10 Web API + React 19 TypeScript). It ensures a clean build baseline exists before any transformation, applies changes in small incremental steps, and verifies build success after each step.

## When to Use

- User requests `/refactor <target>` or "refactor X without breaking Y".
- You identify a hotspot: complex controller action, duplicated mapping logic, long method, deeply nested conditions.
- Code smells require structural improvement — but behavior must not change.
- Before extracting a shared helper from controller actions.

## Workflow

1. **Identify the refactor target**:
   - If user specified, confirm the file/class/method.
   - If not, propose based on complexity (long methods, duplicated mapping, magic strings).

2. **Baseline verification**:
   - Before touching anything, confirm the build is green:
     ```powershell
     dotnet build "CV Tracker.sln" --nologo
     ```
   - If frontend files are in scope, also:
     ```powershell
     cd CvTracker.Client && npm run build
     ```
   - If baseline is broken, STOP. Report to human — do not refactor broken code.

3. **Plan small steps**:
   - Break the refactor into 3-7 small transformations. Each must be independently verifiable.
   - Examples suitable for CV Tracker:
     - Extract private `MapToDto()` helper from controller action
     - Rename parameter/variable for clarity
     - Consolidate duplicated `if (entity == null) return NotFound()` into a guard helper
     - Replace magic string with a named constant
     - Simplify a multi-step LINQ chain

4. **Apply one step at a time**:
   - Make the transformation.
   - Verify:
     ```powershell
     dotnet build "CV Tracker.sln" --nologo
     ```
     For frontend changes:
     ```powershell
     cd CvTracker.Client && npm run build
     ```
   - If build fails: revert, adjust the step, retry. Never accumulate broken steps.
   - If build passes: proceed to next step.

5. **Final verification**:
   - After all steps, run both builds and tests clean:
     ```powershell
     dotnet build "CV Tracker.sln" --nologo
     dotnet test CvTracker.Api.Tests/CvTracker.Api.Tests.csproj --no-build --nologo
     cd CvTracker.Client && npm run build
     ```
   - Confirm CV Tracker invariants still hold (see below).

## CV Tracker invariants — never break during refactoring

- ✅ Controllers still call `AppDbContext` directly — no `Service` or `Repository` class introduced.
- ✅ All enum properties on EF Core entities still have `.HasConversion<string>()` in `AppDbContext.OnModelCreating`.
- ✅ DTOs remain in `CvTracker.Api/Controllers/Models/DTOs/`; entities in `CvTracker.Api/Controllers/Models/`.
- ✅ TypeScript interfaces in `CvTracker.Client/models/` still mirror their C# counterparts.
- ✅ No unused imports or variables in TypeScript (`noUnusedLocals` / `noUnusedParameters` enabled).
- ✅ All HTTP status codes unchanged (don't accidentally turn a `404` into a `200`).

## Example

**User:** `refactor JobApplicationsController — MapToDto is duplicated in 4 actions`

```
Step 1: Identify — confirmed, mapping of JobOffer → JobOfferDto duplicated in GET list + GET by id + POST + PUT.
Step 2: Baseline — dotnet build "CV Tracker.sln" --nologo → PASS
         dotnet test CvTracker.Api.Tests/CvTracker.Api.Tests.csproj --no-build --nologo → PASS
Step 3: Extract private static MapToDto(JobOffer o) → JobOfferDto helper method.
        dotnet build "CV Tracker.sln" --nologo → PASS
        dotnet test CvTracker.Api.Tests/CvTracker.Api.Tests.csproj --no-build --nologo → PASS
Step 4: Replace all 4 inline projections with MapToDto() call.
        dotnet build "CV Tracker.sln" --nologo → PASS
        dotnet test CvTracker.Api.Tests/CvTracker.Api.Tests.csproj --no-build --nologo → PASS
Final: Invariants verified — no Repository layer introduced, DTOs unchanged, build + tests clean.
```

## Reference

This skill uses `.github/prompts/refactor.prompt.md` for additional CV Tracker-specific refactor guidance (invariants checklist, output format, red flags).

## What this skill does NOT do

- Does not add new test files beyond fixing broken tests — the test project is `CvTracker.Api.Tests/`; adding new tests for a refactor is the implementer's responsibility, not the refactorer's.
- Does not change behavior — if new logic is required, use the full agentic pipeline instead:
  `pwsh ./scripts/agent-run.ps1 init -RunId <id> -PromptPath <prompt>`
- Does not introduce service/repository layers — this violates a hard invariant.
