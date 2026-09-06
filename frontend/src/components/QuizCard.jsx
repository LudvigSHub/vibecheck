import Card from "./ui/Card";
import Tag from "./ui/Tag";
import Button from "./ui/Button";
import ProgressBar from "./ui/ProgressBar";
import { LockIcon, TargetIcon } from "./Icons";

// Databasen lagrar svårighetsgraderna på engelska. Översättningen hör hemma
// i gränssnittet, inte i API:et – backend ska prata data, inte svenska.
const DIFFICULTY_LABELS = {
  Easy: "Lätt",
  Medium: "Medel",
  Hard: "Svår",
};

export default function QuizCard({ quiz, onStart, starting = false }) {
  const {
    quizName,
    quizDescription,
    difficulty,
    questionCount,
    bestScore,
    isUnlocked,
    unlockedBy,
    requiredScore,
  } = quiz;

  // Notera !== null och inte bara sanningsvärdet: bestScore 0 är ett riktigt
  // resultat, men 0 är falsy. Med if (bestScore) hade noll räknats som
  // "aldrig spelat" – samma skillnad som int? bär i backend.
  const hasResult = bestScore !== null;

  return (
    <Card
      className={`quiz-card ${isUnlocked ? "" : "quiz-card--locked"}`.trim()}
    >
      <div className="quiz-card__top">
        <Tag>{DIFFICULTY_LABELS[difficulty] ?? difficulty}</Tag>

        {!isUnlocked && (
          <LockIcon className="quiz-card__lock" width={20} height={20} />
        )}
      </div>

      <h2 className="quiz-card__title">{quizName}</h2>
      <p className="quiz-card__description">{quizDescription}</p>
      <p className="quiz-card__meta">{questionCount} frågor</p>

      <div className="quiz-card__progress">
        <div className="quiz-card__progress-labels">
          <span>Bästa resultat</span>
          <span className="quiz-card__score">
            {hasResult ? `${bestScore}%` : "–"}
          </span>
        </div>

        <ProgressBar
          value={bestScore ?? 0}
          aria-label={`Bästa resultat på ${quizName}: ${bestScore ?? 0} procent`}
        />
      </div>

      {isUnlocked ? (
        <Button
          className="quiz-card__action"
          onClick={onStart}
          disabled={starting}
        >
          <TargetIcon width={19} height={19} />
          {starting ? "Startar…" : hasResult ? "Gör om quizet" : "Starta quiz"}
        </Button>
      ) : (
        <p className="quiz-card__locked-text">
          Klara <strong>{unlockedBy}</strong> med minst {requiredScore}% för att
          låsa upp.
        </p>
      )}
    </Card>
  );
}
