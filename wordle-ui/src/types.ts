export type LetterResult = 'Absent' | 'Present' | 'Correct';
export type GameStatus = 'InProgress' | 'Won' | 'Lost';

export interface LetterFeedback {
  letter: string;
  position: number;
  result: LetterResult;
}

export interface GuessHistoryItem {
  attemptNumber: number;
  word: string;
  letters: LetterFeedback[];
  guessedAt: string;
}

export interface GameResponse {
  gameId: string;
  playerName: string;
  status: GameStatus;
  maxAttempts: number;
  attemptsUsed: number;
  guesses: GuessHistoryItem[];
  startedAt: string;
  score: number | null;
  completedAt: string | null;
  secretWord?: string;
}

export interface GuessResult {
  attemptNumber: number;
  word: string;
  letters: LetterFeedback[];
  gameStatus: GameStatus;
  attemptsRemaining: number;
  score: number | null;
  completedAt: string | null;
  secretWord?: string;
}

export interface LeaderboardEntry {
  rank: number;
  playerName: string;
  attempts: number;
  timeTakenSeconds: number;
  score: number;
  completedAt: string;
  word: string;
}

export interface PlayerStats {
  playerName: string;
  gamesPlayed: number;
  gamesWon: number;
  winRate: number;
  averageAttempts: number;
  averageTimeSeconds: number;
  bestScore: number;
  currentStreak: number;
  maxStreak: number;
}
