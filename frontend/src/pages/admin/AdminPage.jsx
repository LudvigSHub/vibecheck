import { useEffect, useState } from "react";

import { getAdminWords } from "../../api/admin";

import AdminWordList from "../../components/admin/AdminWordList";
import SearchInput from "../../components/ui/SearchInput";
import LinkButton from "../../components/ui/LinkButton";

import "../../styles/AdminPage.css";

function AdminPage() {
  const [words, setWords] = useState([]);
  const [search, setSearch] = useState("");
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  useEffect(() => {
    const controller = new AbortController();

    async function loadWords() {
      try {
        const data = await getAdminWords({
          signal: controller.signal,
        });

        setWords(data);
        setError("");
      } catch (error) {
        if (error.name === "AbortError") {
          return;
        }

        setError("Kunde inte hämta orden.");
      } finally {
        if (!controller.signal.aborted) {
          setLoading(false);
        }
      }
    }

    loadWords();

    return () => controller.abort();
  }, []);

  const normalizedSearch = search
    .trim()
    .toLocaleLowerCase("sv-SE");

  const filteredWords = normalizedSearch
    ? words.filter((word) => {
        const matchesWord = word.word
          .toLocaleLowerCase("sv-SE")
          .includes(normalizedSearch);

        const matchesMeaning = word.meaning
          .toLocaleLowerCase("sv-SE")
          .includes(normalizedSearch);

        const matchesTag = word.tags.some((tag) =>
          tag.tagName
            .toLocaleLowerCase("sv-SE")
            .includes(normalizedSearch),
        );

        return matchesWord || matchesMeaning || matchesTag;
      })
    : words;

  return (
    <main className="admin">
      <header className="admin__header">
        <p className="admin__eyebrow">
          Administration
        </p>

        <div className="admin__heading-row">
          <div>
            <h1 className="admin__title">
              Hantera ordboken
            </h1>

            <p className="admin__intro">
              Lägg till, redigera och ta bort slangord och deras innehåll.
            </p>
          </div>

          <LinkButton
           to="/admin/words/new"
            variant="primary"
          >
            + Lägg till ord
          </LinkButton>
        </div>
      </header>

      <div className="admin__toolbar">
        <SearchInput
          value={search}
          onChange={(event) => setSearch(event.target.value)}
          placeholder="Sök efter ord, betydelse eller tagg..."
        />

        {!loading && !error && (
          <p className="admin__word-count">
            {filteredWords.length} av {words.length} ord
          </p>
        )}
      </div>

      {loading && (
        <p className="admin__status">
          Hämtar ord…
        </p>
      )}

      {error && (
        <p
          className="admin__status admin__status--error"
          role="alert"
        >
          {error}
        </p>
      )}

      {!loading && !error && (
        <AdminWordList words={filteredWords} />
      )}
    </main>
  );
}

export default AdminPage;