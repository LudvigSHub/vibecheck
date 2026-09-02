import { useEffect, useState } from "react";
import { Link } from "react-router-dom";

import { getWordOfTheDay } from "../api/words";
import { InfoIcon } from "./Icons";

function SlangOfDayCard() {
  const [word, setWord] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    // Avbryter anropet om komponenten försvinner innan svaret kommit.
    const controller = new AbortController();

    async function load() {
      try {
        const data = await getWordOfTheDay({ signal: controller.signal });

        setWord(data);
        setError(null);
      } catch (err) {
        // Vi avbröt själva – inte ett fel att visa för användaren.
        if (err.name === "AbortError") {
          return;
        }

        setError("Kunde inte hämta dagens ord.");
      } finally {
        if (!controller.signal.aborted) {
          setLoading(false);
        }
      }
    }

    load();

    // Städfunktionen. Körs när komponenten plockas bort.
    return () => controller.abort();
    // Tom lista = kör en gång vid montering. Utan den: oändlig loop.
  }, []);

  return (
    <aside className="slang-card" aria-label="Dagens slang">
      <p className="slang-card__label">Dagens slang</p>

      <hr className="slang-card__divider" />

      {loading && (
        <div className="slang-card__skeleton" aria-live="polite">
          <span className="slang-card__bar slang-card__bar--title" />
          <span className="slang-card__bar" />
          <span className="slang-card__bar slang-card__bar--short" />
          <span className="sr-only">Hämtar dagens ord…</span>
        </div>
      )}

      {!loading && error && <p className="slang-card__status">{error}</p>}

      {!loading && !error && word && (
        <>
          <h2 className="slang-card__word">{word.word}</h2>
          <p className="slang-card__meaning">= {word.meaning}</p>

          {word.example && (
            <>
              <hr className="slang-card__divider" />

              <p className="slang-card__label">Exempel</p>
              <p className="slang-card__example">”{word.example}”</p>
            </>
          )}

          <Link to={`/ordbok/${word.wordId}`} className="slang-card__link">
            <InfoIcon width={15} height={15} />
            Mer detaljer
          </Link>
        </>
      )}
    </aside>
  );
}

export default SlangOfDayCard;
