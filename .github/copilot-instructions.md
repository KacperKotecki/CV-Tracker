# CV Tracker — Copilot Instructions

## Project Identity
CV Tracker is a **portfolio/learning project** built by a single developer. It is a job application tracking app — users add and browse job offers. There is no authentication, no multi-tenancy, and no production deployment. Two independently runnable projects in one solution: `CvTracker.Api` (backend) and `CvTracker.Client` (frontend).

## Tech Stack
- **Backend**: ASP.NET Core Web API (.NET 10), Entity Framework Core 10, SQLite, System.Text.Json (built-in — do NOT use Newtonsoft.Json)
- **Frontend**: React 19, TypeScript, Vite 8
- **No test projects yet** — xUnit planned when feature work stabilizes

## Project Structure

### Backend — `CvTracker.Api/`
- `Controllers/` — API controllers
- `Controllers/Models/` — EF Core entities and enums
- `Controllers/Models/DTOs/` — planned, not yet implemented
- `Controllers/Models/ViewModels/` — planned, not yet implemented
- `Repositories/` — planned, not yet implemented
- `Data/AppDbContext.cs` — EF Core DbContext (single file, all configuration here)

### Frontend — `CvTracker.Client/`
- `models/` — TypeScript interfaces (mirror backend entities, camelCase)
- `src/components/` — React functional components
- `src/App.tsx` — root component

## Architecture
Current state: **flat/simple** — controllers depend directly on `AppDbContext`. No layers, no mediator, no CQRS.  
Intended direction: introduce Repository layer, then DTOs. Do not jump ahead of this plan.

## Backend Conventions
- All controller actions must be `async Task<ActionResult<T>>`
- Inject dependencies via constructor; currently `AppDbContext` directly (no interfaces yet — repositories planned)
- Return `Ok()`, `NotFound()`, `CreatedAtAction()` — no custom response wrappers
- Private fields: `_camelCase` (e.g. `_context`)
- Use `var` where the type is obvious from the right-hand side; use explicit types otherwise
- Do NOT expose EF entities directly in responses — introduce DTOs when adding new endpoints
- Do NOT add migrations manually — always use `dotnet ef migrations add <Name>`
- Enums stored as strings, `List<string>` properties stored as JSON — both configured in `AppDbContext.OnModelCreating`
- Do NOT catch bare `Exception` without either rethrowing or logging it
- Do NOT use `Thread.Sleep` — use `await Task.Delay` if a delay is needed

## Frontend Conventions
- All HTTP calls via native `fetch` — do NOT use axios or any HTTP library
- TypeScript interfaces go in `models/` — one file per model, camelCase properties
- Props must be typed via `interface` declared inside the component file
- Do NOT use `any` type
- Components: functional only, named or default exports are both acceptable
- `useState` / `useEffect` for local state and data fetching — no external state library

## Security Rules (apply to all code)
- Never hardcode secrets, API keys, or connection strings in source files
- Validate all user inputs at the API boundary (controller level)
- Do NOT return stack traces, exception messages, or internal error details to the client
- Use parameterized queries only — EF Core handles this; do NOT use raw SQL string interpolation
- Do NOT log sensitive user data (passwords, personal identifiers)

## Dev Commands
```bash
# Backend
cd CvTracker.Api && dotnet run           # starts on http://localhost:5211
dotnet ef migrations add <Name>
dotnet ef database update

# Frontend
cd CvTracker.Client && npm run dev       # starts on http://localhost:5173
```

## Known Planned Work (do not implement unless asked)
- DTOs for all endpoints
- Repository pattern layer
- React Router for navigation
- Proper error handling and validation responses
- Unit tests (xUnit)
