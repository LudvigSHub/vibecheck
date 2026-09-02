import { Link } from "react-router-dom";

import { ArrowRightIcon } from "./Icons";

/*
  Ett funktionskort på landningssidan.
  Allt innehåll kommer in som props – komponenten vet inget om
  WordStash eller Quiz, den vet bara hur ett kort ser ut.
*/
function FeatureCard({ icon, title, description, linkLabel, to }) {
  return (
    // "article" istället för "div" för bättre läsbarhet / tillgänglighet
    <article className="feature-card">
      <span className="feature-card__icon">{icon}</span>

      <div>
        <h3 className="feature-card__title">{title}</h3>
        <p className="feature-card__text">{description}</p>

        <Link to={to} className="feature-card__link">
          {linkLabel}
          <ArrowRightIcon width={16} height={16} />
        </Link>
      </div>
    </article>
  );
}

export default FeatureCard;
