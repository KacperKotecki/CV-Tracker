# CV Tracker — Copilot Instructions

Personal portfolio project: a Kanban-style job application tracker with AI-powered offer scraping.

## Stack

| Layer | Tech |
|-------|------|
| API | .NET 10, ASP.NET Core Web API, EF Core 10 (SQLite) |
| Frontend | React 19, TypeScript, Vite, React Router v7 |
| AI scraping | OpenRouter API (configurable model) |

## Project layout

```
CvTracker.Api/
├── Controllers/
│   ├── JobApplicationsController.cs   # CRUD for job offers + notes + status patch
│   ├── ScrapeController.cs            # scrape + LLM-parse a URL into ScrapedOfferDto
│   ├── ProfileController.cs           # GET/PUT user profile, avatar, resume, skills
│   ├── TechnologiesController.cs      # GET /api/technologies — grouped technology list
│   └── Models/
│       ├── JobOffer.cs                # EF Core entity
│       ├── JobOfferNote.cs            # EF Core entity — notes per offer
│       ├── JobOfferTechnology.cs      # EF Core join entity — offer ↔ technology
│       ├── Technology.cs              # EF Core entity — canonical skill/technology
│       ├── TechnologyAlias.cs         # EF Core entity — aliases for skill normalization
│       ├── UserProfile.cs             # EF Core entity
│       ├── UserTechnology.cs          # EF Core entity — user skill with proficiency (1–5)
│       ├── ApplicationStatus.cs       # enum → stored as string
│       ├── ContractType.cs            # enum → stored as string
│       ├── WorkLoad.cs                # enum → stored as string
│       ├── WorkMode.cs                # enum → stored as string
│       └── DTOs/
│           ├── JobOfferDto.cs
│           ├── JobOfferNoteDto.cs
│           ├── ScrapedOfferDto.cs
│           ├── TechnologyCategoryDto.cs
│           ├── TechnologyDto.cs
│           ├── UpdateUserProfileRequest.cs
│           ├── UpdateUserTechnologiesRequest.cs
│           ├── UserProfileDto.cs
│           └── UserTechnologyDto.cs
├── Services/
│   ├── IJobOfferService.cs
│   ├── JobOfferService.cs
│   ├── ISkillNormalizationService.cs  # resolves raw skill text → Technology ID
│   ├── SkillNormalizationService.cs
│   ├── ISkillSeedingService.cs        # seeds Technologies + Aliases from jobOfferSkills.json
│   └── SkillSeedingService.cs
├── Data/AppDbContext.cs               # single DbContext, used by services and ProfileController
└── Migrations/                        # EF Core migrations

CvTracker.Api.Tests/                   # xUnit tests (JobOfferService, SalaryParser, ScraperFactory)

CvTracker.Client/
├── models/                            # TypeScript interfaces mirroring API models
└── src/
    ├── components/                    # reusable UI components
    └── pages/                         # route-level pages (Dashboard, OffersPage, ProfilePage)
```

## Key conventions

### Backend

- **Service layer for job offers**: `JobApplicationsController` injects `IJobOfferService`; the service calls `AppDbContext` directly. `ProfileController` and `TechnologiesController` call `AppDbContext` directly. No repository pattern anywhere.
- **Enums stored as strings** in SQLite via `HasConversion<string>()` in `AppDbContext.OnModelCreating`. Always add `HasConversion<string>()` for new enum properties.
- **JSON enum serialization**: `JsonStringEnumConverter` is registered globally in `Program.cs` — enums arrive as strings from the API.
- **No authentication** — this is a single-user personal tool.
- **CORS**: `AllowReact` policy allows `http://localhost:5173` (Vite dev server). New endpoints do not need CORS changes.
- **DTOs** live under `Controllers/Models/DTOs/`. Entities live under `Controllers/Models/`.
- **Skill seeding**: `Technologies` and `TechnologyAliases` are seeded at startup from `CvTracker.Api/jobOfferSkills.json` via `ISkillSeedingService`. `ISkillNormalizationService` (singleton) resolves raw skill text → `Technology` ID using the seeded aliases.
- **OpenRouter API key** must be stored in .NET user secrets (`dotnet user-secrets set "OpenRouter:ApiKey" "<value>"`), not in `appsettings.Development.json`.

### Frontend

- **TypeScript interfaces** in `models/` must mirror the C# models (enum values as string literals).
- **React Router v7** — use `useNavigate` / `useParams` from `react-router-dom`.
- No Redux or global state — local `useState` + `fetch` calls to `http://localhost:5161` (API dev port).

## Build & run commands

```bash
# API (from repo root)
dotnet build "CV Tracker.sln"
dotnet run --project CvTracker.Api
dotnet test "CvTracker.Api.Tests/CvTracker.Api.Tests.csproj"

# Migrations
dotnet ef migrations add <Name> --project CvTracker.Api
dotnet ef database update --project CvTracker.Api

# Frontend (from CvTracker.Client/)
npm install
npm run dev       # http://localhost:5173
npm run build     # tsc + vite build
npm run lint      # eslint
```

## Architecture docs

- [docs/ARCHITECTURE.md](../docs/ARCHITECTURE.md) — application architecture diagram and project structure
- [docs/RUNBOOK.md](../docs/RUNBOOK.md) — operational runbook: local dev, commands, troubleshooting
- [docs/agent-decisions.md](../docs/agent-decisions.md) — agentic pipeline decision log
