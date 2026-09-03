import { useState } from "react";
import Modal from "./ui/Modal";
import Card from "./ui/Card";
import Tag from "./ui/Tag";
import ProgressBar from "./ui/ProgressBar";
import Button from "./ui/Button";
import { ArrowRightIcon, CloseIcon, CheckIcon } from "./Icons";
import "../styles/QuizDemo.css";

const QUESTIONS = [
  {
    question: "Vilket slangord passar in i följande mening?",
    quote: "Det var så ___ på festen igår!",
    options: [
      { id: "A", text: "Lit" },
      { id: "B", text: "Mid" },
      { id: "C", text: "Sus" },
      { id: "D", text: "Slay" },
    ],
    correctId: "A",
    explanation:
      "\"Lit\" betyder något som är riktigt bra, spännande, eller har en vild stämning (som en grym fest).",
  },
  {
    question: "Vilket slangord passar in i följande mening?",
    quote: "Han är helt ___, litar inte på honom.",
    options: [
      { id: "A", text: "Cap" },
      { id: "B", text: "Cooked" },
      { id: "C", text: "Sus" },
      { id: "D", text: "Aura" },
    ],
    correctId: "C",
    explanation: "\"Sus\" (av \"suspicious\") betyder misstänksam.",
  },
];

export default function QuizDemo({ onClose }) {
  const [index, setIndex] = useState(0);
  const [selectedId, setSelectedId] = useState(null);
  const [answered, setAnswered] = useState(false);

  const current = QUESTIONS[index];
  const progress = Math.round(((index + 1) / QUESTIONS.length) * 100);

  function handleSelect(optionId) {
    if (answered) return;
    setSelectedId(optionId);
    setAnswered(true);
  }

  function handleNext() {
    setIndex((i) => i + 1);
    setSelectedId(null);
    setAnswered(false);
  }

    return (
    <Modal onClose={onClose}>
      <Tag>FRÅGA {index + 1} AV {QUESTIONS.length}</Tag>

      <h2 className="quiz-demo__question">{current.question}</h2>

      <Card className="quiz-demo__quote">
        <p>&quot;{current.quote}&quot;</p>
      </Card>

      <div className="quiz-demo__options">
        {current.options.map((option) => {
          const isSelected = option.id === selectedId;
          const isCorrect = option.id === current.correctId;

          let state = "";
          if (answered && isCorrect) state = "quiz-demo__option--correct";
          else if (answered && isSelected && !isCorrect) state = "quiz-demo__option--incorrect";
          else if (answered) state = "quiz-demo__option--dimmed";

          return (
            <button
              key={option.id}
              type="button"
              className={`quiz-demo__option ${state}`.trim()}
              onClick={() => handleSelect(option.id)}
              disabled={answered}
            >
              <span className="quiz-demo__option-badge">{option.id}</span>
              <span className="quiz-demo__option-text">{option.text}</span>
              {answered && isCorrect && (
                <CheckIcon className="quiz-demo__option-status quiz-demo__option-status--correct" />
              )}
              {answered && isSelected && !isCorrect && (
                <CloseIcon className="quiz-demo__option-status quiz-demo__option-status--incorrect" />
              )}
            </button>
          );
        })}
      </div>

      {answered && (
        <Card className="quiz-demo__explanation">
          <p className="quiz-demo__explanation-title">
            Rätt svar: {current.options.find((o) => o.id === current.correctId).text}
          </p>
          <p className="quiz-demo__explanation-text">{current.explanation}</p>
        </Card>
      )}

      {answered && index < QUESTIONS.length - 1 && (
        <Button onClick={handleNext}>
          Nästa fråga
          <ArrowRightIcon width={20} height={20} />
        </Button>
      )}

      {answered && index === QUESTIONS.length - 1 && (
        <Button onClick={onClose}>Klar</Button>
      )}

      <div className="quiz-demo__progress">
        <ProgressBar value={progress} />
        <div className="quiz-demo__progress-labels">
          <span>{progress}% avklarat</span>
          <span>{index + 1} / {QUESTIONS.length}</span>
        </div>
      </div>
    </Modal>
  );


}
