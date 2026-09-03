import WordCard from "./WordCard";

export default function WordList({ words, onWordClick }) {
  return (
    <div className="word-list">
      {words.map((word, index) => {
        const currentLetter = word.word.charAt(0).toUpperCase();

        const previousLetter =
          index > 0 ? words[index - 1].word.charAt(0).toUpperCase() : "";

        const isFirstWordOfLetter = currentLetter !== previousLetter;

        return (
          <div
            key={word.wordId}
            className={isFirstWordOfLetter ? "word-list__group-start" : ""}
          >
            {isFirstWordOfLetter && (
              <div className="word-list__letter">{currentLetter}</div>
            )}

            <WordCard word={word} onClick={() => onWordClick?.(word)} />
          </div>
        );
      })}
    </div>
  );
}
