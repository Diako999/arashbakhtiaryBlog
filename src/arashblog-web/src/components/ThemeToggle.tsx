import { useEffect, useState } from "react";
import { Moon, Sun } from "lucide-react";

type Mode = "light" | "dark";

function currentMode(): Mode {
  return document.documentElement.getAttribute("data-theme") === "dark" ? "dark" : "light";
}

export default function ThemeToggle() {
  const [mode, setMode] = useState<Mode>(currentMode);

  // ThemeProvider sets data-theme asynchronously once the DB-driven default
  // mode loads, which can happen after this button has already mounted —
  // watch the attribute instead of only reading it once, so the icon stays
  // correct without this component owning the source of truth.
  useEffect(() => {
    const observer = new MutationObserver(() => setMode(currentMode()));
    observer.observe(document.documentElement, { attributes: true, attributeFilter: ["data-theme"] });
    return () => observer.disconnect();
  }, []);

  function toggle() {
    const next: Mode = currentMode() === "dark" ? "light" : "dark";
    document.documentElement.setAttribute("data-theme", next);
    localStorage.setItem("theme", next);
  }

  return (
    <button
      type="button"
      onClick={toggle}
      aria-label="Toggle theme"
      className="flex h-11 w-11 items-center justify-center rounded-lg text-ink-muted hover:bg-surface-soft hover:text-ink"
    >
      {mode === "dark" ? <Sun size={18} /> : <Moon size={18} />}
    </button>
  );
}
