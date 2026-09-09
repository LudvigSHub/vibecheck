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

  // En lista i stället för en sträng. Det är hela skillnaden mot förut.
  const [selectedTags, setSelectedTags] = useState([]);

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

        // Promise.all: de två anropen har inget med varandra att göra,
        // så de ska inte köa. Vi väntar in den långsammaste i stället
        // för summan av båda.
        const [data, tagData] = await Promise.all([
          getWords({ search }),
          getTags(),
        ]);

        setWords(data);
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
        tags: selectedTags,
      });

      setWords(data);
    } catch (error) {
      console.error(error);
      setError("Kunde inte söka efter ord.");
    }
  }

  async function handleTagClick(tagName) {
    // Togglar taggen in i eller ut ur listan. Notera att vi bygger ett
    // NYTT array i stället för att pusha in i det gamla – muterar man
    // befintligt state ser React ingen förändring och renderar inte om.
    const newTags = selectedTags.includes(tagName)
      ? selectedTags.filter((name) => name !== tagName)
      : [...selectedTags, tagName];

    setSelectedTags(newTags);

    try {
      setError("");

      // newTags och inte selectedTags: setState uppdaterar inte variabeln
      // direkt, så selectedTags håller fortfarande det gamla värdet här.
      const data = await getWords({
        search: searchTerm,
        tags: newTags,
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

  function handleVoteUpdate(updatedWord) {
    setWords((currentWords) =>
      currentWords.map((word) =>
        word.wordId === updatedWord.wordId
          ? {
              ...word,
              upvotes: updatedWord.upvotes,
              downvotes: updatedWord.downvotes,
              currentUserVote: updatedWord.currentUserVote,
            }
          : word,
      ),
    );
  }

  function handleResetFilters() {
    setSearchTerm("");
    setSelectedTags([]);
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
      <p className="word-stash-page__eyebrow">WordStash</p>

      <h1 className="word-stash-page__title">Lär dig snacka slang</h1>

      <p className="word-stash-page__intro">
        Upptäck nya slangord och lär dig vad de betyder. Sök, filtrera och
        utforska ord från vardagligt snack till ungdomsslang.
      </p>

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
                active={selectedTags.includes(tag.tagName)}
                disabled={
                  !availableTagIds.has(tag.tagId) &&
                  !selectedTags.includes(tag.tagName)
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
        <WordList
          words={displayedWords}
          onWordClick={handleWordClick}
          onVoteUpdate={handleVoteUpdate}
        />
      )}

      {selectedWord && (
        <Modal onClose={() => setSelectedWord(null)}>
          <WordDetails word={selectedWord} onVoteUpdate={handleVoteUpdate} />
        </Modal>
      )}
    </main>
  );
}
