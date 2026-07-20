import { useEffect } from "react";
import { Link, Outlet, useParams } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { useQuery } from "@tanstack/react-query";
import { navApi } from "../api/nav";
import { siteApi } from "../api/site";
import { defaultLanguage, supportedLanguages, type SupportedLanguage } from "../i18n";
import LanguageSwitcher from "./LanguageSwitcher";

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

  return (
    <div className="min-h-screen bg-surface text-ink">
      <header className="border-b border-line bg-card">
        <div className="mx-auto flex max-w-4xl items-center justify-between px-4 py-4">
          <Link to={`/${activeLang}/blog`} className="flex items-center gap-2 text-lg font-bold text-brand no-underline">
            {siteSettings?.logoUrl && <img src={siteSettings.logoUrl} alt="" className="h-8 w-8 rounded object-cover" />}
            {siteSettings?.siteName ?? "ArashBlog"}
          </Link>
          <nav className="flex items-center gap-4">
            {navItems?.map((item) => (
              <Link
                key={item.key}
                to={`/${activeLang}${item.path}`}
                className="text-ink no-underline hover:text-brand"
              >
                {item.title}
              </Link>
            ))}
            <LanguageSwitcher activeLang={activeLang} />
          </nav>
        </div>
      </header>
      <main className="mx-auto max-w-4xl px-4 py-8">
        <Outlet />
      </main>
    </div>
  );
}
