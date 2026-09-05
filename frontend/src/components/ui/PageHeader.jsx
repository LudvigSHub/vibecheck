export default function PageHeader({ eyebrow, title, headingId, children }) {
  return (
    <header className="page-header">
      {eyebrow && <p className="page-header__eyebrow">{eyebrow}</p>}
      <h1 className="page-header__title" id={headingId}>
        {title}
      </h1>
      {children}
    </header>
  );
}
