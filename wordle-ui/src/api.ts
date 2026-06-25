import type { GameResponse, GuessResult, LeaderboardEntry, PlayerStats } from './types';

const BASE = '/api';

async function request<T>(url: string, options?: RequestInit): Promise<T> {
  const res = await fetch(url, options);
  if (!res.ok) {
    const text = await res.text();
    throw new Error(text || res.statusText);
  }
  return res.json();
}

export function createGame(playerName: string) {
  return request<GameResponse>(`${BASE}/games`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ playerName }),
  });
}

export function getGame(gameId: string) {
  return request<GameResponse>(`${BASE}/games/${gameId}`);
}

export function submitGuess(gameId: string, word: string) {
  return request<GuessResult>(`${BASE}/games/${gameId}/guesses`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ word }),
  });
}

export function getLeaderboard(period = 'all', limit = 10) {
  return request<LeaderboardEntry[]>(`${BASE}/leaderboard?period=${period}&limit=${limit}`);
}

export function getPlayerStats(name: string) {
  return request<PlayerStats>(`${BASE}/leaderboard/players/${encodeURIComponent(name)}`);
}
