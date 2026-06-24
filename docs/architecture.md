# Architecture

## System Overview

```
┌─────────────────────────────────────────────────────────────────┐
│                        Client Layer                             │
│                                                                 │
│   ┌──────────┐    ┌──────────┐    ┌──────────────────────────┐  │
│   │ Swagger  │    │ Postman  │    │  Any HTTP Client (curl)  │  │
│   └────┬─────┘    └────┬─────┘    └────────────┬─────────────┘  │
│        └───────────────┼───────────────────────┘                │
└────────────────────────┼────────────────────────────────────────┘
                         │ HTTP (port 5050)
┌────────────────────────┼────────────────────────────────────────┐
│                   WordleApi.Host                                │
│                                                                 │
│   ┌────────────────────┴──────────────────────┐                 │
│   │              ASP.NET Core                  │                │
│   │         Controllers (API Layer)            │                │
│   │                                            │                │
│   │  ┌─────────────────┐  ┌────────────────┐  │                │
│   │  │ GamesController │  │ Leaderboard    │  │                │
│   │  │                 │  │ Controller     │  │                │
│   │  │  POST /games    │  │                │  │                │
│   │  │  GET  /games/id │  │  GET /leader.. │  │                │
│   │  │  POST /guesses  │  │  GET /players  │  │                │
│   │  │  DEL  /games/id │  │                │  │                │
│   │  └────────┬────────┘  └───────┬────────┘  │                │
│   └───────────┼───────────────────┼───────────┘                │
│               │                   │                             │
│   ┌───────────┴───────────────────┴───────────┐                │
│   │            Services Layer                  │                │
│   │                                            │                │
│   │  ┌─────────────────┐  ┌────────────────┐  │                │
│   │  │  GameService    │  │  WordService   │  │                │
│   │  │                 │  │                │  │                │
│   │  │  • Create game  │  │  • Load words  │  │                │
│   │  │  • Submit guess │  │  • Validate    │  │                │
│   │  │  • Score calc   │  │  • Evaluate    │  │                │
│   │  │  • Game state   │  │    (2-pass)    │  │                │
│   │  └────────┬────────┘  └────────────────┘  │                │
│   └───────────┼───────────────────────────────┘                │
│               │                                                 │
│   ┌───────────┴───────────────────────────────┐                │
│   │            Data Layer (Dapper)             │                │
│   │                                            │                │
│   │  ┌─────────────────┐  ┌────────────────┐  │                │
│   │  │ GameRepository  │  │ Leaderboard    │  │                │
│   │  │                 │  │ Repository     │  │                │
│   │  └────────┬────────┘  └───────┬────────┘  │                │
│   │           │                   │            │                │
│   │  ┌────────┴───────────────────┴────────┐  │                │
│   │  │  NpgsqlConnectionFactory            │  │                │
│   │  │  DatabaseInitializer (migrations)   │  │                │
│   │  └────────────────┬────────────────────┘  │                │
│   └───────────────────┼───────────────────────┘                │
└───────────────────────┼────────────────────────────────────────┘
                        │ TCP (port 5432)
┌───────────────────────┼────────────────────────────────────────┐
│                  PostgreSQL                                     │
│                                                                 │
│   ┌───────────┐  ┌───────────┐  ┌──────────────────────────┐  │
│   │  games    │  │  guesses  │  │  leaderboard_rankings    │  │
│   │  table    │  │  table    │  │  (materialized view)     │  │
│   └───────────┘  └───────────┘  └──────────────────────────┘  │
│                                                                 │
│   ┌──────────────────┐                                         │
│   │ schema_migrations│                                         │
│   └──────────────────┘                                         │
└────────────────────────────────────────────────────────────────┘
```

## Game Flow

```
Player                    API                         Database
  │                        │                            │
  │  POST /api/games       │                            │
  │───────────────────────>│  Generate secret word      │
  │                        │  INSERT games              │
  │                        │───────────────────────────>│
  │  201 { gameId, ... }   │                            │
  │<───────────────────────│                            │
  │                        │                            │
  │  POST /guesses         │                            │
  │  { word: "crane" }     │                            │
  │───────────────────────>│  Validate word             │
  │                        │  Evaluate (2-pass algo)    │
  │                        │  INSERT guesses            │
  │                        │  UPDATE games              │
  │                        │───────────────────────────>│
  │  200 { letters, ... }  │                            │
  │<───────────────────────│                            │
  │                        │                            │
  │  ... repeat up to 6x   │                            │
  │                        │                            │
  │  (on win)              │  Calculate score           │
  │                        │  UPDATE games (Won, score) │
  │                        │  REFRESH MATERIALIZED VIEW │
  │                        │───────────────────────────>│
  │  200 { Won, score }    │                            │
  │<───────────────────────│                            │
```

## Guess Evaluation Algorithm

Two-pass algorithm that correctly handles duplicate letters:

**Pass 1 — Exact matches:**
Mark letters that match both value and position as `Correct`. Decrement the available count for that letter.

**Pass 2 — Present/Absent:**
For remaining letters, check if the letter exists in the available pool. If yes, mark `Present` and decrement. Otherwise, mark `Absent`.

Example: Secret = `CREEP`, Guess = `SPEED`
```
Pass 1: S(-) P(-) E(✓) E(✓) D(-)     Available: {C:1, R:1, P:1}
Pass 2: S(✗) P(↔) -    -    D(✗)     Available: {C:1, R:1}

Result: [Absent, Present, Correct, Correct, Absent]
```

## Database Schema

```
games                          guesses
├── game_id (PK, UUID)         ├── guess_id (PK, UUID)
├── player_name (TEXT)         ├── game_id (FK → games)
├── secret_word (TEXT)         ├── attempt_number (INT)
├── status (INT)               ├── word (TEXT)
├── attempts_used (INT)        ├── result_json (JSONB)
├── score (INT, nullable)      └── guessed_at (TIMESTAMPTZ)
├── started_at (TIMESTAMPTZ)
└── completed_at (TIMESTAMPTZ)

leaderboard_rankings (materialized view)
├── game_id, player_name, attempts_used
├── time_taken_seconds, score
├── completed_at, secret_word
└── Refreshed concurrently after each win

schema_migrations
├── version (PK, INT)
├── name (TEXT)
└── applied_at (TIMESTAMPTZ)
```

## CI/CD Pipeline

```
Push/PR to main                    Tag v*
      │                              │
      ▼                              ▼
┌──────────────┐           ┌──────────────────┐
│  build.yml   │           │ publish-images   │
│              │           │                  │
│  Restore     │           │  Checkout        │
│  Build       │           │  Login to GHCR   │
│  Unit Tests  │           │  Derive tags     │
│  Integration │           │  Docker Buildx   │
│    Tests     │           │  Build & Push    │
│  Publish     │           │    (linux/amd64) │
│  Artifacts   │           │                  │
└──────────────┘           └──────────────────┘
```

## Docker Deployment

```bash
# Full stack (API + Postgres)
docker compose -f docker-compose.yml -f docker-compose.postgres.yml up --build

# API only (connect to external Postgres)
POSTGRES_CONNECTION="Host=...;..." docker compose up --build
```

The Dockerfile uses a multi-stage build:
1. **Build stage** — SDK image, restore, publish
2. **Runtime stage** — ASP.NET runtime image only (~220MB)
