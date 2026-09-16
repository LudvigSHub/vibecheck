import Card from "../components/ui/Card";
import LinkButton from "../components/ui/LinkButton";
import Tag from "../components/ui/Tag";
import {
  BookIcon,
  InfoIcon,
  TargetIcon,
} from "../components/Icons";

import "../styles/AboutPage.css";

const TEAM = [
  { name: "Jonathan" },
  { name: "Simon" },
  { name: "Linnea" },
  { name: "Nando" },
  { name: "Ludvig" },
];

function AboutPage() {
  return (
    <main className="about">
      <section className="about__hero" aria-labelledby="about-title">
        <div className="about__hero-copy">
          <Tag>OM VIBECHECK</Tag>
          <h1 className="about__title" id="about-title">
            Slang förändras.
            <br />
            <span>Nu kan du hänga med.</span>
          </h1>
          <p className="about__lead">
            VibeCheck hjälper dig att förstå orden som hörs i skolan, på
            nätet och runt middagsbordet – innan du behöver fråga vad
            &quot;no cap&quot; betyder för tredje gången.
          </p>
        </div>

        <Card className="about__quote-card">
          <p className="about__quote-label">Snabb översättning</p>
          <p className="about__quote-word">“Du är goated.”</p>
          <p className="about__quote-meaning">
            Lugn. Det är en komplimang.
          </p>
        </Card>
      </section>

      <section className="about__section" aria-labelledby="about-why">
        <div className="about__section-heading">
          <p className="about__eyebrow">VARFÖR?</p>
          <h2 id="about-why">För att språket inte kommer med en manual</h2>
          <p>
            Dagens slang utvecklas snabbt. För en förälder, lärare eller annan
            vuxen kan det vara svårt att veta om ett uttryck är vardagligt, en
            komplimang, en förolämpning eller något man helst inte upprepar på
            nästa föräldramöte.
          </p>
        </div>

        <div className="about__reason-grid">
          <Card className="about__reason-card">
            <span className="about__icon">
              <InfoIcon />
            </span>
            <h3>Problemet vi såg</h3>
            <p>
              Vuxna hör orden, men saknar ofta sammanhanget. Då blir det svårt
              att avgöra både betydelsen och tonen bakom det som sägs.
            </p>
          </Card>

          <Card className="about__reason-card about__reason-card--accent">
            <span className="about__icon">
              <TargetIcon />
            </span>
            <h3>Idén vi fick</h3>
            <p>
              Samla förklaringar och praktisk träning på samma plats, så att
              slang går att lära sig utan att det känns som en läxa.
            </p>
          </Card>
        </div>
      </section>

      <section className="about__section" aria-labelledby="about-how">
        <div className="about__section-heading">
          <p className="about__eyebrow">SÅ FUNKAR DET</p>
          <h2 id="about-how">Slå upp först. Testa dig sedan.</h2>
        </div>

        <div className="about__tool-grid">
          <Card className="about__tool-card">
            <span className="about__tool-number">01</span>
            <BookIcon width={30} height={30} />
            <div>
              <h3>WordStash</h3>
              <p>
                Sök efter slangord och se betydelser, exempel och vilken typ
                av uttryck det är.
              </p>
            </div>
          </Card>

          <Card className="about__tool-card">
            <span className="about__tool-number">02</span>
            <TargetIcon width={30} height={30} />
            <div>
              <h3>Quiz</h3>
              <p>
                Testa kunskaperna i olika nivåer och lär dig orden genom
                situationer där de faktiskt skulle kunna användas.
              </p>
            </div>
          </Card>
        </div>
      </section>

      <section className="about__team" aria-labelledby="about-team">
        <div className="about__team-intro">
          <p className="about__eyebrow">TEAMET BAKOM</p>
          <h2 id="about-team">Fem systemutvecklare under utveckling</h2>
          <p>
            VibeCheck är ett projektarbete skapat av oss som studerar till
            systemutvecklare. Vi har byggt appen tillsammans – och lärt oss en
            hel del slang på köpet.
          </p>
        </div>

        <ul className="about__team-list" aria-label="Projektmedlemmar">
          {TEAM.map((member) => (
            <li className="about__member" key={member.name}>
              <p className="about__member-name">{member.name}</p>
            </li>
          ))}
        </ul>
      </section>

      <section className="about__cta" aria-label="Utforska VibeCheck">
        <div>
          <p className="about__eyebrow">REDO ATT BLI LITE MINDRE CONFUSED?</p>
          <h2>Börja med ett ord du hört men aldrig vågat fråga om.</h2>
        </div>
        <LinkButton to="/wordstash" variant="primary">
          <BookIcon width={20} height={20} />
          Utforska WordStash
        </LinkButton>
      </section>
    </main>
  );
}

export default AboutPage;
