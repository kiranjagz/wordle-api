CREATE TABLE IF NOT EXISTS games (
    game_id         UUID PRIMARY KEY,
    player_name     TEXT NOT NULL,
    secret_word     TEXT NOT NULL,
    status          INTEGER NOT NULL DEFAULT 0,
    attempts_used   INTEGER NOT NULL DEFAULT 0,
    score           INTEGER NULL,
    started_at      TIMESTAMPTZ NOT NULL,
    completed_at    TIMESTAMPTZ NULL
);

CREATE INDEX IF NOT EXISTS ix_games_player_name ON games (player_name);
CREATE INDEX IF NOT EXISTS ix_games_status ON games (status);
CREATE INDEX IF NOT EXISTS ix_games_completed_at ON games (completed_at);

CREATE TABLE IF NOT EXISTS guesses (
    guess_id        UUID PRIMARY KEY,
    game_id         UUID NOT NULL REFERENCES games(game_id) ON DELETE CASCADE,
    attempt_number  INTEGER NOT NULL,
    word            TEXT NOT NULL,
    result_json     JSONB NOT NULL,
    guessed_at      TIMESTAMPTZ NOT NULL
);

CREATE INDEX IF NOT EXISTS ix_guesses_game_id ON guesses (game_id);

CREATE MATERIALIZED VIEW IF NOT EXISTS leaderboard_rankings AS
SELECT
    g.game_id,
    g.player_name,
    g.attempts_used,
    EXTRACT(EPOCH FROM (g.completed_at - g.started_at))::INTEGER AS time_taken_seconds,
    g.score,
    g.completed_at,
    g.secret_word
FROM games g
WHERE g.status = 1
ORDER BY g.score DESC, g.completed_at ASC;

CREATE UNIQUE INDEX IF NOT EXISTS ix_leaderboard_rankings_game_id
    ON leaderboard_rankings (game_id);

CREATE TABLE IF NOT EXISTS schema_migrations (
    version     INTEGER PRIMARY KEY,
    name        TEXT NOT NULL,
    applied_at  TIMESTAMPTZ NOT NULL DEFAULT NOW()
);
