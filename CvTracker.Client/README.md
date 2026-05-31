# CV Tracker — Frontend

React 19 + TypeScript + Vite frontend for the CV Tracker job application manager.

## Stack

| | |
|---|---|
| Framework | React 19 |
| Build tool | Vite |
| Language | TypeScript |
| Routing | React Router v7 |
| Linting | ESLint |

## Project structure

```
CvTracker.Client/
├── models/          # TypeScript interfaces mirroring C# API models
├── src/
│   ├── App.tsx      # Route tree: / (OffersPage), /dashboard, /profile
│   ├── components/  # Reusable UI components
│   ├── pages/       # Route-level pages (Dashboard, OffersPage, ProfilePage)
│   └── utils/       # Shared helpers (offerUtils.ts)
└── public/
```

## Pages

| Route | Component | Description |
|---|---|---|
| `/` | `OffersPage` | Full offer list + detail panel + add/edit form |
| `/dashboard` | `Dashboard` | Kanban board — columns per `ApplicationStatus` |
| `/profile` | `ProfilePage` | User profile editor with skills/technologies manager |

## Commands

```bash
# Install dependencies
npm install

# Development server (http://localhost:5173)
npm run dev

# Production build (output: dist/)
npm run build

# Lint
npm run lint
```

## API

All `fetch` calls target `http://localhost:5161` (ASP.NET Core dev port). See the full endpoint list in [`docs/RUNBOOK.md`](../docs/RUNBOOK.md).

The offer text-parsing feature (`POST /api/parse`) uses **local regex heuristics** on the backend — no external API key is required.
