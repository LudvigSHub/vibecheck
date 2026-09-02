const ALPHABET = "ABCDEFGHIJKLMNOPQRSTUVWXYZÅÄÖ".split("");

export default function AlphabetNav({ activeLetters = [], onSelectLetter }) {
  return (
    <nav className="alphabet-nav" aria-label="Hoppa till bokstav">
      {ALPHABET.map((letter) => {
        const isActive = activeLetters.includes(letter);
        return (
          <button
            key={letter}
            type="button"
            className={`alphabet-nav__letter${isActive ? " alphabet-nav__letter--active" : ""}`}
            disabled={!isActive}
            onClick={() => onSelectLetter?.(letter)}
          >
            {letter}
          </button>
        );
      })}
    </nav>
  );
}