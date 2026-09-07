import "../../styles/WordInflections.css";

export default function WordInflections({ inflections = [] }) {
  if (!Array.isArray(inflections) || inflections.length === 0) return null;

  // One spelling may have multiple types, e.g. keffa in plural and definite form.
  const forms = new Map();
  for (const inflection of inflections) {
    if (!inflection?.inflectedText) continue;
    const types = forms.get(inflection.inflectedText) ?? new Set();
    if (inflection.typeName) types.add(inflection.typeName);
    forms.set(inflection.inflectedText, types);
  }

  if (forms.size === 0) return null;

  return (
    <span className="word-inflections" aria-label="Böjningar">
      {[...forms].map(([text, types]) => (
        <span key={text} title={[...types].join(", ")}>
          {text}
        </span>
      ))}
    </span>
  );
}
