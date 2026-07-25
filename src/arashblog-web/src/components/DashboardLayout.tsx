import { useEffect, useState } from "react";
import { NavLink, Outlet, useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { authApi } from "../api/auth";
import Logo from "./Logo";
import ThemeToggleButton from "./ThemeToggleButton";

type GateStatus = "loading" | "ok";

const navEntries = [
  { to: "/dashboard", end: true, labelKey: "dashboard.nav.overview" },
  { to: "/dashboard/analytics", end: false, labelKey: "dashboard.nav.analytics" },
  { to: "/dashboard/content", end: false, labelKey: "dashboard.nav.content" },
  { to: "/dashboard/comments", end: false, labelKey: "dashboard.nav.comments" },
  { to: "/dashboard/offerings", end: false, labelKey: "dashboard.nav.offerings" },
  { to: "/dashboard/testimonials", end: false, labelKey: "dashboard.nav.testimonials" },
  { to: "/dashboard/leads", end: false, labelKey: "dashboard.nav.leads" },
  { to: "/dashboard/pages", end: false, labelKey: "dashboard.nav.pages" },
  { to: "/dashboard/landing", end: false, labelKey: "dashboard.nav.landing" },
  { to: "/dashboard/settings", end: false, labelKey: "dashboard.nav.settings" },
] as const;

// The real Dashboard shell (M2), replacing M1's DashboardPlaceholder.
// Reads /api/auth/me once on mount to decide where an unverified visitor
// belongs — same three-way branch (anonymous -> login, no device -> setup,
// unconfirmed session -> verify) as the Django dashboard_login_required /
// OTPRequiredMixin gate, just resolved client-side instead of via redirect.
export default function DashboardLayout() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const [status, setStatus] = useState<GateStatus>("loading");
  const [username, setUsername] = useState<string | null>(null);

  useEffect(() => {
    authApi.me().then((me) => {
      if (me.isAuthenticated && !me.requiresOtpSetup) {
        setUsername(me.username);
        setStatus("ok");
      } else if (me.requiresOtpVerify) {
        navigate("/dashboard/otp/verify", { replace: true });
      } else if (me.isAuthenticated && me.requiresOtpSetup) {
        navigate("/dashboard/otp/setup", { replace: true });
      } else {
        navigate("/dashboard/login", { replace: true });
      }
    });
  }, [navigate]);

  async function handleLogout() {
    await authApi.logout();
    navigate("/dashboard/login", { replace: true });
  }

  if (status === "loading") {
    return (
      <div dir="rtl" className="p-8 text-ink-muted">
        {t("common.loading")}
      </div>
    );
  }

  const navItemClass = ({ isActive }: { isActive: boolean }) =>
    `block rounded-lg px-4 py-2 no-underline ${isActive ? "bg-brand text-white" : "text-ink hover:bg-surface-soft"}`;
  const pillClass = ({ isActive }: { isActive: boolean }) =>
    `shrink-0 whitespace-nowrap rounded-full px-4 py-2 text-sm no-underline ${
      isActive ? "bg-brand text-white" : "border border-line bg-card text-ink"
    }`;

  return (
    <div dir="rtl" className="flex min-h-dvh bg-surface text-ink">
      <aside className="glass-surface-sidebar sticky top-0 hidden h-dvh w-56 shrink-0 flex-col border-e border-line p-4 md:flex">
        <div className="mb-6 flex items-center gap-2 px-2 text-lg font-bold text-brand">
          <Logo size={28} />
          ArashBlog
        </div>
        <nav className="flex flex-1 flex-col gap-1 overflow-y-auto">
          {navEntries.map((entry) => (
            <NavLink key={entry.to} to={entry.to} end={entry.end} className={navItemClass}>
              {t(entry.labelKey)}
            </NavLink>
          ))}
        </nav>
        <div className="mt-4 border-t border-line pt-4">
          {username && <p className="mb-2 px-2 text-sm text-ink-muted">{username}</p>}
          <div className="mb-2 flex items-center justify-between px-2">
            <ThemeToggleButton />
          </div>
          <button
            type="button"
            onClick={() => void handleLogout()}
            className="w-full rounded-lg px-4 py-2 text-start text-danger hover:bg-surface-soft"
          >
            {t("dashboard.nav.logout")}
          </button>
        </div>
      </aside>

      <div className="flex min-w-0 flex-1 flex-col">
        <nav className="flex gap-2 overflow-x-auto border-b border-line bg-card px-4 py-3 md:hidden">
          {navEntries.map((entry) => (
            <NavLink key={entry.to} to={entry.to} end={entry.end} className={pillClass}>
              {t(entry.labelKey)}
            </NavLink>
          ))}
        </nav>
        <main className="flex-1 p-4 md:p-8">
          <Outlet />
        </main>
      </div>
    </div>
  );
}
