import WordInflections from "../ui/WordInflections";

export default function WordDetails({ word }) {
  return (
    <div className="word-details">
      <div className="word-details__heading">
        <h2>{word.word}</h2>
        <WordInflections inflections={word.inflections} />
      </div>

      {word.isInappropriate && (
        <span className="word-details__warning">Olämpligt</span>
      )}

      <p>{word.meaning}</p>

      <div className="word-details__examples">
        <h3>Exempel</h3>

        {word.examples.map((example, index) => (
          <p key={index}>"{example}"</p>
        ))}
      </div>

      <div className="word-details__tags">
        {word.tags.map((tag) => (
          <span key={tag.tagId}>{tag.tagName}</span>
        ))}
      </div>
    </div>
  );
}
