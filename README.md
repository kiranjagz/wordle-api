# Wordle API

A Wordle-like word guessing game exposed as a REST API, with a PostgreSQL-backed leaderboard. Built to showcase GitHub Actions CI/CD and Docker-ready deployments.

## Tech Stack

- **.NET 10** — ASP.NET Core Web API
- **PostgreSQL** — Data storage
- **Dapper** — Micro-ORM
- **Docker** — Multi-stage container builds
- **GitHub Actions** — CI/CD pipeline with GHCR publishing

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker](https://docs.docker.com/get-docker/) (for containerized runs)
- PostgreSQL (local or via Docker)

### Run with Docker Compose (recommended)

```bash
docker compose -f docker-compose.yml -f docker-compose.postgres.yml up --build
```

The API will be available at `http://localhost:5050/swagger`.

### Run locally

1. Start a PostgreSQL instance (or use `docker compose -f docker-compose.postgres.yml up postgres`)
2. Update the connection string in `appsettings.Development.json` if needed
3. Run the API:

```bash
dotnet run --project WordleApi.Host
```

## How to Play

### 1. Start a new game

```http
POST /api/games
Content-Type: application/json

{ "playerName": "kiran" }
```

### 2. Submit guesses (up to 6 attempts)

```http
POST /api/games/{gameId}/guesses
Content-Type: application/json

{ "word": "crane" }
```

Each guess returns letter-by-letter feedback:
- **Correct** — Right letter, right position
- **Present** — Right letter, wrong position
- **Absent** — Letter not in the word

### 3. Check the leaderboard

```http
GET /api/leaderboard?period=week&limit=10
```

### 4. View player stats

```http
GET /api/leaderboard/players/kiran
```

## API Endpoints

| Method | Route | Description |
|--------|-------|-------------|
| `POST` | `/api/games` | Start a new game |
| `GET` | `/api/games/{gameId}` | Get game state |
| `POST` | `/api/games/{gameId}/guesses` | Submit a guess |
| `DELETE` | `/api/games/{gameId}` | Abandon a game |
| `GET` | `/api/leaderboard` | Top scores |
| `GET` | `/api/leaderboard/players/{name}` | Player stats |
| `GET` | `/health` | Health check |

## Scoring

```
score = max(0, 1000 - (attempts - 1) × 150 - floor(seconds / 10))
```

| Attempts | Time | Score |
|----------|------|-------|
| 1 guess, instant | 0s | 1000 |
| 3 guesses | 30s | 697 |
| 6 guesses | 5 min | 220 |

## CI/CD Pipeline

### Build & Test (`build.yml`)

Runs on every push and PR to `main`:
- Restore → Build → Unit Tests → Integration Tests
- Uses a PostgreSQL service container for integration tests
- Uploads test results and published artifacts

### Publish Images (`publish-images.yml`)

Triggered on version tags (`v*`) and manual dispatch:
- Builds a multi-stage Docker image
- Pushes to GitHub Container Registry (GHCR)
- Tags: semver, short SHA, latest

## Project Structure

```
wordle-api/
├── WordleApi.Host/          # ASP.NET Core Web API
│   ├── Controllers/         # API endpoints
│   ├── Data/                # Dapper repositories & migrations
│   ├── Models/              # Entities & DTOs
│   ├── Services/            # Game logic & word evaluation
│   └── Resources/           # Word list
├── WordleApi.Tests/         # Unit tests (xUnit + Moq)
└── WordleApi.IntegrationTests/  # Integration tests (Testcontainers)
```

## Running Tests

```bash
# Unit tests
dotnet test WordleApi.Tests

# Integration tests (requires Docker for Testcontainers)
dotnet test WordleApi.IntegrationTests
```
