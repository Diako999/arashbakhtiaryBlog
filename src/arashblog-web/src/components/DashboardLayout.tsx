import { useEffect, useState } from "react";
import { NavLink, Outlet, useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { authApi } from "../api/auth";

type GateStatus = "loading" | "ok";

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

  return (
    <div dir="rtl" className="flex min-h-screen bg-surface text-ink">
      <aside className="w-56 shrink-0 border-e border-line bg-card p-4">
        <div className="mb-6 px-2 text-lg font-bold text-brand">ArashBlog</div>
        <nav className="flex flex-col gap-1">
          <NavLink to="/dashboard" end className={navItemClass}>
            {t("dashboard.nav.overview")}
          </NavLink>
          <NavLink to="/dashboard/analytics" className={navItemClass}>
            {t("dashboard.nav.analytics")}
          </NavLink>
          <NavLink to="/dashboard/content" className={navItemClass}>
            {t("dashboard.nav.content")}
          </NavLink>
          <NavLink to="/dashboard/comments" className={navItemClass}>
            {t("dashboard.nav.comments")}
          </NavLink>
          <NavLink to="/dashboard/offerings" className={navItemClass}>
            {t("dashboard.nav.offerings")}
          </NavLink>
          <NavLink to="/dashboard/testimonials" className={navItemClass}>
            {t("dashboard.nav.testimonials")}
          </NavLink>
          <NavLink to="/dashboard/leads" className={navItemClass}>
            {t("dashboard.nav.leads")}
          </NavLink>
          <NavLink to="/dashboard/pages" className={navItemClass}>
            {t("dashboard.nav.pages")}
          </NavLink>
          <NavLink to="/dashboard/settings" className={navItemClass}>
            {t("dashboard.nav.settings")}
          </NavLink>
        </nav>
        <div className="mt-8 border-t border-line pt-4">
          {username && <p className="mb-2 px-2 text-sm text-ink-muted">{username}</p>}
          <button
            type="button"
            onClick={() => void handleLogout()}
            className="w-full rounded-lg px-4 py-2 text-start text-danger hover:bg-surface-soft"
          >
            {t("dashboard.nav.logout")}
          </button>
        </div>
      </aside>
      <main className="flex-1 p-8">
        <Outlet />
      </main>
    </div>
  );
}
