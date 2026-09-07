import ProgressBar from "../ui/ProgressBar";

export default function TopicProgressRow({ label, value }) {
  return (
    <div className="topic-progress-row">
      <p className="topic-progress-row__label">{label}</p>
      <div className="topic-progress-row__bar-wrap">
        <ProgressBar value={value} className="topic-progress-row__bar" />
        <span className="topic-progress-row__percent">{value}%</span>
      </div>
    </div>
  );
}
