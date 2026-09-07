export default function WordCard({ word, onClick }) {
  return (
    <div className="word-card" onClick={onClick} role="button" tabIndex={0}>
      <div className="word-card__header">
        <h2 className="word-card__word">{word.word}</h2>

        {word.isInappropriate && (
          <span className="word-card__warning">Olämpligt</span>
        )}
      </div>

      <p className="word-card__meaning">{word.meaning}</p>

      <div className="word-card__tags">
        {word.tags.map((tag) => (
          <span className="word-card__tag" key={tag.tagId}>
            {tag.tagName}
          </span>
        ))}
      </div>
    </div>
  );
}
