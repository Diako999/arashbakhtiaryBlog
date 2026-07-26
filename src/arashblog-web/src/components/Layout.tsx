import { useEffect, useState } from "react";
import { Link, Outlet, useParams } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { useQuery } from "@tanstack/react-query";
import { navApi } from "../api/nav";
import { siteApi } from "../api/site";
import { defaultLanguage, supportedLanguages, type SupportedLanguage } from "../i18n";
import Footer from "./Footer";
import LanguageSwitcher from "./LanguageSwitcher";
import Logo from "./Logo";
import ThemeToggleButton from "./ThemeToggleButton";

function isSupported(lang: string | undefined): lang is SupportedLanguage {
  return !!lang && (supportedLanguages as readonly string[]).includes(lang);
}

export default function Layout() {
  const { lang } = useParams();
  const { t, i18n } = useTranslation();
  const activeLang: SupportedLanguage = isSupported(lang) ? lang : defaultLanguage;
  const [menuOpen, setMenuOpen] = useState(false);

  useEffect(() => {
    void i18n.changeLanguage(activeLang);
    document.documentElement.lang = activeLang;
    document.documentElement.dir = "rtl";
  }, [activeLang, i18n]);

  useEffect(() => {
    setMenuOpen(false);
  }, [activeLang]);

  const { data: navItems } = useQuery({
    queryKey: ["nav", activeLang],
    queryFn: () => navApi.list(activeLang),
  });

  const { data: siteSettings } = useQuery({ queryKey: ["site-settings"], queryFn: siteApi.settings });

  const navLinks = (
    <>
      <Link to={`/${activeLang}`} className="text-ink no-underline hover:text-brand">
        {t("nav.home")}
      </Link>
      {navItems?.map((item) => (
        <Link key={item.key} to={`/${activeLang}${item.path}`} className="text-ink no-underline hover:text-brand">
          {item.title}
        </Link>
      ))}
    </>
  );

  return (
    <div className="flex min-h-dvh flex-col bg-surface text-ink">
      <header className="glass-surface sticky top-0 z-30 border-b border-line">
        <div className="mx-auto flex max-w-6xl items-center justify-between gap-4 px-4 py-4">
          <Link to={`/${activeLang}`} className="flex items-center gap-2 text-lg font-bold text-brand no-underline">
            {siteSettings?.logoUrl ? (
              <img src={siteSettings.logoUrl} alt="" className="h-8 w-8 rounded object-cover" />
            ) : (
              <Logo size={32} />
            )}
            {siteSettings?.siteName ?? "ArashBlog"}
          </Link>

          <nav className="hidden items-center gap-4 md:flex">
            {navLinks}
            <LanguageSwitcher activeLang={activeLang} />
            <ThemeToggleButton />
          </nav>

          <div className="flex items-center gap-2 md:hidden">
            <ThemeToggleButton />
            <button
              type="button"
              onClick={() => setMenuOpen((open) => !open)}
              aria-label="Menu"
              aria-expanded={menuOpen}
              className="flex h-11 w-11 items-center justify-center rounded-[10px] border border-line bg-card"
            >
              <svg viewBox="0 0 24 24" width={20} height={20} fill="none" stroke="currentColor" strokeWidth={2}>
                {menuOpen ? <path d="M6 6l12 12M18 6L6 18" /> : <path d="M4 7h16M4 12h16M4 17h16" />}
              </svg>
            </button>
          </div>
        </div>

        <div
          aria-hidden={!menuOpen}
          className={`glass-surface absolute inset-x-0 top-full z-40 flex flex-col gap-4 overflow-hidden border-b border-line px-4 transition-all duration-300 ease-out md:hidden ${
            menuOpen ? "max-h-[420px] translate-y-0 py-5 opacity-100" : "pointer-events-none max-h-0 -translate-y-2 py-0 opacity-0"
          }`}
        >
          {navLinks}
          <LanguageSwitcher activeLang={activeLang} />
        </div>
      </header>
      <main className="mx-auto max-w-6xl flex-1 px-5 py-8 sm:px-8 lg:px-10">
        <Outlet />
      </main>
      <Footer activeLang={activeLang} navItems={navItems} siteSettings={siteSettings} />
    </div>
  );
}
