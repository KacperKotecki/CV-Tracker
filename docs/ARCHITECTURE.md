# Architecture

CV Tracker is a personal portfolio project — a Kanban-style job application tracker with AI-powered offer scraping. The architecture is intentionally simple: a single ASP.NET Core Web API backed by SQLite, consumed by a React SPA.

## Runtime topology

```mermaid
flowchart LR
    Browser([Browser])

    subgraph Frontend
        React[CvTracker.Client<br/>React 19 + Vite<br/>localhost:5173]
    end

    subgraph Backend
        Api[CvTracker.Api<br/>ASP.NET Core Web API<br/>localhost:5161]
        DB[(SQLite<br/>CVTracker.db)]
    end

    subgraph External
        OR[OpenRouter API<br/>LLM scraping]
        JobSite([Job offer site])
    end

    Browser --> React
    React -- REST JSON --> Api
    Api --> DB
    Api -- fetch HTML --> JobSite
    Api -- LLM parse --> OR
```

## Project structure

```
CvTracker.Api/
├── Controllers/
│   ├── JobApplicationsController.cs   # GET/POST/PUT/PATCH/DELETE /api/jobapplications + notes
│   ├── ScrapeController.cs            # POST /api/scrape — URL → HTML → LLM → ScrapedOfferDto
│   ├── ProfileController.cs           # GET/PUT /api/profile, avatar, resume, skills
│   ├── TechnologiesController.cs      # GET /api/technologies — grouped technology list
│   └── Models/
│       ├── JobOffer.cs                # EF Core entity (Id, Position, SalaryMin, SalaryMax, enums, ...)
│       ├── JobOfferNote.cs            # EF Core entity — notes per offer (Id, JobOfferId, EventDate, Content)
│       ├── JobOfferTechnology.cs      # EF Core join entity — offer ↔ technology (composite PK)
│       ├── Technology.cs              # EF Core entity — canonical skill/technology (Id, Name, Category)
│       ├── TechnologyAlias.cs         # EF Core entity — raw text aliases for normalization
│       ├── UserProfile.cs             # EF Core entity — single-row user profile
│       ├── UserTechnology.cs          # EF Core entity — user skill with proficiency (1–5)
│       ├── ApplicationStatus.cs       # enum: Applied | Interview | Offer | Rejected | ...
│       ├── ContractType.cs            # enum: UoP | B2B | MandateContract | ...
│       ├── WorkLoad.cs                # enum: FullTime | PartTime
│       ├── WorkMode.cs                # enum: Remote | OnSite | Hybrid
│       └── DTOs/
│           ├── JobOfferDto.cs         # input DTO for create/update
│           ├── JobOfferNoteDto.cs     # input DTO for adding notes
│           ├── ScrapedOfferDto.cs     # output from scrape endpoint
│           ├── TechnologyCategoryDto.cs  # grouped technology response
│           ├── TechnologyDto.cs       # single technology item
│           ├── UpdateUserProfileRequest.cs
│           ├── UpdateUserTechnologiesRequest.cs
│           ├── UserProfileDto.cs
│           └── UserTechnologyDto.cs
├── Services/
│   ├── IJobOfferService.cs / JobOfferService.cs   # job offer CRUD + notes
│   ├── ISkillNormalizationService.cs / SkillNormalizationService.cs  # raw text → Technology ID
│   └── ISkillSeedingService.cs / SkillSeedingService.cs  # seeds Technologies from jobOfferSkills.json
├── Data/AppDbContext.cs               # single DbContext; enum → string conversions, FK config
├── Migrations/                        # EF Core migrations (SQLite)
└── Program.cs                         # DI registration, CORS, Swagger, EF Core, startup seeding

CvTracker.Client/
├── models/                            # TypeScript interfaces mirroring C# models
│   ├── JobOffer.ts
│   ├── JobOfferNote.ts
│   ├── ApplicationStatus.ts
│   ├── ContractType.ts
│   ├── WorkLoad.ts
│   ├── WorkMode.ts
│   ├── Technology.ts                  # Technology + TechnologyCategory interfaces
│   └── UserSkill.ts                   # UserTechnology + request interfaces
└── src/
    ├── App.tsx                        # React Router v7 route tree
    ├── components/                    # OfferForm, OfferSkillPicker, SkillsCard, OfferDetailPanel,
    │                                  # OfferListPanel, NotesTimeline, ProfileInfoCard, Header, ...
    └── pages/
        ├── Dashboard.tsx              # Kanban board — columns per ApplicationStatus
        ├── OffersPage.tsx             # full offer list + detail panel + add/edit form
        └── ProfilePage.tsx            # user profile editor with skills/technologies
```

