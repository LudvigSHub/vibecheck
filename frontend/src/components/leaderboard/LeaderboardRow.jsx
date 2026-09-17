export default function LeaderboardRow({
  rank,
  name,
  subtitle,
  points,
  isCurrentUser = false,
}) {
  const badgeClass = rank <= 3 ? `rank-badge--${rank}` : "rank-badge--default";
  const rowClass = isCurrentUser
    ? "leaderboard-row leaderboard-row--current"
    : "leaderboard-row";

  return (
    <div className={rowClass}>
      <span className={`rank-badge ${badgeClass}`}>{rank}</span>
      <div className="leaderboard-row__info">
        <p className="leaderboard-row__name">{name}</p>
        {subtitle && <p className="leaderboard-row__subtitle">{subtitle}</p>}
      </div>
      <span className="leaderboard-row__points">{points} p</span>
    </div>
  );
}
