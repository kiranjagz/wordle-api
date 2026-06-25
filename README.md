# Wordle API

A Wordle-like word guessing game with a React frontend and REST API backend, featuring a PostgreSQL-backed leaderboard. Built to showcase GitHub Actions CI/CD and Docker-ready deployments.

## Tech Stack

- **React + TypeScript** — Frontend (Vite)
- **.NET 10** — ASP.NET Core Web API
- **PostgreSQL** — Data storage
- **Dapper** — Micro-ORM
- **Docker** — Multi-stage container builds
- **GitHub Actions** — CI/CD pipeline with GHCR publishing

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js 22+](https://nodejs.org/) (for the frontend)
- [Docker](https://docs.docker.com/get-docker/) (for containerized runs)
- PostgreSQL (local or via Docker)

### Run with Docker Compose (recommended)

```bash
docker compose -f docker-compose.yml -f docker-compose.postgres.yml up --build
```

- **UI** — `http://localhost:3000`
- **API / Swagger** — `http://localhost:5050/swagger`

### Run locally (development)

1. Start a PostgreSQL instance (or use `docker compose -f docker-compose.postgres.yml up postgres`)
2. Start the API:

```bash
dotnet run --project WordleApi.Host
```

3. Start the UI (in a separate terminal):

```bash
cd wordle-ui
npm install
npm run dev
```

The UI will be available at `http://localhost:5173` and proxies API requests to the backend.

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

## Postman Collection

Import the [Postman collection](docs/WordleApi.postman_collection.json) to get all endpoints pre-configured with variables and test scripts. The collection auto-saves the `gameId` after creating a game, so you can run requests in sequence.

You can also access the collection in the [Postman workspace](https://www.postman.com/).

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

## Documentation

- [Architecture](docs/architecture.md) — System overview, game flow, guess evaluation algorithm, database schema, CI/CD pipeline
- [Postman Collection](docs/WordleApi.postman_collection.json) — Import into Postman for ready-to-use API requests

## Project Structure

```
wordle-api/
├── wordle-ui/                   # React frontend (Vite + TypeScript)
│   ├── src/components/          # GameBoard, Keyboard, Leaderboard, Header
│   └── Dockerfile               # Nginx-based production image
├── docs/                        # Architecture diagrams & Postman collection
├── WordleApi.Host/              # ASP.NET Core Web API
│   ├── Controllers/             # API endpoints
│   ├── Data/                    # Dapper repositories & migrations
│   ├── Models/                  # Entities & DTOs
│   ├── Services/                # Game logic & word evaluation
│   └── Resources/               # Word list
├── WordleApi.Tests/             # Unit tests (xUnit + Moq)
└── WordleApi.IntegrationTests/  # Integration tests (Testcontainers)
```

## Running Tests

```bash
# Unit tests
dotnet test WordleApi.Tests

# Integration tests (requires Docker for Testcontainers)
dotnet test WordleApi.IntegrationTests
```
