import { useState } from "react";
import Ping from "../components/Ping";
import QuizDemo from "../components/QuizDemo";
import Button from "../components/ui/Button";

function TestPage() {
  const [showQuiz, setShowQuiz] = useState(false);

  return (
    <div style={{ padding: 24 }}>
      <h1>Test, ser ni mig?</h1>
      <Ping />

      <div style={{ marginTop: 24 }}>
        <Button onClick={() => setShowQuiz(true)}>Visa quiz-demo</Button>
      </div>

      {showQuiz && <QuizDemo onClose={() => setShowQuiz(false)} />}
    </div>
  );
}

export default TestPage;
