import { useEffect, useState } from "react";

import QuizCard from "../components/QuizCard";
import QuizRunner from "../components/QuizRunner";
import RankedQuizRunner from "../components/RankedQuizRunner";
import Card from "../components/ui/Card";
import Button from "../components/ui/Button";
import { TargetIcon } from "../components/Icons";
import { getQuizzes, startQuizAttempt } from "../api/quiz";
import { startRankedAttempt } from "../api/ranked";

import "../styles/QuizesPage.css";

function QuizesPage() {
  const [quizzes, setQuizzes] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  // Det pågående försöket (StartQuizAttemptDTO). null = ingen popup öppen.
  const [attempt, setAttempt] = useState(null);

  // Motsvarande för det rankade quizet (StartRankedAttemptDTO).
  const [rankedAttempt, setRankedAttempt] = useState(null);

  // Vilket quiz som håller på att startas. Används för att visa "Startar…"
  // på rätt knapp och för att hindra dubbelklick.
  const [startingId, setStartingId] = useState(null);
  const [startingRanked, setStartingRanked] = useState(false);

  async function refreshQuizzes() {
    try {
      const data = await getQuizzes();

      setQuizzes(data);
      setError("");
    } catch (err) {
      setError(err.message ?? "Kunde inte hämta quizen.");
    }
  }

  useEffect(() => {
    const controller = new AbortController();

    getQuizzes({ signal: controller.signal })
      .then((data) => {
        setQuizzes(data);
        setError("");
      })
      .catch((err) => {
        // Avbryts anropet när komponenten lämnas är det inte ett fel.
        if (err.name !== "AbortError") {
          setError(err.message ?? "Kunde inte hämta quizen.");
        }
      })
      .finally(() => {
        if (!controller.signal.aborted) {
          setLoading(false);
        }
      });

    return () => controller.abort();
  }, []);

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

  async function handleStartRanked() {
    if (startingRanked) {
      return;
    }

    setStartingRanked(true);
    setError("");

    try {
      const data = await startRankedAttempt();

      setRankedAttempt(data);
    } catch (err) {
      setError(err.message ?? "Kunde inte starta det rankade quizet.");
    } finally {
      setStartingRanked(false);
    }
  }

  function handleCloseRunner() {
    setAttempt(null);

    // Hämtar om listan så att max score och upplåsningar uppdateras.
    // Vid avbrott har inget ändrats, men ett extra GET är billigare
    // än att hålla reda på exakt när det behövs.
    refreshQuizzes();
  }

  return (
    <main className="quizzes">
      <header className="quizzes__header">
        <p className="quizzes__eyebrow">Quiz</p>
        <h1 className="quizzes__title">Testa dina kunskaper</h1>
        <p className="quizzes__intro">
          Tre nivåer att spela om och om igen. Varje omgång slumpas upp till 10
          frågor – så du kan få olika frågor även när du spelar samma nivå.
        </p>
        <p className="quizzes__intro">
          Klara en nivå med minst 80% rätt för att låsa upp nästa. Ditt bästa
          resultat räknas, och upplåsta nivåer förblir öppna.
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

      <Card className="quizzes__ranked">
        <div className="quizzes__ranked-text">
          <p className="quizzes__ranked-label">Rankat quiz</p>
          <h2 className="quizzes__ranked-title">En minut. Tre fel.</h2>
          <p className="quizzes__ranked-intro">
            Frågor från alla nivåer, ett poäng per rätt svar. Omgången tar slut
            när minuten är ute eller vid tredje felet. Ditt bästa resultat
            hamnar på veckans topplista.
          </p>
        </div>

        <Button
          className="quizzes__ranked-action"
          onClick={handleStartRanked}
          disabled={startingRanked}
        >
          <TargetIcon width={19} height={19} />
          {startingRanked ? "Startar…" : "Spela rankat"}
        </Button>
      </Card>

      {attempt && <QuizRunner attempt={attempt} onClose={handleCloseRunner} />}

      {rankedAttempt && (
        <RankedQuizRunner
          attempt={rankedAttempt}
          onClose={() => setRankedAttempt(null)}
        />
      )}
    </main>
  );
}

export default QuizesPage;
