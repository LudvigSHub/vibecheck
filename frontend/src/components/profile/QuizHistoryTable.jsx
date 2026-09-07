import Tag from "../ui/Tag";

export default function QuizHistoryTable({ rows }) {
  return (
    <table className="quiz-history-table">
      <thead>
        <tr>
          <th>Quiz</th>
          <th>Tema</th>
          <th>Resultat</th>
          <th>Datum</th>
        </tr>
      </thead>
      <tbody>
        {rows.map((row, index) => (
          <tr key={index}>
            <td>{row.quizName}</td>
            <td>
              <Tag>{row.topic}</Tag>
            </td>
            <td>{row.score}%</td>
            <td>{row.date}</td>
          </tr>
        ))}
      </tbody>
    </table>
  );
}
