import { Link } from "react-router-dom";
import { useQuery } from "@tanstack/react-query";
import { useTranslation } from "react-i18next";
import { Mail, Phone } from "lucide-react";
import { InstagramIcon, LinkedinIcon, TelegramIcon, WhatsappIcon, XIcon, YoutubeIcon } from "@/components/ui/social-icons";
import { navApi } from "@/api/nav";
import { siteApi } from "@/api/site";
import type { SupportedLanguage } from "@/i18n";

export default function Footer({ activeLang }: { activeLang: SupportedLanguage }) {
  const { t } = useTranslation();
  const { data: navItems } = useQuery({ queryKey: ["nav", activeLang], queryFn: () => navApi.list(activeLang) });
  const { data: siteSettings } = useQuery({ queryKey: ["site-settings"], queryFn: siteApi.settings });

  const socialLinks = [
    { url: siteSettings?.instagramUrl, Icon: InstagramIcon, label: "Instagram" },
    { url: siteSettings?.telegramUrl, Icon: TelegramIcon, label: "Telegram" },
    { url: siteSettings?.twitterUrl, Icon: XIcon, label: "Twitter" },
    { url: siteSettings?.linkedinUrl, Icon: LinkedinIcon, label: "LinkedIn" },
    { url: siteSettings?.whatsappUrl, Icon: WhatsappIcon, label: "WhatsApp" },
    { url: siteSettings?.youtubeUrl, Icon: YoutubeIcon, label: "YouTube" },
  ].filter((s): s is typeof s & { url: string } => !!s.url);

  return (
    <footer className="border-t border-line bg-card">
      {/* Fixed LTR order regardless of page direction: logo stays visually
          leftmost, quick-links middle, contact/social rightmost — the brand
          chrome reads the same way in both fa and ckb. */}
      <div dir="ltr" className="mx-auto max-w-4xl px-4 py-10">
        <div className="grid grid-cols-1 divide-y divide-line md:grid-cols-3 md:gap-8 md:divide-y-0">
          <div className="flex flex-col items-center py-6 text-center first:pt-0 md:py-0">
            <Link to={`/${activeLang}`} className="flex items-center gap-2 text-lg font-bold text-brand no-underline">
              {siteSettings?.logoUrl && (
                <img src={siteSettings.logoUrl} alt="" className="h-8 w-8 rounded object-cover" />
              )}
              {siteSettings?.siteName ?? "ArashBlog"}
            </Link>
            {siteSettings?.metaDescription && (
              <p className="mt-2 text-sm text-ink-muted">{siteSettings.metaDescription}</p>
            )}
          </div>

          <div className="flex flex-col items-center py-6 text-center md:py-0">
            <h3 className="mb-3 text-sm font-bold text-ink">{t("footer.quickLinks")}</h3>
            <nav className="flex flex-col items-center gap-2 text-sm">
              <Link to={`/${activeLang}`} className="text-ink-muted no-underline hover:text-brand">
                {t("common.home")}
              </Link>
              {navItems?.map((item) => (
                <Link
                  key={item.key}
                  to={`/${activeLang}${item.path}`}
                  className="text-ink-muted no-underline hover:text-brand"
                >
                  {item.title}
                </Link>
              ))}
            </nav>
          </div>

          <div className="flex flex-col items-center py-6 text-center last:pb-0 md:py-0">
            <h3 className="mb-3 text-sm font-bold text-ink">{t("footer.contact")}</h3>
            {socialLinks.length > 0 && (
              <div className="mb-3 flex gap-4">
                {socialLinks.map(({ url, Icon, label }) => (
                  <a
                    key={label}
                    href={url}
                    target="_blank"
                    rel="noreferrer noopener"
                    aria-label={label}
                    className="text-ink-muted hover:text-brand"
                  >
                    <Icon size={26} />
                  </a>
                ))}
              </div>
            )}
            <div className="flex flex-col items-center gap-1.5 text-sm text-ink-muted">
              {siteSettings?.contactPhone && (
                <a href={`tel:${siteSettings.contactPhone}`} className="flex items-center gap-1.5 no-underline hover:text-brand">
                  <Phone size={14} /> {siteSettings.contactPhone}
                </a>
              )}
              {siteSettings?.contactEmail && (
                <a href={`mailto:${siteSettings.contactEmail}`} className="flex items-center gap-1.5 no-underline hover:text-brand">
                  <Mail size={14} /> {siteSettings.contactEmail}
                </a>
              )}
            </div>
          </div>
        </div>

        <div className="mt-2 border-t border-line pt-4 text-center text-xs text-ink-faint md:mt-8">
          {t("footer.rights", { year: new Date().getFullYear(), siteName: siteSettings?.siteName ?? "ArashBlog" })}
        </div>
      </div>
    </footer>
  );
}
