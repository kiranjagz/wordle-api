import type { LetterFeedback, LetterResult } from '../types';

interface KeyboardProps {
  guesses: { letters: LetterFeedback[] }[];
  onKey: (key: string) => void;
}

const ROWS = [
  ['Q', 'W', 'E', 'R', 'T', 'Y', 'U', 'I', 'O', 'P'],
  ['A', 'S', 'D', 'F', 'G', 'H', 'J', 'K', 'L'],
  ['ENTER', 'Z', 'X', 'C', 'V', 'B', 'N', 'M', '⌫'],
];

function buildLetterMap(guesses: { letters: LetterFeedback[] }[]): Map<string, LetterResult> {
  const map = new Map<string, LetterResult>();
  for (const g of guesses) {
    for (const l of g.letters) {
      const key = l.letter.toUpperCase();
      const existing = map.get(key);
      if (existing === 'Correct') continue;
      if (existing === 'Present' && l.result !== 'Correct') continue;
      map.set(key, l.result);
    }
  }
  return map;
}

export default function Keyboard({ guesses, onKey }: KeyboardProps) {
  const letterMap = buildLetterMap(guesses);

  return (
    <div className="keyboard">
      {ROWS.map((row, i) => (
        <div className="keyboard-row" key={i}>
          {row.map((key) => {
            const result = letterMap.get(key);
            let cls = 'key';
            if (result === 'Correct') cls += ' correct';
            else if (result === 'Present') cls += ' present';
            else if (result === 'Absent') cls += ' absent';
            if (key === 'ENTER' || key === '⌫') cls += ' wide';

            return (
              <button className={cls} key={key} onClick={() => onKey(key)}>
                {key}
              </button>
            );
          })}
        </div>
      ))}
    </div>
  );
}
