import { useEffect, useState } from "react";
import { Link, useNavigate, useParams } from "react-router-dom";

import {
  getAdminInflectionTypes,
  getAdminTags,
  getAdminWord,
  updateAdminWord,
  deleteAdminWord,
} from "../../api/admin";

import Card from "../../components/ui/Card";
import Tag from "../../components/ui/Tag";

import "../../styles/AdminWordDetailsPage.css";

function AdminWordDetailsPage() {
  const { id } = useParams();
  const navigate = useNavigate();

  const [word, setWord] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  const [isEditing, setIsEditing] = useState(false);

  const [formData, setFormData] = useState({
    word: "",
    meaning: "",
    examples: [],
    tagIds: [],
    inflections: [],
  });

  const [availableTags, setAvailableTags] = useState([]);
  const [availableInflectionTypes, setAvailableInflectionTypes] = useState([]);

  const [saving, setSaving] = useState(false);
  const [saveError, setSaveError] = useState("");

  const [deleting, setDeleting] = useState(false);

  useEffect(() => {
    const controller = new AbortController();

    async function loadWord() {
      try {
        const data = await getAdminWord(id, {
          signal: controller.signal,
        });

        setWord(data);
        setError("");
      } catch (error) {
        if (error.name === "AbortError") {
          return;
        }

        setError("Kunde inte hämta ordet.");
      } finally {
        if (!controller.signal.aborted) {
          setLoading(false);
        }
      }
    }

    loadWord();

    return () => controller.abort();
  }, [id]);

  // --------------------------------------------------------------------------
  // Starta redigering
  // --------------------------------------------------------------------------

  async function handleStartEditing() {
    try {
      setSaveError("");

      // Admin får endast välja bland befintliga taggar och böjningstyper.
      const [tags, inflectionTypes] = await Promise.all([
        getAdminTags(),
        getAdminInflectionTypes(),
      ]);

      setAvailableTags(tags);
      setAvailableInflectionTypes(inflectionTypes);

      // Fyll formuläret med ordets nuvarande värden.
      setFormData({
        word: word.word,
        meaning: word.meaning,
        examples: [...word.examples],
        tagIds: word.tags.map((tag) => tag.tagId),
        inflections: word.inflections.map((inflection) => ({
          inflectionTypeId: inflection.inflectionTypeId,
          inflectedText: inflection.inflectedText,
        })),
      });

      setIsEditing(true);
    } catch {
      setSaveError("Kunde inte hämta taggar eller böjningstyper.");
    }
  }

  // --------------------------------------------------------------------------
  // Avbryt redigering
  // --------------------------------------------------------------------------

  function handleCancelEditing() {
    setIsEditing(false);
    setSaveError("");

    // Återställ formuläret till de värden som faktiskt finns sparade.
    setFormData({
      word: word.word,
      meaning: word.meaning,
      examples: [...word.examples],
      tagIds: word.tags.map((tag) => tag.tagId),
      inflections: word.inflections.map((inflection) => ({
        inflectionTypeId: inflection.inflectionTypeId,
        inflectedText: inflection.inflectedText,
      })),
    });
  }

  // --------------------------------------------------------------------------
  // Ändra vanliga textfält
  // --------------------------------------------------------------------------

  function handleFieldChange(event) {
    const { name, value } = event.target;

    setFormData((current) => ({
      ...current,
      [name]: value,
    }));
  }

  // --------------------------------------------------------------------------
  // Ändra ett specifikt meningsexempel
  // --------------------------------------------------------------------------

  function handleExampleChange(index, value) {
    setFormData((current) => ({
      ...current,
      examples: current.examples.map((example, exampleIndex) =>
        exampleIndex === index ? value : example,
      ),
    }));
  }

  // --------------------------------------------------------------------------
  // Lägg till meningsexempel
  // --------------------------------------------------------------------------

  function handleAddExample() {
    setFormData((current) => ({
      ...current,
      examples: [...current.examples, ""],
    }));
  }

  // --------------------------------------------------------------------------
  // Ta bort meningsexempel
  // --------------------------------------------------------------------------

  function handleRemoveExample(index) {
    setFormData((current) => ({
      ...current,
      examples: current.examples.filter(
        (_, exampleIndex) => exampleIndex !== index,
      ),
    }));
  }

  // --------------------------------------------------------------------------
  // Hantera böjningar
  // --------------------------------------------------------------------------

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
      inflections: [
        ...current.inflections,
        { inflectionTypeId: "", inflectedText: "" },
      ],
    }));
  }

  function handleRemoveInflection(index) {
    setFormData((current) => ({
      ...current,
      inflections: current.inflections.filter(
        (_, rowIndex) => rowIndex !== index,
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
  // Spara redigering
  // --------------------------------------------------------------------------

  async function handleSave(event) {
    event.preventDefault();

    if (formData.inflections.some((inflection) =>
      !inflection.inflectionTypeId || !inflection.inflectedText.trim(),
    )) {
      setSaveError(
        "Välj böjningstyp och skriv en böjning på varje rad, eller ta bort raden.",
      );
      return;
    }

    try {
      setSaving(true);
      setSaveError("");

      const updatedWord = await updateAdminWord(id, formData);

      // Backend returnerar den uppdaterade detaljvyn.
      // Vi använder den direkt så sidan uppdateras utan ny GET-request.
      setWord(updatedWord);

      setIsEditing(false);
    } catch (error) {
      setSaveError(
        error.message || "Kunde inte spara ändringarna.",
      );
    } finally {
      setSaving(false);
    }
  }

  // --------------------------------------------------------------------------
  // Ta bort ord
  // --------------------------------------------------------------------------

  async function handleDelete() {
    // Frontend hindrar borttagning direkt om vi redan vet
    // att ordet används i quiz. Backend har dessutom samma skydd.
    if (word.isUsedInQuiz) {
      setSaveError(
        `Ordet "${word.word}" kan inte tas bort eftersom det används i en quizfråga.`,
      );
      return;
    }

    const confirmed = window.confirm(
      `Är du säker på att du vill ta bort "${word.word}"?`,
    );

    if (!confirmed) {
      return;
    }

    try {
      setDeleting(true);
      setSaveError("");

      await deleteAdminWord(id);

      navigate("/admin");
    } catch (error) {
      setSaveError(
        error.message || "Kunde inte ta bort ordet.",
      );
    } finally {
      setDeleting(false);
    }
  }

  // --------------------------------------------------------------------------
  // Loading / error
  // --------------------------------------------------------------------------

  if (loading) {
    return (
      <main className="admin-word-details">
        <p className="admin-word-details__status">
          Hämtar ord…
        </p>
      </main>
    );
  }

  if (error || !word) {
    return (
      <main className="admin-word-details">
        <p
          className="admin-word-details__status admin-word-details__status--error"
          role="alert"
        >
          {error || "Ordet kunde inte hittas."}
        </p>

        <Link
          to="/admin"
          className="admin-word-details__back-link"
        >
          ← Tillbaka till ordboken
        </Link>
      </main>
    );
  }

  return (
    <main className="admin-word-details">
      <header className="admin-word-details__header">
        <div>
          <p className="admin-word-details__eyebrow">
            Administration / Ord
          </p>

          <h1 className="admin-word-details__title">
            {word.word}
          </h1>

          <p className="admin-word-details__intro">
            {isEditing
              ? "Redigera informationen för ordet."
              : "Visa och hantera informationen för ordet."}
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

        {/* ================================================================
            Huvudkort
            ================================================================ */}

        <Card className="admin-word-details__main-card">
          {isEditing ? (
            <form
              className="admin-word-details__form"
              onSubmit={handleSave}
            >
            {word.isUsedInQuiz && (
            <div className="admin-word-details__edit-warning" role="note">
                <strong>Detta ord används i ett quiz.</strong>
                <p>
                Ändringar av ordet eller betydelsen kan påverka quizets innehåll.
                </p>
            </div>
            )}

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

              {/* Böjningar */}
              <section className="admin-word-details__section">
                <p className="admin-word-details__label">
                  Böjningar (frivilligt)
                </p>
                <p className="admin-word-details__inflection-help">
                  Lägg till böjningar om det passar ordet.
                </p>

                <div className="admin-word-details__edit-examples">
                  {formData.inflections.map((inflection, index) => (
                    <div
                      className="admin-word-details__edit-inflection"
                      key={index}
                    >
                      <label>
                        <span className="admin-word-details__label">
                          Böjningstyp
                        </span>
                        <select
                          className="admin-word-details__input"
                          value={inflection.inflectionTypeId}
                          required
                          onChange={(event) =>
                            handleInflectionChange(
                              index,
                              "inflectionTypeId",
                              event.target.value === ""
                                ? ""
                                : Number(event.target.value),
                            )
                          }
                        >
                          <option value="">Välj typ</option>
                          {availableInflectionTypes.map((type) => (
                            <option
                              key={type.inflectionTypeId}
                              value={type.inflectionTypeId}
                            >
                              {type.typeName}
                            </option>
                          ))}
                        </select>
                      </label>

                      <label>
                        <span className="admin-word-details__label">
                          Böjning
                        </span>
                        <input
                          className="admin-word-details__input"
                          type="text"
                          value={inflection.inflectedText}
                          required
                          placeholder="Exempel: softade"
                          onChange={(event) =>
                            handleInflectionChange(
                              index,
                              "inflectedText",
                              event.target.value,
                            )
                          }
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
                  disabled={availableInflectionTypes.length === 0}
                >
                  + Lägg till böjning
                </button>
              </section>

              {/* Taggar */}
              <section className="admin-word-details__section">
                <p className="admin-word-details__label">
                  Taggar
                </p>

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
              </section>

              {saveError && (
                <p
                  className="admin-word-details__save-error"
                  role="alert"
                >
                  {saveError}
                </p>
              )}

              <div className="admin-word-details__form-actions">
                <button
                  className="btn btn--primary"
                  type="submit"
                  disabled={saving}
                >
                  {saving
                    ? "Sparar…"
                    : "Spara ändringar"}
                </button>

                <button
                  className="btn btn--ghost"
                  type="button"
                  onClick={handleCancelEditing}
                  disabled={saving}
                >
                  Avbryt
                </button>
              </div>
            </form>
          ) : (
            <>
              {/* Ord */}
              <section className="admin-word-details__section">
                <p className="admin-word-details__label">
                  Ord
                </p>

                <p className="admin-word-details__value admin-word-details__word">
                  {word.word}
                </p>
              </section>

              {/* Betydelse */}
              <section className="admin-word-details__section">
                <p className="admin-word-details__label">
                  Betydelse
                </p>

                <p className="admin-word-details__value">
                  {word.meaning}
                </p>
              </section>

              {/* Meningsexempel */}
              <section className="admin-word-details__section">
                <p className="admin-word-details__label">
                  Meningsexempel
                </p>

                <div className="admin-word-details__examples">
                  {word.examples.map((example, index) => (
                    <div
                      className="admin-word-details__example"
                      key={`${word.wordId}-${index}`}
                    >
                      <span>{index + 1}</span>
                      <p>{example}</p>
                    </div>
                  ))}
                </div>
              </section>

              {/* Böjningar */}
              <section className="admin-word-details__section">
                <p className="admin-word-details__label">
                  Böjningar
                </p>

                {word.inflections.length > 0 ? (
                  <div className="admin-word-details__examples">
                    {word.inflections.map((inflection, index) => (
                      <div
                        className="admin-word-details__example"
                        key={`${inflection.inflectionTypeId}-${inflection.inflectedText}-${index}`}
                      >
                        <span>{inflection.typeName}</span>
                        <p>{inflection.inflectedText}</p>
                      </div>
                    ))}
                  </div>
                ) : (
                  <p className="admin-word-details__status">
                    Inga böjningar tillagda.
                  </p>
                )}
              </section>

              {/* Taggar */}
              <section className="admin-word-details__section">
                <p className="admin-word-details__label">
                  Taggar
                </p>

                <div className="admin-word-details__tags">
                  {word.tags.map((tag) => (
                    <Tag key={tag.tagId}>
                      {tag.tagName}
                    </Tag>
                  ))}
                </div>
              </section>
            </>
          )}
        </Card>

        {/* ================================================================
            Sidebar
            ================================================================ */}

        <aside className="admin-word-details__sidebar">
          <Card className="admin-word-details__info-card">
            <p className="admin-word-details__label">
              Status
            </p>

            <p
              className={
                word.isUsedInQuiz
                  ? "admin-word-details__quiz-status admin-word-details__quiz-status--used"
                  : "admin-word-details__quiz-status"
              }
            >
              {word.isUsedInQuiz
                ? "Används i quiz"
                : "Används inte i quiz"}
            </p>
          </Card>

          <Card className="admin-word-details__actions">
            <h2>Hantera ord</h2>

            <p>
              Redigera ordets innehåll eller ta bort det från
              ordboken.
            </p>

            {!isEditing && (
              <button
                className="btn btn--primary"
                type="button"
                onClick={handleStartEditing}
              >
                Redigera ord
              </button>
            )}

           <button
                className="btn btn--ghost"
                type="button"
                onClick={handleDelete}
                disabled={deleting || word.isUsedInQuiz}
            >
            {deleting ? "Tar bort…" : "Ta bort ord"}
            </button>

            {word.isUsedInQuiz && (
              <p className="admin-word-details__delete-warning">
                Ordet används i quiz och kan därför inte tas bort.
              </p>
            )}

            {!isEditing && saveError && (
              <p
                className="admin-word-details__save-error"
                role="alert"
              >
                {saveError}
              </p>
            )}
          </Card>
        </aside>
      </div>
    </main>
  );
}

export default AdminWordDetailsPage;
