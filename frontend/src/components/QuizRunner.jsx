import { useState } from "react";

import Modal from "./ui/Modal";
import Card from "./ui/Card";
import Tag from "./ui/Tag";
import Button from "./ui/Button";
import ProgressBar from "./ui/ProgressBar";
import { ArrowRightIcon, CheckIcon, CloseIcon } from "./Icons";
import {
  submitAnswer,
  completeQuizAttempt,
  abandonQuizAttempt,
} from "../api/quiz";

import "../styles/Quiz.css";

// Alternativen har inga bokstäver i databasen – de är rent visuella
// och härleds ur ordningen. Räcker för både 2 och 4 alternativ.
const LETTERS = ["A", "B", "C", "D"];

export default function QuizRunner({ attempt, onClose }) {
  const [index, setIndex] = useState(0);
  const [selectedId, setSelectedId] = useState(null);

  // answer är serverns dom över den aktuella frågan (AnswerResultDTO).
  // null = frågan är obesvarad. Den styr hela rättningsvyn.
  const [answer, setAnswer] = useState(null);

  const [result, setResult] = useState(null);
  const [confirmLeave, setConfirmLeave] = useState(false);
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState("");

  const questions = attempt.questions;
  const current = questions[index];
  const total = questions.length;
  const isLast = index === total - 1;

  async function handleSelect(alternativeId) {
    if (answer || busy) {
      return;
    }

    setSelectedId(alternativeId);
    setBusy(true);
    setError("");

    try {
      const res = await submitAnswer(
        attempt.quizAttemptId,
        current.questionId,
        alternativeId,
      );

      setAnswer(res);
    } catch (err) {
      // Gick rättningen inte igenom ska valet inte se ut som ett svar.
      setSelectedId(null);
      setError(err.message ?? "Kunde inte skicka svaret.");
    } finally {
      setBusy(false);
    }
  }

  function handleNext() {
    setIndex((i) => i + 1);
    setSelectedId(null);
    setAnswer(null);
  }

  async function handleFinish() {
    setBusy(true);
    setError("");

    try {
      const res = await completeQuizAttempt(attempt.quizAttemptId);

      setResult(res);
    } catch (err) {
      setError(err.message ?? "Kunde inte spara resultatet.");
    } finally {
      setBusy(false);
    }
  }

  // Modal anropar den här vid Esc, klick utanför panelen och krysset.
  function handleRequestClose() {
    // Står bekräftelserutan uppe stänger vi den, inte quizet.
    if (confirmLeave) {
      setConfirmLeave(false);
      return;
    }

    // Ett avslutat quiz är redan sparat – inget att varna för.
    if (result) {
      onClose();
      return;
    }

    setConfirmLeave(true);
  }

  async function handleConfirmLeave() {
    setBusy(true);

    try {
      await abandonQuizAttempt(attempt.quizAttemptId);
    } catch {
      // Misslyckas raderingen är det inget användaren kan göra åt. Försöket
      // ligger kvar som påbörjat och städas bort nästa gång quizet startas.
    } finally {
      onClose();
    }
  }

  // ---------- Bekräfta avbrott ----------

  if (confirmLeave) {
    return (
      <Modal onClose={handleRequestClose}>
        <Tag>AVBRYTA?</Tag>

        <h2 className="quiz__question">Vill du avbryta quizet?</h2>

        <p className="quiz__confirm-text">
          Ditt resultat sparas inte, och du börjar om från fråga 1 nästa gång.
        </p>

        <div className="quiz__confirm-actions">
          <Button onClick={handleConfirmLeave} disabled={busy}>
            Ja, avbryt
          </Button>

          <Button variant="ghost" onClick={() => setConfirmLeave(false)}>
            Fortsätt quizet
          </Button>
        </div>
      </Modal>
    );
  }

  // ---------- Resultat ----------

  if (result) {
    let emoji = "💪";
    let message = "Fortsätt öva!";

    if (result.passed) {
      emoji = "🏆";
      message = "Godkänt!";
    } else if (result.score >= 50) {
      emoji = "👍";
      message = "Bra jobbat!";
    }

    return (
      <Modal onClose={handleRequestClose}>
        <Tag>QUIZ AVKLARAT</Tag>

        <div className="quiz__result">
          <div className="quiz__result-trophy">{emoji}</div>

          <p className="quiz__result-score">
            {result.correctCount}/{result.totalCount}
          </p>

          <p className="quiz__result-percent">{result.score}% rätt svar</p>
          <p className="quiz__result-message">{message}</p>

          {result.isNewBest ? (
            <p className="quiz__result-best quiz__result-best--new">
              Nytt personbästa!
            </p>
          ) : (
            <p className="quiz__result-best">
              Ditt bästa är fortfarande {result.previousBestScore}%.
            </p>
          )}

          {result.unlockedQuizName && (
            <Card className="quiz__unlocked">
              <p className="quiz__unlocked-title">Ny nivå upplåst</p>
              <p className="quiz__unlocked-text">{result.unlockedQuizName}</p>
            </Card>
          )}

          <div className="quiz__result-actions">
            <Button onClick={onClose}>Till quizen</Button>
          </div>
        </div>
      </Modal>
    );
  }

  // ---------- Frågan ----------

  // Räknaren kommer från servern när frågan är besvarad, annars från index.
  // Servern vet sanningen om hur många svar som faktiskt ligger sparade.
  const answered = answer ? answer.answeredCount : index;
  const progress = Math.round((answered / total) * 100);

  return (
    <Modal onClose={handleRequestClose}>
      <Tag>
        FRÅGA {index + 1} AV {total}
      </Tag>

      <h2 className="quiz__question">{current.prompt}</h2>

      <Card className="quiz__quote">
        <p>{current.body}</p>
      </Card>

      <div className="quiz__options">
        {current.alternatives.map((option, i) => {
          const isSelected = option.alternativeId === selectedId;
          const isCorrect =
            answer !== null &&
            option.alternativeId === answer.correctAlternativeId;

          let state = "";
          if (answer && isCorrect) state = "quiz__option--correct";
          else if (answer && isSelected) state = "quiz__option--incorrect";
          else if (answer) state = "quiz__option--dimmed";

          return (
            <button
              key={option.alternativeId}
              type="button"
              className={`quiz__option ${state}`.trim()}
              onClick={() => handleSelect(option.alternativeId)}
              disabled={answer !== null || busy}
            >
              <span className="quiz__option-badge">{LETTERS[i]}</span>
              <span className="quiz__option-text">
                {option.alternativeText}
              </span>

              {answer && isCorrect && (
                <CheckIcon className="quiz__option-status quiz__option-status--correct" />
              )}

              {answer && isSelected && !isCorrect && (
                <CloseIcon className="quiz__option-status quiz__option-status--incorrect" />
              )}
            </button>
          );
        })}
      </div>

      {error && (
        <p className="quiz__error" role="alert">
          {error}
        </p>
      )}

      {answer && (
        <Card className="quiz__explanation">
          <p className="quiz__explanation-title">
            Rätt svar: {answer.correctAlternativeText}
          </p>
          <p className="quiz__explanation-text">{answer.explanation}</p>
        </Card>
      )}

      {answer && !isLast && (
        <Button className="quiz__next" onClick={handleNext}>
          Nästa fråga
          <ArrowRightIcon width={20} height={20} />
        </Button>
      )}

      {answer && isLast && (
        <Button className="quiz__next" onClick={handleFinish} disabled={busy}>
          {busy ? "Sparar…" : "Se resultat"}
          <ArrowRightIcon width={20} height={20} />
        </Button>
      )}

      <div className="quiz__progress">
        <ProgressBar value={progress} />
        <div className="quiz__progress-labels">
          <span>{progress}% avklarat</span>
        </div>
      </div>
    </Modal>
  );
}
