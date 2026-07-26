import { Link } from "react-router-dom";
import { useTranslation } from "react-i18next";
import type { NavItemDto, SiteSettingDto } from "../api/types";
import type { SupportedLanguage } from "../i18n";
import Logo from "./Logo";

type SocialLink = { url: string; label: string; path: string };

// Minimal monochrome glyphs (currentColor) — no icon library installed
// anywhere in this project, matching ThemeToggleButton's precedent.
const socialIconPaths: Record<string, string> = {
  instagram:
    "M12 2c2.7 0 3 .01 4.12.06 1.11.05 1.87.23 2.53.49.68.27 1.26.62 1.83 1.19.57.57.92 1.15 1.19 1.83.26.66.44 1.42.49 2.53C22.21 9 22.22 9.3 22.22 12s-.01 3-.06 4.12c-.05 1.11-.23 1.87-.49 2.53a5.08 5.08 0 0 1-1.19 1.83 5.08 5.08 0 0 1-1.83 1.19c-.66.26-1.42.44-2.53.49-1.12.05-1.42.06-4.12.06s-3-.01-4.12-.06c-1.11-.05-1.87-.23-2.53-.49a5.08 5.08 0 0 1-1.83-1.19 5.08 5.08 0 0 1-1.19-1.83c-.26-.66-.44-1.42-.49-2.53C1.79 15 1.78 14.7 1.78 12s.01-3 .06-4.12c.05-1.11.23-1.87.49-2.53.27-.68.62-1.26 1.19-1.83a5.08 5.08 0 0 1 1.83-1.19c.66-.26 1.42-.44 2.53-.49C9 2.01 9.3 2 12 2zm0 1.8c-2.66 0-2.97.01-4.02.06-.97.04-1.5.21-1.85.35-.47.18-.8.4-1.15.75-.35.35-.57.68-.75 1.15-.14.35-.31.88-.35 1.85-.05 1.05-.06 1.36-.06 4.02s.01 2.97.06 4.02c.04.97.21 1.5.35 1.85.18.47.4.8.75 1.15.35.35.68.57 1.15.75.35.14.88.31 1.85.35 1.05.05 1.36.06 4.02.06s2.97-.01 4.02-.06c.97-.04 1.5-.21 1.85-.35.47-.18.8-.4 1.15-.75.35-.35.57-.68.75-1.15.14-.35.31-.88.35-1.85.05-1.05.06-1.36.06-4.02s-.01-2.97-.06-4.02c-.04-.97-.21-1.5-.35-1.85a3.1 3.1 0 0 0-.75-1.15 3.1 3.1 0 0 0-1.15-.75c-.35-.14-.88-.31-1.85-.35-1.05-.05-1.36-.06-4.02-.06zm0 3.5a4.7 4.7 0 1 1 0 9.4 4.7 4.7 0 0 1 0-9.4zm0 1.8a2.9 2.9 0 1 0 0 5.8 2.9 2.9 0 0 0 0-5.8zm4.88-1.99a1.1 1.1 0 1 1 0 2.2 1.1 1.1 0 0 1 0-2.2z",
  telegram:
    "M21.9 4.2 2.9 11.6c-1.3.5-1.3 1.2-.2 1.6l4.9 1.5 1.9 5.8c.2.6.4.8.9.8.4 0 .6-.2.9-.5l2.2-2.1 4.6 3.4c.8.5 1.4.2 1.6-.8l3-13.9c.3-1.3-.5-1.9-1.8-1.6zM8.4 14.3l9.3-5.8c.5-.3.9-.1.6.3l-8 7.4-.3 3.2-1.6-5.1z",
  twitter:
    "M18.9 2H22l-7 8 8.2 12h-6.4l-5-6.6L5.9 22H2.8l7.5-8.6L2.5 2h6.6l4.5 6zm-1.1 18h1.7L7.3 3.9H5.5z",
  linkedin:
    "M6.94 5a2 2 0 1 1-4 0 2 2 0 0 1 4 0zM3.3 8.8h3.3V21H3.3zm6.6 0h3.16v1.67h.05c.44-.83 1.52-1.7 3.12-1.7 3.34 0 3.96 2.2 3.96 5.05V21h-3.3v-5.53c0-1.32-.02-3.02-1.84-3.02-1.84 0-2.12 1.44-2.12 2.93V21H9.9z",
  whatsapp:
    "M17 14.4c-.3-.1-1.6-.8-1.8-.9-.2-.1-.4-.1-.6.1-.2.3-.7.9-.8 1-.2.2-.3.2-.5.1-.3-.1-1.2-.4-2.2-1.4-.8-.7-1.4-1.6-1.5-1.9-.2-.3 0-.5.1-.6.1-.1.3-.3.4-.5.1-.1.2-.3.3-.4.1-.2 0-.4 0-.5C10.3 9.2 9.8 8 9.6 7.5c-.2-.4-.4-.4-.6-.4h-.5c-.2 0-.5.1-.7.3-.3.3-1 1-1 2.4s1 2.8 1.2 3c.1.2 2 3 4.8 4.3.7.3 1.2.5 1.6.6.7.2 1.3.2 1.8.1.5-.1 1.6-.7 1.9-1.3.2-.6.2-1.1.2-1.2-.1-.2-.3-.3-.6-.4zM12 2a10 10 0 0 0-8.6 15L2 22l5.1-1.3A10 10 0 1 0 12 2zm0 18.2a8.2 8.2 0 0 1-4.2-1.1l-.3-.2-3 .8.8-2.9-.2-.3a8.2 8.2 0 1 1 6.9 3.7z",
};

