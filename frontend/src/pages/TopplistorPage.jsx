import { useEffect, useState } from "react";

import PageHeader from "../components/ui/PageHeader";
import Card from "../components/ui/Card";
import Button from "../components/ui/Button";
import LeaderboardRow from "../components/leaderboard/LeaderboardRow";
import { getLeaderboard } from "../api/leaderboard";
import { useAuth } from "../context/AuthContext";
import WordRankRow from "../components/leaderboard/WordRankRow";
import { getWords } from "../api/words";

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
    const [words, setWords] = useState([]);


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

    useEffect(() => {
    getWords()
      .then(setWords)
      .catch(() => setWords([]));
  }, []);

  const topUpvoted = [...words]
    .sort((a, b) => b.upvotes - a.upvotes)
    .slice(0, 5);

  const topDownvoted = [...words]
    .sort((a, b) => b.downvotes - a.downvotes)
    .slice(0, 5);

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

          <select
            className="topplistor-page__week-select"
            value={week}
            onChange={(event) => setWeek(event.target.value)}
          >
            <option value="current">Denna vecka</option>
            <option value="previous">Förra veckan</option>
          </select>
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

            {data.currentUserRow && 
              !data.rows.some((row) => row.rank === data.currentUserRow.rank) && (
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

      <div className="topplistor-page__word-lists">
        <Card className="topplistor-page__word-list">
          <h2>Topp 5 — Hissade ord</h2>
          <p className="topplistor-page__leaderboard-subtitle">
            Ord som communityn tycker bäst om.
          </p>
          <div className="topplistor-page__rows">
            {topUpvoted.map((word, index) => (
              <WordRankRow
                key={word.wordId}
                rank={index + 1}
                word={word.word}
                votes={word.upvotes}
                direction="up"
              />
            ))}
          </div>
        </Card>

        <Card className="topplistor-page__word-list">
          <h2>Topp 5 — Dissade ord</h2>
          <p className="topplistor-page__leaderboard-subtitle">
            Ord som communityn tycker sämst om.
          </p>
          <div className="topplistor-page__rows">
            {topDownvoted.map((word, index) => (
              <WordRankRow
                key={word.wordId}
                rank={index + 1}
                word={word.word}
                votes={word.downvotes}
                direction="down"
              />
            ))}
          </div>
        </Card>
      </div>


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
