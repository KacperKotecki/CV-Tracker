# Pull Request

## What and why

<!-- Brief: what problem does this PR solve? Which issue/task does it address? -->

Fixes: #

## How it was tested

<!-- Check what was verified: -->

- [ ] Manual testing in local environment (`dotnet run --project CvTracker.Api` + `npm run dev`)
- [ ] Verified in Swagger UI / Postman / curl
- [ ] Frontend tested in browser (http://localhost:5173)

**Test details:**

<!-- Optional: add screenshots, logs, reproduction steps -->

## AI code review checklist

Before submitting, run the **pr-review skill** for automated verification:

```bash
# In Copilot Chat:
# "@workspace Use the pr-review skill to review my staged changes"
```

- [ ] AI review completed — no blocking issues or all fixed

## Self-review checklist

- [ ] No Repository class introduced — controllers inject services; services call `AppDbContext` directly
- [ ] New enum properties have `.HasConversion<string>()` in `AppDbContext.OnModelCreating`
- [ ] DTOs are in `CvTracker.Api/Controllers/Models/DTOs/`, entities in `CvTracker.Api/Controllers/Models/`
- [ ] C# DTO changes are mirrored in `CvTracker.Client/models/`
- [ ] No unused TypeScript imports/variables (`noUnusedLocals` / `noUnusedParameters` enabled)
- [ ] No secrets or API keys committed

## Breaking changes

- [ ] No breaking changes
- [ ] **Breaking:** _(describe what changes and how to migrate / re-run migrations)_

## Additional notes

<!-- Links, screenshots, diagrams, comments for reviewers -->
