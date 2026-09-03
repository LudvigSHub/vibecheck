import { useCallback, useEffect, useState } from "react";

import QuizCard from "../components/QuizCard";
import { getQuizzes } from "../api/quiz";

import "../styles/QuizesPage.css";

function QuizesPage() {
  const [quizzes, setQuizzes] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  // Quizet som körs just nu. null = ingen popup öppen. Används i steg B.
  const [activeQuiz, setActiveQuiz] = useState(null);

  // useCallback: funktionen behövs både i useEffect nedan och senare när ett
  // quiz avslutats och listan ska uppdateras. Utan den skapas en ny funktion
  // vid varje rendering, useEffect ser en ändrad dependency och hämtar om –
  // i all oändlighet.
  const loadQuizzes = useCallback(async (signal) => {
    try {
      const data = await getQuizzes({ signal });

      setQuizzes(data);
      setError("");
    } catch (err) {
      // Avbryts anropet när komponenten lämnas är det inte ett fel.
      if (err.name === "AbortError") {
        return;
      }

      setError(err.message ?? "Kunde inte hämta quizen.");
    } finally {
      if (!signal?.aborted) {
        setLoading(false);
      }
    }
  }, []);

  useEffect(() => {
    const controller = new AbortController();

    loadQuizzes(controller.signal);

    return () => controller.abort();
  }, [loadQuizzes]);

  return (
    <main className="quizzes">
      <header className="quizzes__header">
        <p className="quizzes__eyebrow">Quiz</p>
        <h1 className="quizzes__title">Testa dina kunskaper</h1>
        <p className="quizzes__intro">
          Tre nivåer. Klara en nivå med minst 80% för att låsa upp nästa. 
          <br/>
          Ditt bästa resultat räknas, så ett sämre försök kan aldrig ta ifrån dig
          något.
        </p>
      </header>

      {loading && <p className="quizzes__status">Hämtar quiz…</p>}

      {error && (
        <p className="quizzes__status quizzes__status--error" role="alert">
          {error}
        </p>
      )}

      {!loading && !error && (
        <section className="quizzes__grid">
          {quizzes.map((quiz) => (
            <QuizCard
              key={quiz.quizId}
              quiz={quiz}
              onStart={() => setActiveQuiz(quiz)}
            />
          ))}
        </section>
      )}

      {/* Steg B: här renderas QuizRunner när activeQuiz är satt. */}
    </main>
  );
}

export default QuizesPage;
