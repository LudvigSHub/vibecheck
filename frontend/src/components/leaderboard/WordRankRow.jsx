export default function WordRankRow({ rank, word, votes, direction }) {
  const iconClass =
    direction === "down"
      ? "word-rank-row__icon word-rank-row__icon--down"
      : "word-rank-row__icon";

  return (
    <div className="word-rank-row">
      <span className="word-rank-row__rank">{rank}</span>
      <span className="word-rank-row__word">{word}</span>
      <span className={`word-rank-row__votes word-rank-row__votes--${direction}`}>
        <svg className={iconClass} viewBox="0 0 24 24" aria-hidden="true">
          <path d="M7 10v12M3 10h4v12H3V10Zm4 0 4-8a3 3 0 0 1 3 3v2.5a2.5 2.5 0 0 0 2.5 2.5H20a2 2 0 0 1 1.9 2.6l-2.3 7A2 2 0 0 1 17.7 21H7" />
        </svg>
        {votes}
      </span>
    </div>
  );
}
