import { useEffect, useState } from "react";
import Modal from "./ui/Modal";
import Card from "./ui/Card";
import Tag from "./ui/Tag";
import ProgressBar from "./ui/ProgressBar";
import Button from "./ui/Button";
import { ArrowRightIcon, CloseIcon, CheckIcon } from "./Icons";
import { getQuizDemoQuestions } from "../api/words";
import "../styles/Quiz.css";

const STORAGE_KEY = "vibecheck.quizDemo.state";

function loadSavedState() {
  try {
    const raw = localStorage.getItem(STORAGE_KEY);
    if (!raw) return null;

    const parsed = JSON.parse(raw);

    if (!Array.isArray(parsed.questions) || parsed.questions.length === 0) {
      return null;
    }

    if (typeof parsed.index !== "number" || parsed.index >= parsed.questions.length) {
      return null;
    }

    return parsed;
  } catch {
    return null;
  }
}

function saveState(questions, index, correctCount) {
  try {
    localStorage.setItem(
      STORAGE_KEY,
      JSON.stringify({ questions, index, correctCount })
    );
  } catch {
    // localStorage kan vara avstängt (privat läge m.m.) – strunta i det då.
  }
}

function clearSavedState() {
  try {
    localStorage.removeItem(STORAGE_KEY);
  } catch {
    // se ovan
  }
}

export default function QuizDemo({ onClose, onCreateAccount }) {
  const [savedQuiz] = useState(loadSavedState);
  const [questions, setQuestions] = useState(savedQuiz?.questions ?? []);
  const [loading, setLoading] = useState(!savedQuiz);
  const [error, setError] = useState("");

  const [index, setIndex] = useState(savedQuiz?.index ?? 0);
  const [selectedId, setSelectedId] = useState(null);
  const [answered, setAnswered] = useState(false);
  const [correctCount, setCorrectCount] = useState(
    savedQuiz?.correctCount ?? 0
  );
  const [finished, setFinished] = useState(false);

  // Om inget quiz finns sparat hämtas ett nytt från backend.
  useEffect(() => {
    if (savedQuiz) return;

    let cancelled = false;

    getQuizDemoQuestions(10)
      .then((data) => {
        if (!cancelled) {
          setQuestions(data);
          setLoading(false);
        }
      })
      .catch((err) => {
        if (!cancelled) {
          setError(err.message);
          setLoading(false);
        }
      });

    return () => {
      cancelled = true;
    };
  }, [savedQuiz]);

  // Spara framsteg varje gång man går vidare eller får rätt/fel.
  useEffect(() => {
    if (loading || error || questions.length === 0 || finished) return;
    saveState(questions, index, correctCount);
  }, [questions, index, correctCount, loading, error, finished]);

  function handleSelect(optionId) {
    if (answered) return;
    setSelectedId(optionId);
    setAnswered(true);
    if (optionId === current.correctId) {
      setCorrectCount((c) => c + 1);
    }
  }

  function handleNext() {
    setIndex((i) => i + 1);
    setSelectedId(null);
    setAnswered(false);
  }

  function handleFinish() {
    clearSavedState();
    setFinished(true);
  }

  function handleClose() {
    clearSavedState();
    onClose?.();
  }

  function handleCreateAccount() {
    clearSavedState();
    onClose?.();
    onCreateAccount?.();
  }

  if (loading) {
    return (
      <Modal onClose={handleClose}>
        <p>Laddar quiz...</p>
      </Modal>
    );
  }

  if (error) {
    return (
      <Modal onClose={handleClose}>
        <p>Kunde inte hämta quizet: {error}</p>
      </Modal>
    );
  }

  if (questions.length === 0) {
    return (
      <Modal onClose={handleClose}>
        <p>Det finns inte tillräckligt med ord i databasen för att bygga ett quiz än.</p>
      </Modal>
    );
  }

  if (finished) {
    const percent = Math.round((correctCount / questions.length) * 100);

    let resultEmoji = "💪";
    let resultMessage = "Fortsätt öva!";

    if (percent >= 80) {
      resultEmoji = "🏆";
      resultMessage = "Grymt jobbat!";
    } else if (percent >= 50) {
      resultEmoji = "👍";
      resultMessage = "Bra jobbat!";
    }

    return (
      <Modal onClose={handleClose}>
        <Tag>QUIZ AVKLARAT</Tag>

        <div className="quiz__result">
          <div className="quiz__result-trophy">{resultEmoji}</div>
          <p className="quiz__result-score">
            {correctCount}/{questions.length}
          </p>
          <p className="quiz__result-percent">{percent}% rätt svar</p>
          <p className="quiz__result-message">{resultMessage}</p>

          <div className="quiz__result-actions">
            {onCreateAccount && (
              <Button onClick={handleCreateAccount}>Skapa konto</Button>
            )}
            <button
              type="button"
              className="quiz__result-link"
              onClick={handleClose}
            >
              Till startsidan
            </button>
          </div>
        </div>
      </Modal>
    );
  }

  const current = questions[index];
  const progress = Math.round(((index) / questions.length) * 100);
  const isLastQuestion = index === questions.length - 1;

  return (
    <Modal onClose={handleClose}>
      <Tag>FRÅGA {index + 1} AV {questions.length}</Tag>

      <h2 className="quiz__question">{current.question}</h2>

      <Card className="quiz__quote">
        <p>&quot;{current.quote}&quot;</p>
      </Card>

      <div className="quiz__options">
        {current.options.map((option) => {
          const isSelected = option.id === selectedId;
          const isCorrect = option.id === current.correctId;

          let state = "";
          if (answered && isCorrect) state = "quiz__option--correct";
          else if (answered && isSelected && !isCorrect) state = "quiz__option--incorrect";
          else if (answered) state = "quiz__option--dimmed";

          return (
            <button
              key={option.id}
              type="button"
              className={`quiz__option ${state}`.trim()}
              onClick={() => handleSelect(option.id)}
              disabled={answered}
            >
              <span className="quiz__option-badge">{option.id}</span>
              <span className="quiz__option-text">{option.text}</span>
              {answered && isCorrect && (
                <CheckIcon className="quiz__option-status quiz__option-status--correct" />
              )}
              {answered && isSelected && !isCorrect && (
                <CloseIcon className="quiz__option-status quiz__option-status--incorrect" />
              )}
            </button>
          );
        })}
      </div>

      {answered && (
        <Card className="quiz__explanation">
          <p className="quiz__explanation-title">
            Rätt svar: {current.options.find((o) => o.id === current.correctId).text}
          </p>
          <p className="quiz__explanation-text">{current.explanation}</p>
        </Card>
      )}

      {answered && !isLastQuestion && (
        <Button className="quiz__next" onClick={handleNext}>
          Nästa fråga
          <ArrowRightIcon width={20} height={20} />
        </Button>
      )}

      {answered && isLastQuestion && (
        <Button className="quiz__next" onClick={handleFinish}>
          Se resultat
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
