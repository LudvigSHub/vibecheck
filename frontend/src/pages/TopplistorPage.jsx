import { useEffect, useState } from "react";

import PageHeader from "../components/ui/PageHeader";
import Card from "../components/ui/Card";
import FilterPill from "../components/ui/FilterPill";
import Button from "../components/ui/Button";
import LeaderboardRow from "../components/leaderboard/LeaderboardRow";
import { getLeaderboard } from "../api/leaderboard";
import { useAuth } from "../context/AuthContext";

import "../styles/TopplistorPage.css";

function formatAchievedAt(dateString) {
  return `Uppnått ${new Date(dateString).toLocaleDateString("sv-SE", {
    day: "numeric",
    month: "short",
  })}`;
}

function TopplistorPage({ onOpenRegister }) {
  const { isAuthenticated } = useAuth();
  const [week, setWeek] = useState("current");
  const [data, setData] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  useEffect(() => {
    const controller = new AbortController();
    setLoading(true);

    async function loadLeaderboard() {
      try {
        const result = await getLeaderboard(week, {
          signal: controller.signal,
        });
        setData(result);
        setError("");
      } catch (err) {
        if (err.name === "AbortError") {
          return;
        }
        setError("Kunde inte hämta topplistan.");
      } finally {
        if (!controller.signal.aborted) {
          setLoading(false);
        }
      }
    }

    loadLeaderboard();

    return () => controller.abort();
  }, [week]);

  return (
    <main className="topplistor-page">
      <PageHeader eyebrow="TOPPLISTOR" title="Se vem som kan mest slang">
        <p className="topplistor-page__subtitle">
          Jämför dina resultat med andra, och se vilka ord som communityn
          tycker är bäst – och sämst.
        </p>
      </PageHeader>

      <Card className="topplistor-page__leaderboard">
        <div className="topplistor-page__leaderboard-header">
          <div>
            <h2>Topplista — Användare</h2>
            <p className="topplistor-page__leaderboard-subtitle">
              Rankat efter bästa resultat i veckans quiz.
            </p>
          </div>

          <div className="topplistor-page__week-toggle">
            <FilterPill
              active={week === "current"}
              onClick={() => setWeek("current")}
            >
              Denna vecka
            </FilterPill>
            <FilterPill
              active={week === "previous"}
              onClick={() => setWeek("previous")}
            >
              Förra veckan
            </FilterPill>
          </div>
        </div>

        {error && (
          <p className="topplistor-page__status" role="alert">
            {error}
          </p>
        )}

        {!loading && !error && data && (
          <div className="topplistor-page__rows">
            {data.rows.map((row) => (
              <LeaderboardRow
                key={row.rank}
                rank={row.rank}
                name={row.userName}
                subtitle={formatAchievedAt(row.achievedAt)}
                points={row.score}
              />
            ))}

            {data.currentUserRow && (
              <LeaderboardRow
                key="current-user"
                rank={data.currentUserRow.rank}
                name={data.currentUserRow.userName}
                subtitle={formatAchievedAt(data.currentUserRow.achievedAt)}
                points={data.currentUserRow.score}
                isCurrentUser
              />
            )}
          </div>
        )}
      </Card>

      {/* Ordtopplistorna ("Topp 5 — Hissade/Dissade ord") väntar tills
          backend har en endpoint för toppord. */}

      {!isAuthenticated && (
        <Card className="topplistor-page__cta">
          <div>
            <p className="topplistor-page__cta-title">
              Vill du synas på topplistan?
            </p>
            <p className="topplistor-page__cta-text">
              Skapa ett konto för att spara dina quiz-resultat och tävla om en
              plats.
            </p>
          </div>
          <Button onClick={onOpenRegister}>Skapa konto</Button>
        </Card>
      )}
    </main>
  );
}

export default TopplistorPage;
