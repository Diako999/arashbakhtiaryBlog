import { useEffect, type ReactNode } from "react";
import { useQuery } from "@tanstack/react-query";
import { siteApi } from "../api/site";

// Applies the DB-driven brand/accent colors + default light/dark mode
// site-wide (dashboard included) — the dynamic half of what M1 hardcoded
// as static CSS defaults. Runs once at the app root rather than being
// duplicated in both the public Layout and DashboardLayout.
//
// Known limitation vs. the Django project's inline pre-hydration <script>:
// this SPA has no way to apply the *fetched* brand/accent colors before
// first paint, only the default-mode dark/light flash is avoided (via the
// inline script in index.html reading localStorage synchronously). A
// brief color flash on first load is an accepted tradeoff here, not a bug
// to chase down with full SSR just for this.
export default function ThemeProvider({ children }: { children: ReactNode }) {
  const { data: theme } = useQuery({ queryKey: ["site-theme"], queryFn: siteApi.theme });

  useEffect(() => {
    if (!theme) return;

    document.documentElement.style.setProperty("--brand", theme.brandColor);
    document.documentElement.style.setProperty("--accent", theme.accentColor);
    document.documentElement.setAttribute("data-font", theme.fontChoice);
    document.documentElement.setAttribute("data-card-style", theme.cardStyle);
    document.documentElement.setAttribute("data-header-footer-style", theme.headerFooterStyle);

    if (!localStorage.getItem("theme")) {
      document.documentElement.setAttribute("data-theme", theme.defaultMode.toLowerCase());
    }
  }, [theme]);

  return children;
}
