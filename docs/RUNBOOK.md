# Runbook

Operational notes for running CV Tracker locally.

## Local development

### First run

```bash
# 1. API — from repo root
dotnet build "CV Tracker.sln"
dotnet ef database update --project CvTracker.Api   # creates CVTracker.db
dotnet run --project CvTracker.Api                  # http://localhost:5161

# 2. Frontend — in a second terminal
cd CvTracker.Client
npm install
npm run dev   # http://localhost:5173
```

### OpenRouter API key (required for scraping)

The key must be stored in .NET user secrets — **never** in `appsettings*.json`:

```bash
dotnet user-secrets init --project CvTracker.Api
dotnet user-secrets set "OpenRouter:ApiKey" "<your-key>" --project CvTracker.Api
```

Optionally override the model (default: `mistralai/mistral-7b-instruct:free`):

```bash
dotnet user-secrets set "OpenRouter:Model" "openai/gpt-4o-mini" --project CvTracker.Api
```

### Reset the database

```bash
rm CvTracker.Api/CVTracker.db
dotnet ef database update --project CvTracker.Api
```

## Common operations

### Add a new EF Core migration

```bash
dotnet ef migrations add <Name> --project CvTracker.Api
dotnet ef database update --project CvTracker.Api
```

### Build for production

```bash
# API
dotnet publish CvTracker.Api -c Release

# Frontend
cd CvTracker.Client
npm run build   # output: dist/
```

### Lint frontend

```bash
cd CvTracker.Client
npm run lint
```

## API endpoints

| Method | Route | Description |
|---|---|---|
| `GET` | `/api/jobapplications` | List all offers |
| `GET` | `/api/jobapplications/{id}` | Get single offer |
| `POST` | `/api/jobapplications` | Create offer |
| `PUT` | `/api/jobapplications/{id}` | Update offer |
| `DELETE` | `/api/jobapplications/{id}` | Delete offer |
| `POST` | `/api/scrape` | Scrape URL → `ScrapedOfferDto` |

Swagger UI available at `http://localhost:5161/swagger` in Development.

## Configuration

| Key | Location | Description |
|---|---|---|
| `ConnectionStrings:DefaultConnection` | `appsettings.Development.json` | SQLite file path (`Data Source=CVTracker.db`) |
| `OpenRouter:ApiKey` | .NET user secrets | Required for `/api/scrape` |
| `OpenRouter:Model` | `appsettings.json` or user secrets | LLM model identifier |

## Agent pipeline — commit message approval

When the agentic pipeline reaches the `pr-commit` phase, `scripts/pr-finalize.ps1` pauses before committing:

1. It writes a proposed commit message to `.agent-run/<run-id>/commit-message.md`.
2. The console prints `AWAITING_HUMAN: edit commit-message.md, then prepend APPROVE`.
3. Open the file, edit the message if needed, then **prepend a line with exactly `APPROVE`**:

```
APPROVE
feat(job): Add salary field to JobOffer

Plan: ./.agent-run/.../plan.approved.md
...
```

4. Save. The script detects the `APPROVE` line, strips it (and any `#` comment lines), then commits.

**Timeout:** 10 minutes. If no `APPROVE` appears, the script exits with code 4. Re-run `pr-finalize.ps1` to restart from the commit step.

The read-only reference (before your edits) is always at `commit-message.proposed.md` in the same run folder.



| Symptom | Likely cause | Fix |
|---------|-------------|-----|
| `dotnet build` fails | Source error or missing restore | `dotnet restore "CV Tracker.sln"`, then retry |
| Frontend `npm run build` fails | TypeScript error or missing packages | `npm install` in `CvTracker.Client/`; check TypeScript errors |
| `/api/scrape` returns 500 | Missing OpenRouter API key | `dotnet user-secrets set "OpenRouter:ApiKey" "<key>" --project CvTracker.Api` |
| `table not found` (SQLite) | Migration not applied | `dotnet ef database update --project CvTracker.Api` |
| CORS error in browser | API not on expected port | Confirm API is on `http://localhost:5161`; check `AllowReact` in `Program.cs` |
| Enums serialize as integers | Missing `HasConversion<string>()` | Add `.HasConversion<string>()` in `AppDbContext.OnModelCreating` |
| Frontend model out of sync with API | C# DTO changed without updating TS | Mirror changes in `CvTracker.Client/models/*.ts` |
