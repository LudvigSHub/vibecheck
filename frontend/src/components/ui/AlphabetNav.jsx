const ALPHABET = "ABCDEFGHIJKLMNOPQRSTUVWXYZÅÄÖ".split("");

export default function AlphabetNav({
  availableLetters = [],
  selectedLetter = "",
  onSelectLetter,
}) {
  return (
    <nav className="alphabet-nav" aria-label="Hoppa till bokstav">
      {ALPHABET.map((letter) => {
        const isAvailable = availableLetters.includes(letter);
        const isSelected = selectedLetter === letter;
        const isDisabled = !isAvailable && !isSelected;

        return (
          <button
            key={letter}
            type="button"
            className={`alphabet-nav__letter${
              isSelected ? " alphabet-nav__letter--selected" : ""
            }`}
            disabled={isDisabled}
            onClick={() => onSelectLetter?.(letter)}
            aria-pressed={isSelected}
          >
            {letter}
          </button>
        );
      })}
    </nav>
  );
}
