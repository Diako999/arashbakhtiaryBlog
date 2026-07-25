import { useCallback, useState } from "react";

type Theme = "light" | "dark";

function readCurrentTheme(): Theme {
  return document.documentElement.getAttribute("data-theme") === "dark" ? "dark" : "light";
}

// The missing write-path: index.html's pre-hydration script and
// ThemeProvider both only ever READ localStorage/set the DB default — until
// now nothing ever let a visitor override it. document.documentElement is
// already the single source of truth (no other component needs to
// reactively read theme state), so a plain hook with local state is enough
// — no need to lift this into ThemeProvider's context.
export default function useTheme() {
  const [theme, setTheme] = useState<Theme>(() => readCurrentTheme());

  const toggle = useCallback(() => {
    const next: Theme = readCurrentTheme() === "dark" ? "light" : "dark";
    document.documentElement.setAttribute("data-theme", next);
    localStorage.setItem("theme", next);
    setTheme(next);
  }, []);

  return { theme, toggle };
}
