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

  const stats = [
    // Streak kopplas till riktig data i nästa steg.
    { label: "Streak", value: "5 dagar" },
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
          <p className="home__streak">Bra jobbat, du har en 5-dagars streak!</p>

          <div className="home__progress-copy">
            <p>Fortsätt där du slutade</p>
            <p className="home__progress-meta">Quiz 3 av 10</p>
          </div>

          <ProgressBar
            value={30}
            className="home__progress"
            aria-label="Quizprogression: 3 av 10"
          />

          <LinkButton to="/quiz" variant="ghost" className="home__continue">
            <TargetIcon width={19} height={19} />
            Fortsätt quiz
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