## Data model

Multiple entities share the database. All enum columns are stored as strings via `HasConversion<string>()`.

### `JobOffer` (main entity)

| Field | Type | Notes |
|---|---|---|
| `Id` | `int` | PK, auto-increment |
| `Position` | `string` | required |
| `ContractType` | `ContractType` | stored as string |
| `WorkLoad` | `WorkLoad` | stored as string |
| `WorkMode` | `WorkMode` | stored as string |
| `CompanyName` | `string?` | |
| `Location` | `string?` | |
| `SourceUrl` | `string?` | validated URL |
| `Status` | `ApplicationStatus` | stored as string |
| `SalaryMin` | `decimal?` | |
| `SalaryMax` | `decimal?` | |
| `AppliedAt` | `DateTimeOffset?` | |
| `FollowUpDate` | `DateTimeOffset?` | |
| `RecruiterName` | `string?` | |
| `RecruiterContact` | `string?` | |
| `SentCvVersion` | `string?` | |
| `RejectionReason` | `string?` | |
| `Notes` | `ICollection<JobOfferNote>` | cascade delete |
| `RequiredTechnologies` | `ICollection<JobOfferTechnology>` | cascade delete |
| `MatchScore` | `int?` | `[NotMapped]` — computed at query time |
| `RequiredSkillIds` | `List<int>` | `[NotMapped]` — hydrated from join |
| `RequiredSkillNames` | `List<string>` | `[NotMapped]` — hydrated from join |

### Other entities

| Entity | Key fields |
|---|---|
| `JobOfferNote` | `Id`, `JobOfferId`, `EventDate`, `Content` |
| `JobOfferTechnology` | `JobOfferId` + `TechnologyId` (composite PK) |
| `Technology` | `Id`, `Name` (unique), `Category` |
| `TechnologyAlias` | `Id`, `Alias` (unique), `TechnologyId` |
| `UserProfile` | `Id` (always 1), `FirstName`, `LastName`, `Location`, social URLs, `AvatarFileName`, `ResumeFileName` |
| `UserTechnology` | `Id`, `TechnologyId`, `Proficiency` (1–5) |

## AI scraping pipeline

`POST /api/scrape` accepts `{ "url": "..." }` and:

1. Fetches the HTML of the job offer page via `ScrapeClient` (named `HttpClient` with browser-like headers).
2. Strips HTML tags, truncates to 12 000 chars.
3. Sends the text to OpenRouter with a structured JSON prompt asking the model to extract offer fields.
4. Parses the model response into `ScrapedOfferDto`.
5. Normalizes raw skill strings to `Technology` IDs via `ISkillNormalizationService`.
6. Returns the filled `ScrapedOfferDto` to the frontend, which pre-fills the offer form.

Model and API key are configured via:
- `OpenRouter:Model` in `appsettings.json` (default: `mistralai/mistral-7b-instruct:free`)
- `OpenRouter:ApiKey` in **.NET user secrets only** — never in `appsettings*.json`

## Key conventions

- **Service layer for job offers** — `JobApplicationsController` injects `IJobOfferService`; the service calls `AppDbContext` directly. `ProfileController` and `TechnologiesController` call `AppDbContext` directly (no service layer needed). No repository pattern.
- **Enums as strings** — every new enum property needs `.HasConversion<string>()` in `AppDbContext.OnModelCreating`.
- **No authentication** — single-user personal tool.
- **CORS** — `AllowReact` policy for `http://localhost:5173` only; configured in `Program.cs`.
- **JSON serialization** — `JsonStringEnumConverter` registered globally; enums travel as strings over the wire.
- **Skill seeding** — `Technologies` and `TechnologyAliases` are seeded at startup from `CvTracker.Api/jobOfferSkills.json` by `ISkillSeedingService`.
