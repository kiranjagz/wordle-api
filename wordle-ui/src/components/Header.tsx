interface HeaderProps {
  view: 'play' | 'leaderboard';
  onNavigate: (view: 'play' | 'leaderboard') => void;
}

export default function Header({ view, onNavigate }: HeaderProps) {
  return (
    <header className="header">
      <h1>Wordle</h1>
      <nav>
        <button
          className={view === 'play' ? 'active' : ''}
          onClick={() => onNavigate('play')}
        >
          Play
        </button>
        <button
          className={view === 'leaderboard' ? 'active' : ''}
          onClick={() => onNavigate('leaderboard')}
        >
          Leaderboard
        </button>
      </nav>
    </header>
  );
}
