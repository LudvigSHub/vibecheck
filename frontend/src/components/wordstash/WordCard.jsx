import { removeVote, voteWord } from "../../api/words";

export default function WordCard({ word, onClick, onVoteUpdate }) {
  const handleVote = async (isPositive) => {
    try {
      if (word.currentUserVote === isPositive) {
        await removeVote(word.wordId);

        onVoteUpdate({
          ...word,
          upvotes: isPositive ? word.upvotes - 1 : word.upvotes,
          downvotes: isPositive ? word.downvotes : word.downvotes - 1,
          currentUserVote: null,
        });

        return;
      }

      await voteWord(word.wordId, isPositive);

      onVoteUpdate({
        ...word,
        upvotes: isPositive
          ? word.upvotes + 1
          : word.currentUserVote === true
            ? word.upvotes - 1
            : word.upvotes,

        downvotes: !isPositive
          ? word.downvotes + 1
          : word.currentUserVote === false
            ? word.downvotes - 1
            : word.downvotes,

        currentUserVote: isPositive,
      });
    } catch (error) {
      if (error.status === 401) {
        window.dispatchEvent(new Event("auth:login-required"));
        return;
      }

      console.error("Kunde inte spara röst:", error);
    }
  };

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

      <div className="word-card__votes">
        <button
          type="button"
          className={`word-card__vote ${
            word.currentUserVote === true ? "word-card__vote--active" : ""
          }`}
          onClick={(event) => {
            event.stopPropagation();
            handleVote(true);
          }}
        >
          <svg
            className="word-card__vote-icon"
            viewBox="0 0 24 24"
            aria-hidden="true"
          >
            <path d="M7 10v12M3 10h4v12H3V10Zm4 0 4-8a3 3 0 0 1 3 3v2.5a2.5 2.5 0 0 0 2.5 2.5H20a2 2 0 0 1 1.9 2.6l-2.3 7A2 2 0 0 1 17.7 21H7" />
          </svg>

          <span>{word.upvotes}</span>
        </button>

        <button
          type="button"
          className={`word-card__vote ${
            word.currentUserVote === false ? "word-card__vote--active" : ""
          }`}
          onClick={(event) => {
            event.stopPropagation();
            handleVote(false);
          }}
        >
          <svg
            className="word-card__vote-icon word-card__vote-icon--down"
            viewBox="0 0 24 24"
            aria-hidden="true"
          >
            <path d="M7 10v12M3 10h4v12H3V10Zm4 0 4-8a3 3 0 0 1 3 3v2.5a2.5 2.5 0 0 0 2.5 2.5H20a2 2 0 0 1 1.9 2.6l-2.3 7A2 2 0 0 1 17.7 21H7" />
          </svg>

          <span>{word.downvotes}</span>
        </button>
      </div>
    </div>
  );
}
