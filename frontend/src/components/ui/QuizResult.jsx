import Tag from "./Tag";

export default function QuizResult({ emoji, score, percent, message, children }) {
  return (
    <>
      <Tag>QUIZ AVKLARAT</Tag>

      <div className="quiz-result">
        <div className="quiz-result__trophy">{emoji}</div>
        <p className="quiz-result__score">{score}</p>
        <p className="quiz-result__percent">{percent}% rätt svar</p>
        <p className="quiz-result__message">{message}</p>
        <div className="quiz-result__actions">{children}</div>
      </div>
    </>
  );
}
