import { useEffect, useState } from "react";
import { Link, Outlet, useParams } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { useQuery } from "@tanstack/react-query";
import { Menu, X } from "lucide-react";
import { navApi } from "../api/nav";
import { siteApi } from "../api/site";
import { defaultLanguage, supportedLanguages, type SupportedLanguage } from "../i18n";
import LanguageSwitcher from "./LanguageSwitcher";
import Footer from "./Footer";
import ThemeToggle from "./ThemeToggle";

function isSupported(lang: string | undefined): lang is SupportedLanguage {
  return !!lang && (supportedLanguages as readonly string[]).includes(lang);
}

export default function Layout() {
  const { lang } = useParams();
  const { i18n } = useTranslation();
  const activeLang: SupportedLanguage = isSupported(lang) ? lang : defaultLanguage;

  useEffect(() => {
    void i18n.changeLanguage(activeLang);
    document.documentElement.lang = activeLang;
    document.documentElement.dir = "rtl";
  }, [activeLang, i18n]);

  const { data: navItems } = useQuery({
    queryKey: ["nav", activeLang],
    queryFn: () => navApi.list(activeLang),
  });

  const { data: siteSettings } = useQuery({ queryKey: ["site-settings"], queryFn: siteApi.settings });
  const [mobileOpen, setMobileOpen] = useState(false);

  useEffect(() => {
    setMobileOpen(false);
  }, [activeLang]);

  return (
    <div className="min-h-screen bg-surface text-ink">
      <header className="sticky top-0 z-40 border-b border-line bg-card/90 backdrop-blur-md">
        <div className="mx-auto flex max-w-4xl items-center justify-between px-4 py-4">
          <Link to={`/${activeLang}`} className="flex items-center gap-2 text-brand no-underline">
            {siteSettings?.logoUrl && <img src={siteSettings.logoUrl} alt="" className="h-10 w-10 rounded object-cover" />}
            <span className="text-sm font-bold sm:text-base">{siteSettings?.siteName ?? "ArashBlog"}</span>
          </Link>
          <div className="flex items-center gap-2 sm:gap-4">
            <nav className="hidden items-center gap-4 sm:flex">
              {navItems?.map((item) => (
                <Link
                  key={item.key}
                  to={`/${activeLang}${item.path}`}
                  className="text-ink no-underline hover:text-brand"
                >
                  {item.title}
                </Link>
              ))}
            </nav>
            <LanguageSwitcher activeLang={activeLang} />
            <ThemeToggle />
            <button
              type="button"
              onClick={() => setMobileOpen((v) => !v)}
              aria-label="Menu"
              className="flex h-11 w-11 items-center justify-center rounded-lg text-ink-muted hover:bg-surface-soft hover:text-ink sm:hidden"
            >
              {mobileOpen ? <X size={20} /> : <Menu size={20} />}
            </button>
          </div>
        </div>

        <nav
          className={
            "absolute inset-x-0 top-full z-40 flex origin-top flex-col gap-1 border-b border-line bg-card/90 px-4 py-3 backdrop-blur-md transition-all duration-200 ease-out sm:hidden " +
            (mobileOpen ? "visible scale-y-100 opacity-100" : "invisible scale-y-95 opacity-0")
          }
        >
          {navItems?.map((item) => (
            <Link
              key={item.key}
              to={`/${activeLang}${item.path}`}
              onClick={() => setMobileOpen(false)}
              className="rounded-lg px-2 py-2 text-ink no-underline hover:bg-surface-soft"
            >
              {item.title}
            </Link>
          ))}
        </nav>
      </header>
      <main className="mx-auto max-w-4xl px-4 py-8">
        <Outlet />
      </main>
      <Footer activeLang={activeLang} />
    </div>
  );
}
