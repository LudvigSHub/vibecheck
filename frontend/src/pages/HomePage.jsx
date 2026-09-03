import { useEffect, useState } from "react";

import FeatureCard from "../components/FeatureCard";
import SlangOfDayCard from "../components/SlangOfDayCard";
import { BookIcon, ChartIcon, TargetIcon, UserIcon } from "../components/Icons";
import LinkButton from "../components/ui/LinkButton";
import ProgressBar from "../components/ui/ProgressBar";
import { getHomeSummary } from "../api/home";
import { useAuth } from "../context/AuthContext";

import "../styles/HomePage.css";

const QUICK_LINKS = [
  {
    to: "/ordbok",
    icon: <BookIcon width={28} height={28} />,
    title: "WordStash",
    description: "Hitta betydelser och exempel på 100+ slangord.",
    linkLabel: "Utforska",
  },
  {
    to: "/quiz",
    icon: <TargetIcon width={28} height={28} />,
    title: "Quiz",
    description: "Testa dina kunskaper och lär dig lingot.",
    linkLabel: "Starta quiz",
  },
  {
    to: "/topplistor",
    icon: <ChartIcon width={28} height={28} />,
    title: "Topplistor",
    description: "Se vilka ord som trendar mest bland kidsen.",
    linkLabel: "Visa topplistor",
  },
  {
    to: "/min-profil",
    icon: <UserIcon width={28} height={28} />,
    title: "Min profil",
    description: "Spara favoriter och följ dina framsteg.",
    linkLabel: "Till min profil",
  },
];

function HomePage() {
  const { user } = useAuth();
  const [summary, setSummary] = useState(null);
  const [summaryLoading, setSummaryLoading] = useState(true);
  const [summaryError, setSummaryError] = useState("");

  useEffect(() => {
    const controller = new AbortController();

    async function loadSummary() {
      try {
        const data = await getHomeSummary({ signal: controller.signal });

        setSummary(data);
        setSummaryError("");
      } catch (error) {
        if (error.name === "AbortError") {
          return;
        }

        setSummaryError("Kunde inte hämta din quizstatistik.");
      } finally {
        if (!controller.signal.aborted) {
          setSummaryLoading(false);
        }
      }
    }

    loadSummary();

    return () => controller.abort();
  }, []);

  const streakDays = summary?.currentStreak ?? 0;
  const streakLabel = `${streakDays} ${streakDays === 1 ? "dag" : "dagar"}`;
  const streakMessage = summaryLoading
    ? "Vi hämtar din quizstatistik…"
    : summaryError
      ? "Din streak kunde inte hämtas just nu."
      : streakDays === 0
        ? "Gör ett quiz idag och starta din streak!"
        : `Bra jobbat, du har en streak på ${streakLabel}!`;

  const activeQuiz = summary?.activeQuiz;
  const quizProgress = activeQuiz?.totalQuestionCount
    ? Math.round(
        (activeQuiz.answeredQuestionCount / activeQuiz.totalQuestionCount) * 100,
      )
    : 0;
  const quizHeading = summaryLoading
    ? "Hämtar ditt senaste quiz…"
    : activeQuiz
      ? "Fortsätt där du slutade"
      : "Redo för nästa quiz?";
  const quizMeta = activeQuiz
    ? `${activeQuiz.quizName}: ${activeQuiz.answeredQuestionCount} av ${activeQuiz.totalQuestionCount} frågor besvarade`
    : "Du har inget påbörjat quiz.";
  const stats = [
    { label: "Streak", value: summaryLoading ? "…" : streakLabel },
    {
      label: "Bästa resultat",
      value: summaryLoading
        ? "…"
        : summary?.bestScore == null
          ? "–"
          : `${summary.bestScore}%`,
    },
    {
      label: "Quiz gjorda",
      value: summaryLoading ? "…" : (summary?.completedQuizCount ?? "–"),
    },
  ];

  return (
    <main className="home">
      <section className="home__overview" aria-labelledby="home-heading">
        <div className="home__welcome">
          <p className="home__eyebrow">Din översikt</p>
          <h1 id="home-heading" className="home__title">
            Hej{user?.userName ? `, ${user.userName}` : ""}!
          </h1>
          <p className="home__streak">{streakMessage}</p>

          <div className="home__progress-copy">
            <p>{quizHeading}</p>
            {!summaryLoading && (
              <p className="home__progress-meta">{quizMeta}</p>
            )}
          </div>

          {activeQuiz && (
            <ProgressBar
              value={quizProgress}
              className="home__progress"
              aria-label={`Quizprogression: ${activeQuiz.answeredQuestionCount} av ${activeQuiz.totalQuestionCount} frågor`}
            />
          )}

          <LinkButton to="/quiz" variant="ghost" className="home__continue">
            <TargetIcon width={19} height={19} />
            {activeQuiz ? "Fortsätt quiz" : "Starta quiz"}
          </LinkButton>
        </div>

        <img
          className="home__phone"
          src="/images/phone-mockup.png"
          alt="Exempel på en chatt med slanguttryck"
          width="277"
          height="476"
        />

        <SlangOfDayCard />
      </section>

      <dl className="home__stats" aria-label="Din statistik">
        {stats.map((stat) => (
          <div className="home__stat" key={stat.label}>
            <dt>{stat.label}</dt>
            <dd>{stat.value}</dd>
          </div>
        ))}
      </dl>

      {summaryError && (
        <p className="home__stats-status" role="alert">
          {summaryError}
        </p>
      )}

      <section className="home__features" aria-label="Snabbnavigering">
        {QUICK_LINKS.map((feature) => (
          <FeatureCard key={feature.to} {...feature} />
        ))}
      </section>
    </main>
  );
}

export default HomePage;
