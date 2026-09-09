import { useEffect, useState } from "react";

import PageHeader from "../components/ui/PageHeader";
import Avatar from "../components/ui/Avatar";
import Button from "../components/ui/Button";
import Card from "../components/ui/Card";
import StatCard from "../components/ui/StatCard";
import QuizHistoryTable from "../components/profile/QuizHistoryTable";
import EditAccountForm from "../components/profile/EditAccountForm";
import { getProfile } from "../api/profile";

import "../styles/ProfilePage.css";

function ProfilePage() {
  const [profile, setProfile] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [showEditAccount, setShowEditAccount] = useState(false);

  // Vi använder .then() eftersom lintregeln varnade för den tidigare await-varianten.
  // Hämtningen är fortfarande asynkron
  async function loadProfile(signal) {
    return getProfile({ signal })
      .then((data) => {
        setProfile(data);
        setError("");
      })
      .catch((err) => {
        if (err.name === "AbortError") {
          return;
        }
        setError("Kunde inte hämta din profil.");
      })
      .finally(() => {
        if (!signal || !signal.aborted) {
          setLoading(false);
        }
      });
  }

  useEffect(() => {
    const controller = new AbortController();
    loadProfile(controller.signal);
    return () => controller.abort();
  }, []);

  const initials = profile?.userName
    ? profile.userName.slice(0, 2).toUpperCase()
    : "";

  const stats = [
    {
      label: "Streak",
      value: loading ? "…" : `${profile?.currentStreak ?? 0} dagar`,
    },
    {
      label: "Bästa resultat",
      value: loading
        ? "…"
        : profile?.bestScore == null
          ? "–"
          : `${profile.bestScore}%`,
    },
    {
      label: "Quiz gjorda",
      value: loading ? "…" : (profile?.completedQuizCount ?? "–"),
    },
  ];

  const historyRows = (profile?.quizHistory ?? []).map((item) => ({
    quizName: item.quizName,
    topic: item.topic,
    score: item.score,
    date: new Date(item.completedAt).toLocaleDateString("sv-SE"),
  }));

  return (
    <main className="profile-page">
      <PageHeader title="Mitt Konto" />

      <div className="profile-page__header">
        <Avatar initials={initials} size="lg" />
        <p className="profile-page__username">{profile?.userName}</p>
        <Button variant="ghost" onClick={() => setShowEditAccount(true)}>
          Redigera konto
        </Button>
      </div>

      <dl className="profile-page__stats" aria-label="Din statistik">
        {stats.map((stat) => (
          <StatCard key={stat.label} label={stat.label} value={stat.value} />
        ))}
      </dl>

      {error && (
        <p className="profile-page__status" role="alert">
          {error}
        </p>
      )}

      <Card className="profile-page__history">
        <h2>Quiz-historik</h2>
        <p className="profile-page__history-subtitle">
          Dina senaste genomförda quiz.
        </p>
        <QuizHistoryTable rows={historyRows} />
      </Card>

      {showEditAccount && (
        <EditAccountForm
          onClose={() => setShowEditAccount(false)}
          onSuccess={() => {
            setShowEditAccount(false);
            loadProfile();
          }}
        />
      )}
    </main>
  );
}

export default ProfilePage;
