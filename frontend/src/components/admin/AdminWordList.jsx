import { ArrowRightIcon } from "../Icons";
import Card from "../ui/Card";
import LinkButton from "../ui/LinkButton";
import Tag from "../ui/Tag";

function AdminWordList({ words }) {
  if (words.length === 0) {
    return (
      <Card className="admin-word-list__empty">
        <p>Inga ord matchar din sökning.</p>
      </Card>
    );
  }

  return (
    <section className="admin-word-list" aria-label="Ord i ordboken">
      {words.map((word) => (
        <Card className="admin-word-card" key={word.wordId}>
          <div className="admin-word-card__header">
            <div>
              <h2 className="admin-word-card__title">{word.word}</h2>

              <p className="admin-word-card__meaning">
                {word.meaning}
              </p>
            </div>

            <LinkButton
              to={`/admin/words/${word.wordId}`}
              variant="ghost"
              className="admin-word-card__details"
            >
              Detaljer
              <ArrowRightIcon width={17} height={17} />
            </LinkButton>
          </div>

          <div className="admin-word-card__tags">
            {word.tags.map((tag) => (
              <Tag key={tag.tagId}>{tag.tagName}</Tag>
            ))}
          </div>

          <div className="admin-word-card__footer">
            <span>
              {word.exampleCount}{" "}
              {word.exampleCount === 1 ? "exempel" : "exempel"}
            </span>

            {word.isUsedInQuiz && (
              <span className="admin-word-card__quiz-status">
                Används i quiz
              </span>
            )}
          </div>
        </Card>
      ))}
    </section>
  );
}

export default AdminWordList;