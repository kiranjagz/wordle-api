import { useEffect, useState } from 'react';
import { getLeaderboard, getPlayerStats } from '../api';
import type { LeaderboardEntry, PlayerStats } from '../types';

export default function Leaderboard() {
  const [entries, setEntries] = useState<LeaderboardEntry[]>([]);
  const [playerName, setPlayerName] = useState('');
  const [stats, setStats] = useState<PlayerStats | null>(null);
  const [statsError, setStatsError] = useState('');
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    getLeaderboard()
      .then(setEntries)
      .catch(() => {})
      .finally(() => setLoading(false));
  }, []);

  const lookupPlayer = async () => {
    if (!playerName.trim()) return;
    setStatsError('');
    setStats(null);
    try {
      const s = await getPlayerStats(playerName.trim());
      setStats(s);
    } catch {
      setStatsError('Player not found');
    }
  };

  return (
    <div className="leaderboard">
      <h2>Top Scores</h2>
      {loading ? (
        <p>Loading...</p>
      ) : entries.length === 0 ? (
        <p>No scores yet. Play a game!</p>
      ) : (
        <table>
          <thead>
            <tr>
              <th>#</th>
              <th>Player</th>
              <th>Word</th>
              <th>Attempts</th>
              <th>Time</th>
              <th>Score</th>
            </tr>
          </thead>
          <tbody>
            {entries.map((e) => (
              <tr key={`${e.rank}-${e.playerName}`}>
                <td>{e.rank}</td>
                <td>{e.playerName}</td>
                <td>{e.word}</td>
                <td>{e.attempts}/6</td>
                <td>{e.timeTakenSeconds}s</td>
                <td>{e.score}</td>
              </tr>
            ))}
          </tbody>
        </table>
      )}

      <h2>Player Stats</h2>
      <div className="stats-lookup">
        <input
          type="text"
          placeholder="Enter player name"
          value={playerName}
          onChange={(e) => setPlayerName(e.target.value)}
          onKeyDown={(e) => e.key === 'Enter' && lookupPlayer()}
        />
        <button onClick={lookupPlayer}>Look up</button>
      </div>

      {statsError && <p className="error">{statsError}</p>}

      {stats && (
        <div className="player-stats">
          <h3>{stats.playerName}</h3>
          <div className="stats-grid">
            <div className="stat">
              <span className="stat-value">{stats.gamesPlayed}</span>
              <span className="stat-label">Played</span>
            </div>
            <div className="stat">
              <span className="stat-value">{Math.round(stats.winRate * 100)}%</span>
              <span className="stat-label">Win Rate</span>
            </div>
            <div className="stat">
              <span className="stat-value">{stats.currentStreak}</span>
              <span className="stat-label">Streak</span>
            </div>
            <div className="stat">
              <span className="stat-value">{stats.maxStreak}</span>
              <span className="stat-label">Max Streak</span>
            </div>
            <div className="stat">
              <span className="stat-value">{stats.bestScore}</span>
              <span className="stat-label">Best Score</span>
            </div>
            <div className="stat">
              <span className="stat-value">{stats.averageAttempts.toFixed(1)}</span>
              <span className="stat-label">Avg Attempts</span>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
