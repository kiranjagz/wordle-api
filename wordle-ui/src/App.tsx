import { useState, useEffect, useCallback } from 'react';
import Header from './components/Header';
import GameBoard from './components/GameBoard';
import Keyboard from './components/Keyboard';
import Leaderboard from './components/Leaderboard';
import { createGame, submitGuess } from './api';
import type { GameStatus, LetterFeedback } from './types';
import './App.css';

interface GuessRow {
  word: string;
  letters: LetterFeedback[];
}

export default function App() {
  const [view, setView] = useState<'play' | 'leaderboard'>('play');
  const [playerName, setPlayerName] = useState('');
  const [gameId, setGameId] = useState<string | null>(null);
  const [guesses, setGuesses] = useState<GuessRow[]>([]);
  const [currentGuess, setCurrentGuess] = useState('');
  const [gameStatus, setGameStatus] = useState<GameStatus>('InProgress');
  const [secretWord, setSecretWord] = useState<string | null>(null);
  const [score, setScore] = useState<number | null>(null);
  const [error, setError] = useState('');
  const [submitting, setSubmitting] = useState(false);

  const startGame = async () => {
    if (!playerName.trim()) return;
    setError('');
    try {
      const game = await createGame(playerName.trim());
      setGameId(game.gameId);
      setGuesses([]);
      setCurrentGuess('');
      setGameStatus('InProgress');
      setSecretWord(null);
      setScore(null);
    } catch {
      setError('Failed to start game. Is the API running?');
    }
  };

  const handleSubmitGuess = useCallback(async () => {
    if (!gameId || currentGuess.length !== 5 || submitting || gameStatus !== 'InProgress') return;
    setError('');
    setSubmitting(true);
    try {
      const result = await submitGuess(gameId, currentGuess.toLowerCase());
      setGuesses((prev) => [...prev, { word: result.word, letters: result.letters }]);
      setCurrentGuess('');
      setGameStatus(result.gameStatus);
      if (result.gameStatus !== 'InProgress') {
        setScore(result.score ?? null);
        setSecretWord(result.secretWord ?? null);
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Invalid guess');
    } finally {
      setSubmitting(false);
    }
  }, [gameId, currentGuess, submitting, gameStatus]);

  const handleKey = useCallback(
    (key: string) => {
      if (gameStatus !== 'InProgress' || submitting) return;
      if (key === 'ENTER') {
        handleSubmitGuess();
      } else if (key === '⌫') {
        setCurrentGuess((prev) => prev.slice(0, -1));
      } else if (key.length === 1 && /^[A-Z]$/i.test(key) && currentGuess.length < 5) {
        setCurrentGuess((prev) => prev + key.toUpperCase());
      }
    },
    [gameStatus, submitting, currentGuess, handleSubmitGuess]
  );

  useEffect(() => {
    const onKeyDown = (e: KeyboardEvent) => {
      if (e.ctrlKey || e.metaKey || e.altKey) return;
      if (e.key === 'Enter') handleKey('ENTER');
      else if (e.key === 'Backspace') handleKey('⌫');
      else if (/^[a-zA-Z]$/.test(e.key)) handleKey(e.key.toUpperCase());
    };
    window.addEventListener('keydown', onKeyDown);
    return () => window.removeEventListener('keydown', onKeyDown);
  }, [handleKey]);

  const newGame = () => {
    setGameId(null);
    setGuesses([]);
    setCurrentGuess('');
    setGameStatus('InProgress');
    setSecretWord(null);
    setScore(null);
    setError('');
  };

  return (
    <div className="app">
      <Header view={view} onNavigate={setView} />

      {view === 'leaderboard' ? (
        <Leaderboard />
      ) : !gameId ? (
        <div className="start-screen">
          <h2>Start a New Game</h2>
          <div className="name-input">
            <input
              type="text"
              placeholder="Your name"
              value={playerName}
              onChange={(e) => setPlayerName(e.target.value)}
              onKeyDown={(e) => e.key === 'Enter' && startGame()}
              maxLength={20}
            />
            <button onClick={startGame} disabled={!playerName.trim()}>
              Play
            </button>
          </div>
          {error && <p className="error">{error}</p>}
        </div>
      ) : (
        <div className="game-screen">
          <p className="game-info">
            Playing as <strong>{playerName}</strong>
          </p>

          <GameBoard guesses={guesses} currentGuess={currentGuess} maxAttempts={6} />

          {error && <p className="error">{error}</p>}

          {gameStatus === 'InProgress' ? (
            <Keyboard guesses={guesses} onKey={handleKey} />
          ) : (
            <div className="game-over">
              <h2>{gameStatus === 'Won' ? 'You won!' : 'Game over'}</h2>
              <p>
                The word was <strong>{secretWord}</strong>
              </p>
              {score !== null && <p className="score">Score: {score}</p>}
              <div className="game-over-actions">
                <button onClick={newGame}>Play Again</button>
                <button onClick={() => setView('leaderboard')}>View Leaderboard</button>
              </div>
            </div>
          )}
        </div>
      )}
    </div>
  );
}
