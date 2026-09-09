export default function ConversationQuestion({ body }) {
  const speakers = [];

  const messages = body
    .split("\n")
    .filter((line) => line.trim() !== "")
    .map((line, index) => {
      const separatorIndex = line.indexOf(":");

      if (separatorIndex === -1) {
        return {
          id: index,
          speaker: "",
          message: line.trim(),
          side: "left",
        };
      }

      const speaker = line.slice(0, separatorIndex).trim();
      const message = line.slice(separatorIndex + 1).trim();

      if (!speakers.includes(speaker)) {
        speakers.push(speaker);
      }

      const speakerIndex = speakers.indexOf(speaker);

      return {
        id: index,
        speaker,
        message,
        side: speakerIndex % 2 === 0 ? "left" : "right",
      };
    });

  return (
    <div className="quiz-conversation">
      {messages.map((message) => (
        <div
          key={message.id}
          className={`quiz-conversation__row quiz-conversation__row--${message.side}`}
        >
          <div className="quiz-conversation__message">
            {message.speaker && (
              <span className="quiz-conversation__speaker">
                {message.speaker}
              </span>
            )}

            <p className="quiz-conversation__bubble">
              {message.message}
            </p>
          </div>
        </div>
      ))}
    </div>
  );
}