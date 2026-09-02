import { Link } from "react-router-dom";

import { InfoIcon } from "./Icons";

/*
  "Dagens slang"-kortet.
  Datan kommer in som props så att kortet blir lätt att koppla
  mot backend senare utan att röra markup eller CSS.
*/
function SlangOfDayCard({ word, meaning, exampleQuestion, exampleAnswer, to }) {
  return (
    <aside className="slang-card" aria-label="Dagens slang">
      <p className="slang-card__label">Dagens slang</p>

      <hr className="slang-card__divider" />

      <h2 className="slang-card__word">{word}</h2>
      <p className="slang-card__meaning">= {meaning}</p>

      <hr className="slang-card__divider" />

      <p className="slang-card__label">Exempel</p>
      <p className="slang-card__example">
        ”{exampleQuestion}”
        <br />– ”{exampleAnswer}”
      </p>

      <Link to={to} className="slang-card__link">
        <InfoIcon width={15} height={15} />
        Mer detaljer
      </Link>
    </aside>
  );
}

export default SlangOfDayCard;
