import { useEffect } from "react";
import Card from "./Card";
import { CloseIcon } from "../Icons";

export default function Modal({ onClose, children }) {
  useEffect(() => {
    function handleKeyDown(event) {
      if (event.key === "Escape") {
        onClose?.();
      }
    }

    window.addEventListener("keydown", handleKeyDown);
    return () => window.removeEventListener("keydown", handleKeyDown);
  }, [onClose]);

  function handleOverlayClick(event) {
    if (event.target === event.currentTarget) {
      onClose?.();
    }
  }

  return (
    <div className="modal-overlay" onClick={handleOverlayClick}>
      <Card className="modal-panel">
        <button
          type="button"
          className="modal-close"
          onClick={() => onClose?.()}
          aria-label="Stäng"
        >
          <CloseIcon width={18} height={18} />
        </button>
        {children}
      </Card>
    </div>
  );
}