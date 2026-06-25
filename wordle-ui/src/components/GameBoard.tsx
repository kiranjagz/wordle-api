import type { LetterFeedback, LetterResult } from '../types';

interface GameBoardProps {
  guesses: { word: string; letters: LetterFeedback[] }[];
  currentGuess: string;
  maxAttempts: number;
}

function tileClass(result: LetterResult): string {
  switch (result) {
    case 'Correct': return 'tile correct';
    case 'Present': return 'tile present';
    case 'Absent': return 'tile absent';
  }
}

export default function GameBoard({ guesses, currentGuess, maxAttempts }: GameBoardProps) {
  const rows = [];

  for (let i = 0; i < maxAttempts; i++) {
    if (i < guesses.length) {
      // Submitted guess row
      rows.push(
        <div className="row" key={i}>
          {guesses[i].letters.map((l, j) => (
            <div className={tileClass(l.result)} key={j}>
              {l.letter}
            </div>
          ))}
        </div>
      );
    } else if (i === guesses.length) {
      // Current input row
      const letters = currentGuess.padEnd(5, ' ').split('');
      rows.push(
        <div className="row" key={i}>
          {letters.map((ch, j) => (
            <div className={`tile${ch !== ' ' ? ' filled' : ''}`} key={j}>
              {ch === ' ' ? '' : ch}
            </div>
          ))}
        </div>
      );
    } else {
      // Empty row
      rows.push(
        <div className="row" key={i}>
          {Array.from({ length: 5 }).map((_, j) => (
            <div className="tile" key={j} />
          ))}
        </div>
      );
    }
  }

  return <div className="board">{rows}</div>;
}
