import { Link } from "react-router-dom";

import FeatureCard from "../components/FeatureCard";
import SlangOfDayCard from "../components/SlangOfDayCard";
import { BookIcon, ChartIcon, TargetIcon, UserIcon } from "../components/Icons";

import "../styles/LandingPage.css";

/*
  Ligger utanför komponenten så att arrayen bara skapas en gång,
  inte vid varje rendering.
  Icon hämtas från icons component
*/
const FEATURES = [
  {
    to: "/ordbok",
    icon: <BookIcon width={26} height={26} />,
    title: "WordStash",
    description: "Hitta betydelser och exempel på 100+ slangord.",
    linkLabel: "Utforska",
  },
  {
    to: "/quiz",
    icon: <TargetIcon width={26} height={26} />,
    title: "Quiz",
    description: "Testa dina kunskaper och lär dig lingot.",
    linkLabel: "Starta quiz",
  },
  {
    to: "/topplistor",
    icon: <ChartIcon width={26} height={26} />,
    title: "Topplistor",
    description: "Se vilka ord som trendar mest bland kidsen.",
    linkLabel: "Visa topplistor",
  },
  {
    to: "/min-profil",
    icon: <UserIcon width={26} height={26} />,
    title: "Min profil",
    description: "Spara favoriter och följ dina framsteg.",
    linkLabel: "Till min profil",
  },
];

function LandingPage() {
  return (
    <main className="landing">
      <section className="landing__hero">
        <div className="landing__intro">
          <h1 className="landing__title">
            Förstå slang.
            <br />
            <span className="landing__title-accent">Förstå dina kids.</span>
          </h1>

          <p className="landing__subtitle">All slang samlad på ett ställe</p>

          <div className="landing__cta">
            <Link to="/ordbok" className="btn btn--primary">
              <BookIcon width={20} height={20} />
              Utforska ordboken
            </Link>

            <Link to="/quiz" className="btn btn--ghost">
              <TargetIcon width={20} height={20} />
              Testa quiz
            </Link>
          </div>
        </div>

        <img
          className="landing__phone"
          src="/images/phone-mockup.png"
          alt="Chattkonversation där en tonåring skriver 'No cap, det där var cringe' och föräldern svarar 'Vad betyder det ens?'"
          width={277}
          height={476}
        />

        <SlangOfDayCard />
        {/* Hårdkodad data tills endpointen för dagens ord finns.
        <SlangOfDayCard
          word="Cooked"
          meaning="Körd / chanslös"
          exampleQuestion="Jag har prov imorgon"
          exampleAnswer="Du är cooked 💀"
          to="/ordbok/cooked"
        /> */}
      </section>

      <section className="landing__features" aria-label="Vad du kan göra">
        {FEATURES.map((feature) => (
          <FeatureCard key={feature.to} {...feature} />
        ))}
      </section>
    </main>
  );
}

export default LandingPage;
