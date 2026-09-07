import { useState } from "react";

import PageHeader from "../components/ui/PageHeader";
import Card from "../components/ui/Card";
import Tag from "../components/ui/Tag";
import Button from "../components/ui/Button";
import LinkButton from "../components/ui/LinkButton";
import FilterPill from "../components/ui/FilterPill";
import SearchInput from "../components/ui/SearchInput";
import AlphabetNav from "../components/ui/AlphabetNav";
import Avatar from "../components/ui/Avatar";
import ProgressBar from "../components/ui/ProgressBar";
import Modal from "../components/ui/Modal";
import QuizResult from "../components/ui/QuizResult";
import Input from "../components/ui/Input";
import FormField from "../components/ui/FormField";
import StatCard from "../components/ui/StatCard";

import "../styles/StyleguidePage.css";

function Section({ title, children }) {
  return (
    <Card className="styleguide-section">
      <h2>{title}</h2>
      <div className="styleguide-row">{children}</div>
    </Card>
  );
}

function StyleguidePage() {
  const [activePill, setActivePill] = useState("alla");
  const [searchValue, setSearchValue] = useState("");
  const [showModal, setShowModal] = useState(false);

  return (
    <main className="styleguide">
      <PageHeader eyebrow="Internt verktyg" title="Styleguide" />

      <Section title="Knappar">
        <Button variant="primary">Primary</Button>
        <Button variant="ghost">Ghost</Button>
        <LinkButton to="/styleguide" variant="primary">
          LinkButton primary
        </LinkButton>
        <LinkButton to="/styleguide" variant="ghost">
          LinkButton ghost
        </LinkButton>
      </Section>

      <Section title="Ytor">
        <Card className="styleguide-card-demo">Card-innehåll</Card>
        <Tag>Tag</Tag>
        <StatCard label="Streak" value="5 dagar" />
        <StatCard label="Bästa resultat" value="87%" />
      </Section>

      <Section title="Formulär">
        <Input placeholder="Normal input" />
        <Input placeholder="Error input" error />
        <FormField
          id="styleguide-hint"
          label="Fält med hint"
          hint="Det här är en hjälptext."
        />
        <FormField
          id="styleguide-error"
          label="Fält med error"
          error="Något blev fel."
        />
        <SearchInput
          value={searchValue}
          onChange={(event) => setSearchValue(event.target.value)}
          placeholder="Sök..."
        />
      </Section>

      <Section title="Feedback & progress">
        <ProgressBar value={25} />
        <ProgressBar value={75} />
        <Avatar initials="AB" size="md" />
        <Avatar initials="CD" size="lg" />
        <FilterPill
          active={activePill === "alla"}
          onClick={() => setActivePill("alla")}
        >
          Alla
        </FilterPill>
        <FilterPill
          active={activePill === "quiz"}
          onClick={() => setActivePill("quiz")}
        >
          Quiz
        </FilterPill>
      </Section>

      <Section title="Navigering">
        <AlphabetNav activeLetters={["A", "B", "C", "Q", "S"]} />
      </Section>

      <Section title="Overlays">
        <Button onClick={() => setShowModal(true)}>Öppna modal</Button>
        {showModal && (
          <Modal onClose={() => setShowModal(false)}>
            <p>Exempelinnehåll i en modal.</p>
          </Modal>
        )}

        <QuizResult
          emoji="🏆"
          score="8/10"
          percent={80}
          message="Grymt jobbat!"
        >
          <Button variant="primary">Skapa konto</Button>
          <button type="button" className="quiz-result__link">
            Till startsidan
          </button>
        </QuizResult>
      </Section>
    </main>
  );
}

export default StyleguidePage;
