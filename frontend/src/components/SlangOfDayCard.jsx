import { useEffect, useState } from "react";

import { getWordOfTheDay, getWordById } from "../api/words";
import { InfoIcon } from "./Icons";
import Modal from "./ui/Modal";
import WordDetails from "./wordstash/WordDetails";

function SlangOfDayCard() {
  const [word, setWord] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  // Ordet som visas i popupen. null = ingen popup öppen.
  const [selectedWord, setSelectedWord] = useState(null);
  const [detailsLoading, setDetailsLoading] = useState(false);
  const [detailsError, setDetailsError] = useState("");

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

  // Hämtningen ligger i en händelsehanterare och inte i en useEffect,
  // eftersom den ska ske för att någon klickade – inte för att kortet
  // renderades.
  async function handleShowDetails() {
    if (detailsLoading) {
      return;
    }

    setDetailsLoading(true);
    setDetailsError("");

    try {
      const data = await getWordById(word.wordId);

      setSelectedWord(data);
    } catch (err) {
      console.error(err);
      // Egen felvariabel, inte error – annars byts hela kortets innehåll
      // ut mot ett felmeddelande och dagens ord försvinner.
      setDetailsError("Kunde inte hämta ordets detaljer.");
    } finally {
      setDetailsLoading(false);
    }
  }

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

          <button
            type="button"
            className="slang-card__link"
            onClick={handleShowDetails}
            disabled={detailsLoading}
          >
            <InfoIcon width={15} height={15} />
            {detailsLoading ? "Hämtar…" : "Mer detaljer"}
          </button>

          {detailsError && (
            <p className="slang-card__status" role="alert">
              {detailsError}
            </p>
          )}
        </>
      )}

      {selectedWord && (
        <Modal onClose={() => setSelectedWord(null)}>
          <WordDetails word={selectedWord} onVoteUpdate={setSelectedWord} />
        </Modal>
      )}
    </aside>
  );
}

export default SlangOfDayCard;
