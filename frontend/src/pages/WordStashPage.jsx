import { useEffect, useState } from "react";
import { getWords, getTags } from "../api/words";
import SearchInput from "../components/ui/SearchInput";
import WordList from "../components/wordstash/WordList";
import FilterPill from "../components/ui/FilterPill";
import AlphabetNav from "../components/ui/AlphabetNav";
import "../styles/WordStashPage.css";

export default function WordStashPage() {
  const [words, setWords] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [searchTerm, setSearchTerm] = useState("");
  const [tags, setTags] = useState([]);
  const [selectedTag, setSelectedTag] = useState("");
  const [selectedLetter, setSelectedLetter] = useState("");

  useEffect(() => {
    async function loadWords() {
      try {
        setLoading(true);
        setError("");

        const data = await getWords();
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
  }, []);

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
    <main>
      <h1>Ordbok</h1>

      <SearchInput
        value={searchTerm}
        onChange={handleSearch}
        placeholder="Sök efter ett slangord..."
      />

      <div>
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
      <AlphabetNav
        availableLetters={availableLetters}
        selectedLetter={selectedLetter}
        onSelectLetter={handleLetterClick}
      />

      {displayedWords.length === 0 ? (
        <p>Inga ord hittades.</p>
      ) : (
        <WordList words={displayedWords} />
      )}
    </main>
  );
}