export default function Footer({
  activeLang,
  navItems,
  siteSettings,
}: {
  activeLang: SupportedLanguage;
  navItems: NavItemDto[] | undefined;
  siteSettings: SiteSettingDto | undefined;
}) {
  const { t } = useTranslation();

  const socialLinks: SocialLink[] = siteSettings
    ? [
        { url: siteSettings.instagramUrl, label: "Instagram", path: socialIconPaths.instagram },
        { url: siteSettings.telegramUrl, label: "Telegram", path: socialIconPaths.telegram },
        { url: siteSettings.twitterUrl, label: "Twitter / X", path: socialIconPaths.twitter },
        { url: siteSettings.linkedinUrl, label: "LinkedIn", path: socialIconPaths.linkedin },
        { url: siteSettings.whatsappUrl, label: "WhatsApp", path: socialIconPaths.whatsapp },
      ].filter((link) => link.url.trim() !== "")
    : [];

  return (
    <footer className="glass-surface mt-16 border-t border-line">
      <div className="mx-auto flex max-w-6xl flex-col gap-6 px-4 py-10">
        <div className="flex flex-wrap items-center justify-between gap-6">
          <Link to={`/${activeLang}`} className="flex items-center gap-2 text-lg font-bold text-brand no-underline">
            {siteSettings?.logoUrl ? (
              <img src={siteSettings.logoUrl} alt="" className="h-8 w-8 rounded object-cover" />
            ) : (
              <Logo size={32} />
            )}
            {siteSettings?.siteName ?? "ArashBlog"}
          </Link>

          <nav className="flex flex-wrap items-center gap-4">
            <Link to={`/${activeLang}`} className="text-sm text-ink-muted no-underline hover:text-brand">
              {t("nav.home")}
            </Link>
            {navItems?.map((item) => (
              <Link
                key={item.key}
                to={`/${activeLang}${item.path}`}
                className="text-sm text-ink-muted no-underline hover:text-brand"
              >
                {item.title}
              </Link>
            ))}
          </nav>

          {socialLinks.length > 0 && (
            <div className="flex items-center gap-3">
              {socialLinks.map((link) => (
                <a
                  key={link.label}
                  href={link.url}
                  target="_blank"
                  rel="noopener noreferrer"
                  aria-label={link.label}
                  className="flex h-11 w-11 items-center justify-center rounded-full border border-line text-ink-faint no-underline transition-colors hover:border-brand hover:text-brand"
                >
                  <svg viewBox="0 0 24 24" width={18} height={18} fill="currentColor">
                    <path d={link.path} />
                  </svg>
                </a>
              ))}
            </div>
          )}
        </div>

        <p className="text-xs text-ink-faint">
          © {new Date().getFullYear()} {siteSettings?.siteName ?? "ArashBlog"} — {t("footer.rights")}
        </p>
      </div>
    </footer>
  );
}
