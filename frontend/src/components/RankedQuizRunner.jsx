import { useCallback, useEffect, useRef, useState } from "react";

import Modal from "./ui/Modal";
import Card from "./ui/Card";
import Tag from "./ui/Tag";
import Button from "./ui/Button";
import ConversationQuestion from "./ConversationQuestion";
import { CheckIcon, CloseIcon } from "./Icons";
import { submitRankedAnswer, completeRankedAttempt } from "../api/ranked";

import "../styles/Quiz.css";

const LETTERS = ["A", "B", "C", "D"];

// Hur länge rätt/fel visas innan nästa fråga. Lång nog att hinna se domen,
// kort nog att inte kosta tid i en tävling på klockan.
const FEEDBACK_MS = 700;

const END_REASONS = {
  TimeUp: "Tiden tog slut",
  ThreeWrong: "Tre fel – omgången avbröts",
  OutOfQuestions: "Du hann med alla frågor",
};

export default function RankedQuizRunner({ attempt, onClose }) {
  const questions = attempt.questions;

  const [index, setIndex] = useState(0);
  const [selectedId, setSelectedId] = useState(null);

  // Serverns dom över den aktuella frågan. null = obesvarad.
  const [feedback, setFeedback] = useState(null);

  // Löpande ställning. Kommer från servern vid varje svar – feedback
  // nollställs när vi går vidare, så räkningen behöver bo för sig.
  const [counts, setCounts] = useState({ correct: 0, wrong: 0 });

  const [result, setResult] = useState(null);
  const [confirmLeave, setConfirmLeave] = useState(false);
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState("");

  const [secondsLeft, setSecondsLeft] = useState(attempt.secondsRemaining);

  // Deadline sätts en gång vid montering, och nedräkningen räknar mot den
  // i stället för att dra ett från en räknare vid varje tick. setInterval
  // bromsas i bakgrundsflikar – en räknare hade då gått för långsamt och
  // gett spelaren mer tid än en minut.
  const deadlineRef = useRef(Date.now() + attempt.secondsRemaining * 1000);

  // Avslutet får bara skickas en gång. Timern och ett sista svar kan
  // annars utlösa det samtidigt.
  const finishingRef = useRef(false);

  const current = questions[index];

  const finishRound = useCallback(async () => {
    if (finishingRef.current) {
      return;
    }

    finishingRef.current = true;
    setBusy(true);

    try {
      const res = await completeRankedAttempt(attempt.rankedAttemptId);

      setResult(res);
    } catch (err) {
      setError(err.message ?? "Kunde inte spara resultatet.");
      finishingRef.current = false;
    } finally {
      setBusy(false);
    }
  }, [attempt.rankedAttemptId]);

  // Nedräkningen. Beror bara på result, inte på confirmLeave – klockan ska
  // fortsätta ticka medan avbrytsrutan står uppe. Att pausa den hade gjort
  // rutan till en gratis paus mitt i en tävling på tid.
  useEffect(() => {
    if (result) {
      return;
    }

    const id = setInterval(() => {
      const remaining = Math.max(
        0,
        Math.ceil((deadlineRef.current - Date.now()) / 1000),
      );

      setSecondsLeft(remaining);
    }, 250);

    return () => clearInterval(id);
  }, [result]);

  // Tiden ute – avsluta. Servern gör samma bedömning oberoende av oss.
  useEffect(() => {
    if (secondsLeft > 0 || result) {
      return;
    }

    finishRound();
  }, [secondsLeft, result, finishRound]);

  // Visa domen en kort stund, gå sedan vidare automatiskt.
  useEffect(() => {
    if (!feedback) {
      return;
    }

    const id = setTimeout(() => {
      if (feedback.roundOver) {
        finishRound();
        return;
      }

      setIndex((i) => i + 1);
      setSelectedId(null);
      setFeedback(null);
    }, FEEDBACK_MS);

    return () => clearTimeout(id);
  }, [feedback, finishRound]);

  async function handleSelect(alternativeId) {
    if (feedback || busy || result) {
      return;
    }

    setSelectedId(alternativeId);
    setBusy(true);
    setError("");

    try {
      const res = await submitRankedAnswer(
        attempt.rankedAttemptId,
        current.questionId,
        alternativeId,
      );

      setCounts({ correct: res.correctCount, wrong: res.wrongCount });
      setFeedback(res);
    } catch (err) {
      setSelectedId(null);
      setError(err.message ?? "Kunde inte skicka svaret.");
    } finally {
      setBusy(false);
    }
  }

  // Modal anropar den här vid Esc, klick utanför panelen och krysset.
  function handleRequestClose() {
    // Resultatet först: hinner tiden ta slut medan avbrytsrutan står uppe
    // visas resultatet, och då ska krysset stänga på riktigt.
    if (result) {
      onClose();
      return;
    }

    if (confirmLeave) {
      setConfirmLeave(false);
      return;
    }

    setConfirmLeave(true);
  }

  const minutes = Math.floor(secondsLeft / 60);
  const seconds = secondsLeft % 60;
  const timeLabel = `${minutes}:${String(seconds).padStart(2, "0")}`;

  const timer = (
    <span
      className={`quiz__timer${secondsLeft <= 10 ? " quiz__timer--urgent" : ""}`}
      aria-label={`${secondsLeft} sekunder kvar`}
    >
      {timeLabel}
    </span>
  );

  // ---------- Resultat ----------

  if (result) {
    return (
      <Modal onClose={handleRequestClose}>
        <Tag>RANKAT QUIZ</Tag>

        <div className="quiz__result">
          <p className="quiz__result-score">{result.score}</p>
          <p className="quiz__result-percent">
            poäng av {result.answeredCount} besvarade
          </p>

          <p className="quiz__result-message">
            {END_REASONS[result.endReason] ?? result.endReason}
          </p>

          {result.isNewPersonalBest ? (
            <p className="quiz__result-best quiz__result-best--new">
              Nytt veckobästa!
            </p>
          ) : (
            <p className="quiz__result-best">
              Ditt bästa den här veckan är fortfarande{" "}
              {result.previousBestScore}.
            </p>
          )}

          <Card className="quiz__unlocked">
            <p className="quiz__unlocked-title">Placering denna vecka</p>
            <p className="quiz__unlocked-text">
              {result.rank} av {result.totalPlayers}
            </p>
          </Card>

          <div className="quiz__result-actions">
            <Button onClick={onClose}>Stäng</Button>
          </div>
        </div>
      </Modal>
    );
  }

  // ---------- Avbryta? ----------

  if (confirmLeave) {
    return (
      <Modal onClose={handleRequestClose}>
        <div className="quiz__ranked-bar">
          {timer}
          <span className="quiz__ranked-score">{counts.correct} rätt</span>
        </div>

        <h2 className="quiz__question">Avbryta omgången?</h2>

        <p className="quiz__confirm-text">
          Klockan fortsätter att ticka medan du bestämmer dig. Avbryter du
          sparas inget resultat och inget hamnar på topplistan.
        </p>

        <div className="quiz__confirm-actions">
          <Button variant="ghost" onClick={() => setConfirmLeave(false)}>
            Fortsätt spela
          </Button>

          <Button onClick={onClose}>Ja, avbryt</Button>
        </div>
      </Modal>
    );
  }

  // ---------- Frågan ----------

  return (
    <Modal onClose={handleRequestClose}>
      <div className="quiz__ranked-bar">
        {timer}

        <span className="quiz__ranked-score">{counts.correct} rätt</span>

        <span
          className="quiz__wrongs"
          aria-label={`${counts.wrong} av ${attempt.maxWrongAnswers} fel`}
        >
          {Array.from({ length: attempt.maxWrongAnswers }).map((_, i) => (
            <span
              key={i}
              className={`quiz__wrong-mark${
                i < counts.wrong ? " quiz__wrong-mark--filled" : ""
              }`}
              aria-hidden="true"
            >
              ✕
            </span>
          ))}
        </span>
      </div>

      <h2 className="quiz__question">{current.prompt}</h2>

      {current.questionType === "Conversation" ? (
        <ConversationQuestion body={current.body} />
      ) : (
        <Card className="quiz__quote">
          <p>{current.body}</p>
        </Card>
      )}

      <div className="quiz__options">
        {current.alternatives.map((option, i) => {
          const isSelected = option.alternativeId === selectedId;
          const isCorrect =
            feedback !== null &&
            option.alternativeId === feedback.correctAlternativeId;

          let state = "";
          if (feedback && isCorrect) state = "quiz__option--correct";
          else if (feedback && isSelected) state = "quiz__option--incorrect";
          else if (feedback) state = "quiz__option--dimmed";

          return (
            <button
              key={option.alternativeId}
              type="button"
              className={`quiz__option ${state}`.trim()}
              onClick={() => handleSelect(option.alternativeId)}
              disabled={feedback !== null || busy}
            >
              <span className="quiz__option-badge">{LETTERS[i]}</span>
              <span className="quiz__option-text">
                {option.alternativeText}
              </span>

              {feedback && isCorrect && (
                <CheckIcon className="quiz__option-status quiz__option-status--correct" />
              )}

              {feedback && isSelected && !isCorrect && (
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
    </Modal>
  );
}
