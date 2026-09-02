import { useState } from "react";
import Ping from "../components/Ping";
import Tag from "../components/ui/Tag";
import FilterPill from "../components/ui/FilterPill";
import SearchInput from "../components/ui/SearchInput";
import AlphabetNav from "../components/ui/AlphabetNav";
import Avatar from "../components/ui/Avatar";
import ProgressBar from "../components/ui/ProgressBar";

function TestPage() {
  const [activeFilter, setActiveFilter] = useState("Alla");
  const [search, setSearch] = useState("");

  return (
    <div style={{ padding: 24, display: "flex", flexDirection: "column", gap: 24 }}>
      <h1>Test, ser ni mig?</h1>
      <Ping />

      <h2>UI-kit demo</h2>

      <div style={{ display: "flex", gap: 12, alignItems: "center" }}>
        <Tag>Ungdomsslang</Tag>
        <Avatar initials="PL" />
      </div>

      <div style={{ display: "flex", gap: 12, flexWrap: "wrap" }}>
        {["Alla", "Komplimang", "Ungdomsslang", "Svordom", "Negativ laddat"].map((label) => (
          <FilterPill
            key={label}
            active={activeFilter === label}
            onClick={() => setActiveFilter(label)}
          >
            {label}
          </FilterPill>
        ))}
      </div>

      <div style={{ maxWidth: 400 }}>
        <SearchInput
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          placeholder="Sök efter ett slang..."
        />
      </div>

      <AlphabetNav activeLetters={["A", "C", "G", "T"]} onSelectLetter={(l) => console.log(l)} />

      <div style={{ maxWidth: 300 }}>
        <ProgressBar value={30} />
      </div>
    </div>
  );
}

export default TestPage;