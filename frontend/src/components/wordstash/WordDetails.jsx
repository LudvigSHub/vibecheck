import { useState } from "react";
import { removeVote, voteWord } from "../../api/words";
import WordInflections from "../ui/WordInflections";

export default function WordDetails({ word, onVoteUpdate }) {
  const [currentVote, setCurrentVote] = useState(word.currentUserVote);

  const [voteCounts, setVoteCounts] = useState({
    upvotes: word.upvotes,
    downvotes: word.downvotes,
  });

  const handleVote = async (isPositive) => {
    try {
      if (currentVote === isPositive) {
        await removeVote(word.wordId);

        setCurrentVote(null);

        const updatedCounts = {
          upvotes: isPositive ? voteCounts.upvotes - 1 : voteCounts.upvotes,
          downvotes: isPositive
            ? voteCounts.downvotes
            : voteCounts.downvotes - 1,
        };

        setVoteCounts(updatedCounts);

        onVoteUpdate({
          ...word,
          ...updatedCounts,
          currentUserVote: null,
        });

        return;
      }

      await voteWord(word.wordId, isPositive);

      const updatedCounts = {
        upvotes: isPositive
          ? voteCounts.upvotes + 1
          : currentVote === true
            ? voteCounts.upvotes - 1
            : voteCounts.upvotes,

        downvotes: !isPositive
          ? voteCounts.downvotes + 1
          : currentVote === false
            ? voteCounts.downvotes - 1
            : voteCounts.downvotes,
      };

      setVoteCounts(updatedCounts);
      setCurrentVote(isPositive);

      onVoteUpdate({
        ...word,
        ...updatedCounts,
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

      <div className="word-details__votes">
        <button
          type="button"
          className={`word-details__vote ${
            currentVote === true ? "word-details__vote--active" : ""
          }`}
          onClick={() => handleVote(true)}
        >
          <svg
            className="word-details__vote-icon"
            viewBox="0 0 24 24"
            aria-hidden="true"
          >
            <path d="M7 10v12M3 10h4v12H3V10Zm4 0 4-8a3 3 0 0 1 3 3v2.5a2.5 2.5 0 0 0 2.5 2.5H20a2 2 0 0 1 1.9 2.6l-2.3 7A2 2 0 0 1 17.7 21H7" />
          </svg>

          <span>{voteCounts.upvotes}</span>
        </button>

        <button
          type="button"
          className={`word-details__vote ${
            currentVote === false ? "word-details__vote--active" : ""
          }`}
          onClick={() => handleVote(false)}
        >
          <svg
            className="word-details__vote-icon word-details__vote-icon--down"
            viewBox="0 0 24 24"
            aria-hidden="true"
          >
            <path d="M7 10v12M3 10h4v12H3V10Zm4 0 4-8a3 3 0 0 1 3 3v2.5a2.5 2.5 0 0 0 2.5 2.5H20a2 2 0 0 1 1.9 2.6l-2.3 7A2 2 0 0 1 17.7 21H7" />
          </svg>

          <span>{voteCounts.downvotes}</span>
        </button>
      </div>
    </div>
  );
}
