import { useEffect, useState } from "react";
import { Link, useNavigate } from "react-router-dom";

import {
  createAdminWord,
  getAdminTags,
  getAdminInflectionTypes,
} from "../../api/admin";

import Card from "../../components/ui/Card";

import "../../styles/AdminWordDetailsPage.css";

function AdminCreateWordPage() {
  const navigate = useNavigate();

  const [formData, setFormData] = useState({
    word: "",
    meaning: "",
    examples: [""],
    tagIds: [],
    inflections: [],
  });

  const [availableTags, setAvailableTags] = useState([]);
  const [availableInflectionTypes, setAvailableInflectionTypes] = useState([]);
  const [loadingInflectionTypes, setLoadingInflectionTypes] = useState(true);
  const [inflectionTypesError, setInflectionTypesError] = useState("");

  const [loadingTags, setLoadingTags] = useState(true);
  const [error, setError] = useState("");
  const [saving, setSaving] = useState(false);

  // --------------------------------------------------------------------------
  // Hämta befintliga taggar
  // --------------------------------------------------------------------------

  useEffect(() => {
    const controller = new AbortController();

    async function loadTags() {
      try {
        const tags = await getAdminTags({
          signal: controller.signal,
        });

        setAvailableTags(tags);
        setError("");
      } catch (error) {
        if (error.name === "AbortError") {
          return;
        }

        setError("Kunde inte hämta taggarna.");
      } finally {
        if (!controller.signal.aborted) {
          setLoadingTags(false);
        }
      }
    }

    loadTags();

    return () => controller.abort();
  }, []);

  // --------------------------------------------------------------------------
  // Hämta böjningstyper och hantera böjningsrader
  // --------------------------------------------------------------------------

  useEffect(() => {
    const controller = new AbortController();

    async function loadInflectionTypes() {
      try {
        const types = await getAdminInflectionTypes({ signal: controller.signal });
        setAvailableInflectionTypes(types);
        setInflectionTypesError("");
      } catch (error) {
        if (error.name !== "AbortError") {
          setInflectionTypesError("Kunde inte hämta böjningstyperna. Du kan fortfarande skapa ord utan böjningar.");
        }
      } finally {
        if (!controller.signal.aborted) setLoadingInflectionTypes(false);
      }
    }

    loadInflectionTypes();
    return () => controller.abort();
  }, []);

  function handleInflectionChange(index, field, value) {
    setFormData((current) => ({
      ...current,
      inflections: current.inflections.map((inflection, rowIndex) =>
        rowIndex === index ? { ...inflection, [field]: value } : inflection,
      ),
    }));
  }

  function handleAddInflection() {
    setFormData((current) => ({
      ...current,
      inflections: [...current.inflections, { inflectionTypeId: "", inflectedText: "" }],
    }));
  }

  function handleRemoveInflection(index) {
    setFormData((current) => ({
      ...current,
      inflections: current.inflections.filter((_, rowIndex) => rowIndex !== index),
    }));
  }

  function handleFieldChange(event) {
    const { name, value } = event.target;

    setFormData((current) => ({
      ...current,
      [name]: value,
    }));
  }

  // --------------------------------------------------------------------------
  // Ändra meningsexempel
  // --------------------------------------------------------------------------

  function handleExampleChange(index, value) {
    setFormData((current) => ({
      ...current,
      examples: current.examples.map((example, exampleIndex) =>
        exampleIndex === index ? value : example,
      ),
    }));
  }

  function handleAddExample() {
    setFormData((current) => ({
      ...current,
      examples: [...current.examples, ""],
    }));
  }

  function handleRemoveExample(index) {
    setFormData((current) => ({
      ...current,
      examples: current.examples.filter(
        (_, exampleIndex) => exampleIndex !== index,
      ),
    }));
  }

  // --------------------------------------------------------------------------
  // Välj / avmarkera tagg
  // --------------------------------------------------------------------------

  function handleTagToggle(tagId) {
    setFormData((current) => {
      const isSelected = current.tagIds.includes(tagId);

      return {
        ...current,
        tagIds: isSelected
          ? current.tagIds.filter((id) => id !== tagId)
          : [...current.tagIds, tagId],
      };
    });
  }

  // --------------------------------------------------------------------------
  // Skapa ord
  // --------------------------------------------------------------------------

  async function handleSubmit(event) {
    event.preventDefault();

    if (formData.inflections.some((inflection) =>
      !inflection.inflectionTypeId || !inflection.inflectedText.trim(),
    )) {
      setError("Välj böjningstyp och skriv en böjning på varje rad, eller ta bort raden.");
      return;
    }

    try {
      setSaving(true);
      setError("");

      const createdWord = await createAdminWord(formData);

      // Backend returnerar det skapade ordet med WordId.
      // Efter lyckat skapande går vi direkt till detaljsidan.
      navigate(`/admin/words/${createdWord.wordId}`);
    } catch (error) {
      setError(
        error.message || "Kunde inte skapa ordet.",
      );
    } finally {
      setSaving(false);
    }
  }

  return (
    <main className="admin-word-details">
      <header className="admin-word-details__header">
        <div>
          <p className="admin-word-details__eyebrow">
            Administration / Ord
          </p>

          <h1 className="admin-word-details__title">
            Lägg till ord
          </h1>

          <p className="admin-word-details__intro">
            Lägg till ett nytt slangord i ordboken.
          </p>
        </div>

        <Link
          to="/admin"
          className="admin-word-details__back-link"
        >
          ← Tillbaka
        </Link>
      </header>

      <div className="admin-word-details__grid">
        <Card className="admin-word-details__main-card">
          <form
            className="admin-word-details__form"
            onSubmit={handleSubmit}
          >
            {/* Ord */}
            <section className="admin-word-details__section">
              <label
                className="admin-word-details__label"
                htmlFor="word"
              >
                Ord
              </label>

              <input
                id="word"
                name="word"
                type="text"
                className="admin-word-details__input"
                value={formData.word}
                onChange={handleFieldChange}
                placeholder="Exempel: rizz"
              />
            </section>

            {/* Betydelse */}
            <section className="admin-word-details__section">
              <label
                className="admin-word-details__label"
                htmlFor="meaning"
              >
                Betydelse
              </label>

              <textarea
                id="meaning"
                name="meaning"
                className="admin-word-details__textarea"
                value={formData.meaning}
                onChange={handleFieldChange}
                rows={4}
                placeholder="Beskriv vad ordet betyder..."
              />
            </section>

            {/* Meningsexempel */}
            <section className="admin-word-details__section">
              <p className="admin-word-details__label">
                Meningsexempel
              </p>

              <div className="admin-word-details__edit-examples">
                {formData.examples.map((example, index) => (
                  <div
                    className="admin-word-details__edit-example"
                    key={index}
                  >
                    <span>{index + 1}</span>

                    <input
                      type="text"
                      className="admin-word-details__input"
                      value={example}
                      placeholder="Skriv ett exempel..."
                      onChange={(event) =>
                        handleExampleChange(
                          index,
                          event.target.value,
                        )
                      }
                    />

                    <button
                      className="btn btn--ghost admin-word-details__remove-example"
                      type="button"
                      onClick={() =>
                        handleRemoveExample(index)
                      }
                      disabled={formData.examples.length === 1}
                    >
                      Ta bort
                    </button>
                  </div>
                ))}
              </div>

              <button
                className="btn btn--ghost admin-word-details__add-example"
                type="button"
                onClick={handleAddExample}
              >
                + Lägg till exempel
              </button>
            </section>

            <section className="admin-word-details__section">
              <p className="admin-word-details__label">Böjningar (frivilligt)</p>
              <p className="admin-word-details__inflection-help">
                Lägg till böjningar om det passar ordet.
              </p>

              {loadingInflectionTypes && (
                <p className="admin-word-details__status">Hämtar böjningstyper…</p>
              )}
              {inflectionTypesError && (
                <p className="admin-word-details__save-error" role="alert">{inflectionTypesError}</p>
              )}
              {!loadingInflectionTypes && !inflectionTypesError && availableInflectionTypes.length === 0 && (
                <p className="admin-word-details__status">Det finns inga böjningstyper att välja.</p>
              )}

              <div className="admin-word-details__edit-examples">
                {formData.inflections.map((inflection, index) => (
                  <div className="admin-word-details__edit-inflection" key={index}>
                    <label>
                      <span className="admin-word-details__label">Böjningstyp</span>
                      <select
                        className="admin-word-details__input"
                        value={inflection.inflectionTypeId}
                        required
                        onChange={(event) => handleInflectionChange(index, "inflectionTypeId", event.target.value === "" ? "" : Number(event.target.value))}
                      >
                        <option value="">Välj typ</option>
                        {availableInflectionTypes.map((type) => (
                          <option key={type.inflectionTypeId} value={type.inflectionTypeId}>
                            {type.typeName}
                          </option>
                        ))}
                      </select>
                    </label>
                    <label>
                      <span className="admin-word-details__label">Böjning</span>
                      <input
                        className="admin-word-details__input"
                        type="text"
                        value={inflection.inflectedText}
                        required
                        placeholder="Exempel: softade"
                        onChange={(event) => handleInflectionChange(index, "inflectedText", event.target.value)}
                      />
                    </label>
                    <button
                      className="btn btn--ghost"
                      type="button"
                      aria-label={`Ta bort böjning ${index + 1}`}
                      onClick={() => handleRemoveInflection(index)}
                    >
                      Ta bort
                    </button>
                  </div>
                ))}
              </div>

              <button
                className="btn btn--ghost admin-word-details__add-example"
                type="button"
                onClick={handleAddInflection}
                disabled={loadingInflectionTypes || availableInflectionTypes.length === 0}
              >
                + Lägg till böjning
              </button>
            </section>

            {/* Taggar */}
            <section className="admin-word-details__section">
              <p className="admin-word-details__label">
                Taggar
              </p>

              {loadingTags ? (
                <p className="admin-word-details__status">
                  Hämtar taggar…
                </p>
              ) : (
                <div className="admin-word-details__tag-options">
                  {availableTags.map((tag) => {
                    const isSelected =
                      formData.tagIds.includes(tag.tagId);

                    return (
                      <label
                        className={
                          isSelected
                            ? "admin-word-details__tag-option admin-word-details__tag-option--selected"
                            : "admin-word-details__tag-option"
                        }
                        key={tag.tagId}
                      >
                        <input
                          type="checkbox"
                          checked={isSelected}
                          onChange={() =>
                            handleTagToggle(tag.tagId)
                          }
                        />

                        <span>{tag.tagName}</span>
                      </label>
                    );
                  })}
                </div>
              )}
            </section>

            {error && (
              <p
                className="admin-word-details__save-error"
                role="alert"
              >
                {error}
              </p>
            )}

            <div className="admin-word-details__form-actions">
              <button
                className="btn btn--primary"
                type="submit"
                disabled={saving || loadingTags}
              >
                {saving ? "Skapar…" : "Skapa ord"}
              </button>

              <Link
                to="/admin"
                className="btn btn--ghost"
              >
                Avbryt
              </Link>
            </div>
          </form>
        </Card>

        <aside className="admin-word-details__sidebar">
          <Card className="admin-word-details__info-card">
            <p className="admin-word-details__label">
              Taggar
            </p>

            <p className="admin-word-details__quiz-status">
              Välj endast bland befintliga taggar.
            </p>
          </Card>

          <Card className="admin-word-details__actions">
            <h2>Nytt slangord</h2>

            <p>
              Ord, betydelse, minst ett meningsexempel och minst en
              tagg krävs.
            </p>
          </Card>
        </aside>
      </div>
    </main>
  );
}

export default AdminCreateWordPage;
