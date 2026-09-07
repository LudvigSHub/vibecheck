import { useEffect, useState } from "react";
import { useSearchParams } from "react-router-dom";
import { getWords, getTags, getWordById } from "../api/words";
import SearchInput from "../components/ui/SearchInput";
import WordList from "../components/wordstash/WordList";
import FilterPill from "../components/ui/FilterPill";
import AlphabetNav from "../components/ui/AlphabetNav";
import Modal from "../components/ui/Modal";
import WordDetails from "../components/wordstash/WordDetails";
import "../styles/WordStashPage.css";

export default function WordStashPage() {
  const [searchParams] = useSearchParams();
  const [words, setWords] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [searchTerm, setSearchTerm] = useState("");
  const [tags, setTags] = useState([]);
  const [selectedTag, setSelectedTag] = useState("");
  const [selectedLetter, setSelectedLetter] = useState("");
  const [selectedWord, setSelectedWord] = useState(null);
  const [showFilters, setShowFilters] = useState(false);

  useEffect(() => {
    async function loadWords() {
      try {
        setLoading(true);
        setError("");

        const search = searchParams.get("search") || "";
        setSearchTerm(search);

        const data = await getWords({ search });
        setWords(data);

        const tagData = await getTags();
        setTags(tagData);
      } catch (error) {
        console.error(error);
        setError("Kunde inte hämta orden.");
      } finally {
        setLoading(false);
      }
    }

    loadWords();
  }, [searchParams]);

  async function handleSearch(event) {
    const value = event.target.value;

    setSearchTerm(value);

    try {
      setError("");

      const data = await getWords({
        search: value,
        tag: selectedTag,
      });

      setWords(data);
    } catch (error) {
      console.error(error);
      setError("Kunde inte söka efter ord.");
    }
  }

  async function handleTagClick(tagName) {
    const newTag = selectedTag === tagName ? "" : tagName;

    setSelectedTag(newTag);

    try {
      setError("");

      const data = await getWords({
        search: searchTerm,
        tag: newTag,
      });

      setWords(data);
    } catch (error) {
      console.error(error);
      setError("Kunde inte filtrera orden.");
    }
  }

  function handleLetterClick(letter) {
    setSelectedLetter(selectedLetter === letter ? "" : letter);
  }

  async function handleWordClick(word) {
    try {
      setError("");

      const data = await getWordById(word.wordId);
      setSelectedWord(data);
    } catch (error) {
      console.error(error);
      setError("Kunde inte hämta ordets detaljer.");
    }
  }

  function handleResetFilters() {
    setSearchTerm("");
    setSelectedTag("");
    setSelectedLetter("");

    getWords()
      .then(setWords)
      .catch((error) => {
        console.error(error);
        setError("Kunde inte återställa filtren.");
      });
  }

  if (loading) {
    return <p>Hämtar ord...</p>;
  }

  if (error) {
    return <p>{error}</p>;
  }

  const availableTagIds = new Set(
    words.flatMap((word) => word.tags.map((tag) => tag.tagId)),
  );

  const availableLetters = [
    ...new Set(words.map((word) => word.word.charAt(0).toUpperCase())),
  ];

  const displayedWords = selectedLetter
    ? words.filter(
        (word) => word.word.charAt(0).toUpperCase() === selectedLetter,
      )
    : words;

  return (
    <main className="word-stash-page">
      <h1 className="word-stash-page__title">WordStash</h1>

      <div className="word-stash-page__sticky">
        <div className="word-stash-page__controls">
          <SearchInput
            value={searchTerm}
            onChange={handleSearch}
            placeholder="Sök efter ett slangord..."
          />

          <div className="word-stash-page__filter-actions">
            <button
              type="button"
              className="word-stash-page__filter-toggle"
              onClick={() => setShowFilters(!showFilters)}
              aria-expanded={showFilters}
            >
              {showFilters ? "Dölj filter" : "Visa filter"}
            </button>

            <button
              type="button"
              className="word-stash-page__reset"
              onClick={handleResetFilters}
            >
              Återställ filter
            </button>
          </div>

          <div
            className={`word-stash-page__filters${
              showFilters ? " word-stash-page__filters--visible" : ""
            }`}
          >
            {tags.map((tag) => (
              <FilterPill
                key={tag.tagId}
                active={selectedTag === tag.tagName}
                disabled={
                  !availableTagIds.has(tag.tagId) && selectedTag !== tag.tagName
                }
                onClick={() => handleTagClick(tag.tagName)}
              >
                {tag.tagName}
              </FilterPill>
            ))}
          </div>
        </div>

        <AlphabetNav
          availableLetters={availableLetters}
          selectedLetter={selectedLetter}
          onSelectLetter={handleLetterClick}
        />
      </div>

      {displayedWords.length === 0 ? (
        <p>Inga ord hittades.</p>
      ) : (
        <WordList words={displayedWords} onWordClick={handleWordClick} />
      )}

      {selectedWord && (
        <Modal onClose={() => setSelectedWord(null)}>
          <WordDetails word={selectedWord} />
        </Modal>
      )}
    </main>
  );
}
