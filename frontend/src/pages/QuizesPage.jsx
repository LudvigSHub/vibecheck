import { useCallback, useEffect, useState } from "react";

import QuizCard from "../components/QuizCard";
import QuizRunner from "../components/QuizRunner";
import { getQuizzes, startQuizAttempt } from "../api/quiz";

import "../styles/QuizesPage.css";

function QuizesPage() {
  const [quizzes, setQuizzes] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  // Det pågående försöket (StartQuizAttemptDTO). null = ingen popup öppen.
  const [attempt, setAttempt] = useState(null);

  // Vilket quiz som håller på att startas. Används för att visa "Startar…"
  // på rätt knapp och för att hindra dubbelklick.
  const [startingId, setStartingId] = useState(null);

  // useCallback: funktionen används både i useEffect nedan och när ett quiz
  // stängs. Utan den skapas en ny funktion vid varje rendering, useEffect ser
  // en ändrad dependency och hämtar om – i all oändlighet.
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

  // POST:en ligger här och inte i QuizRunner. Ett anrop som SKAPAR något på
  // servern hör hemma i en händelsehanterare, inte i en useEffect – effekter
  // körs två gånger under StrictMode och hade gett två försök i databasen.
  async function handleStart(quiz) {
    if (startingId !== null) {
      return;
    }

    setStartingId(quiz.quizId);
    setError("");

    try {
      const data = await startQuizAttempt(quiz.quizId);

      setAttempt(data);
    } catch (err) {
      setError(err.message ?? "Kunde inte starta quizet.");
    } finally {
      setStartingId(null);
    }
  }

  function handleCloseRunner() {
    setAttempt(null);

    // Hämtar om listan så att max score och upplåsningar uppdateras.
    // Vid avbrott har inget ändrats, men ett extra GET är billigare
    // än att hålla reda på exakt när det behövs.
    loadQuizzes();
  }

  return (
    <main className="quizzes">
      <header className="quizzes__header">
        <p className="quizzes__eyebrow">Quiz</p>
        <h1 className="quizzes__title">Testa dina kunskaper</h1>
        <p className="quizzes__intro">
          Tre nivåer. Klara en nivå med minst 80% för att låsa upp nästa – ditt
          bästa resultat räknas, så ett sämre försök kan aldrig ta ifrån dig
          något.
        </p>
      </header>

      {loading && <p className="quizzes__status">Hämtar quiz…</p>}

      {error && (
        <p className="quizzes__status quizzes__status--error" role="alert">
          {error}
        </p>
      )}

      {!loading && (
        <section className="quizzes__grid">
          {quizzes.map((quiz) => (
            <QuizCard
              key={quiz.quizId}
              quiz={quiz}
              onStart={() => handleStart(quiz)}
              starting={startingId === quiz.quizId}
            />
          ))}
        </section>
      )}

      {attempt && <QuizRunner attempt={attempt} onClose={handleCloseRunner} />}
    </main>
  );
}

export default QuizesPage;
