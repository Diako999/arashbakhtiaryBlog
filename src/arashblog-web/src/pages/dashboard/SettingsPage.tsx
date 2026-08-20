import { useEffect, useState, type FormEvent } from "react";
import { useTranslation } from "react-i18next";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { dashboardApi } from "../../api/dashboard";
import type { DashboardLandingPageSettingsDto, SiteSettingDto, ThemeDto } from "../../api/types";
import FileUploadField from "../../components/FileUploadField";
import { useSeo } from "../../hooks/useSeo";
import HeroSlidesManager from "./HeroSlidesManager";

const emptySite: SiteSettingDto = {
  siteName: "",
  logoUrl: "",
  contactEmail: "",
  contactPhone: "",
  instagramUrl: "",
  telegramUrl: "",
  twitterUrl: "",
  linkedinUrl: "",
  whatsappUrl: "",
  youtubeUrl: "",
  metaDescription: "",
};

const emptyTheme: ThemeDto = { brandColor: "#0f9d8e", accentColor: "#f0b429", defaultMode: "Dark" };

const emptyLanding: DashboardLandingPageSettingsDto = {
  heroBadgeFa: "",
  heroBadgeCkb: "",
  heroSubtitleFa: "",
  heroSubtitleCkb: "",
  heroDescriptionFa: "",
  heroDescriptionCkb: "",
  aboutRoleFa: "",
  aboutRoleCkb: "",
  aboutBioFa: "",
  aboutBioCkb: "",
  aboutPhotoUrl: "",
  aboutGithubUrl: "",
  aboutYoutubeUrl: "",
};

export default function SettingsPage() {
  const { t } = useTranslation();
  useSeo({ title: t("dashboard.nav.settings") });
  const queryClient = useQueryClient();

  const [siteForm, setSiteForm] = useState<SiteSettingDto>(emptySite);
  const [siteSaved, setSiteSaved] = useState(false);
  const [themeForm, setThemeForm] = useState<ThemeDto>(emptyTheme);
  const [themeError, setThemeError] = useState(false);
  const [themeSaved, setThemeSaved] = useState(false);
  const [landingForm, setLandingForm] = useState<DashboardLandingPageSettingsDto>(emptyLanding);
  const [landingSaved, setLandingSaved] = useState(false);

  const { data: site } = useQuery({ queryKey: ["dashboard-site-setting"], queryFn: dashboardApi.siteSetting });
  const { data: theme } = useQuery({ queryKey: ["dashboard-theme"], queryFn: dashboardApi.theme });
  const { data: landing } = useQuery({ queryKey: ["dashboard-landing-settings"], queryFn: dashboardApi.landingSettings });

  useEffect(() => {
    if (site) setSiteForm(site);
  }, [site]);

  useEffect(() => {
    if (theme) setThemeForm(theme);
  }, [theme]);

  useEffect(() => {
    if (landing) setLandingForm(landing);
  }, [landing]);

  async function handleSiteSubmit(e: FormEvent) {
    e.preventDefault();
    await dashboardApi.updateSiteSetting(siteForm);
    setSiteSaved(true);
    void queryClient.invalidateQueries({ queryKey: ["site-settings"] });
  }

  async function handleThemeSubmit(e: FormEvent) {
    e.preventDefault();
    setThemeError(false);
    try {
      await dashboardApi.updateTheme(themeForm);
      setThemeSaved(true);
      void queryClient.invalidateQueries({ queryKey: ["site-theme"] });
    } catch {
      setThemeError(true);
    }
  }

  async function handleLandingSubmit(e: FormEvent) {
    e.preventDefault();
    await dashboardApi.updateLandingSettings(landingForm);
    setLandingSaved(true);
    void queryClient.invalidateQueries({ predicate: (q) => q.queryKey[0] === "landing-settings" });
  }

  const inputClass = "w-full rounded-lg border border-line bg-card px-3 py-2";
  const labelClass = "mb-1 block text-sm text-ink-muted";

  return (
    <div className="flex max-w-2xl flex-col gap-10">
      <div>
        <h1 className="mb-4 text-2xl font-bold">{t("dashboard.settings.siteTitle")}</h1>
        <form onSubmit={(e) => void handleSiteSubmit(e)} className="grid grid-cols-1 gap-4 sm:grid-cols-2">
          <div className="sm:col-span-2">
            <label className={labelClass}>{t("dashboard.settings.siteName")}</label>
            <input required value={siteForm.siteName} onChange={(e) => setSiteForm({ ...siteForm, siteName: e.target.value })} className={inputClass} />
          </div>
          <div className="sm:col-span-2">
            <FileUploadField
              kind="image"
              label={t("dashboard.settings.logoUrl")}
              value={siteForm.logoUrl}
              onChange={(url) => setSiteForm({ ...siteForm, logoUrl: url })}
            />
          </div>
          <div>
            <label className={labelClass}>{t("dashboard.settings.contactEmail")}</label>
            <input dir="ltr" value={siteForm.contactEmail} onChange={(e) => setSiteForm({ ...siteForm, contactEmail: e.target.value })} className={inputClass} />
          </div>
          <div>
            <label className={labelClass}>{t("dashboard.settings.contactPhone")}</label>
            <input dir="ltr" value={siteForm.contactPhone} onChange={(e) => setSiteForm({ ...siteForm, contactPhone: e.target.value })} className={inputClass} />
          </div>
          <div>
            <label className={labelClass}>{t("dashboard.settings.instagram")}</label>
            <input dir="ltr" value={siteForm.instagramUrl} onChange={(e) => setSiteForm({ ...siteForm, instagramUrl: e.target.value })} className={inputClass} />
          </div>
          <div>
            <label className={labelClass}>{t("dashboard.settings.telegram")}</label>
            <input dir="ltr" value={siteForm.telegramUrl} onChange={(e) => setSiteForm({ ...siteForm, telegramUrl: e.target.value })} className={inputClass} />
          </div>
          <div>
            <label className={labelClass}>{t("dashboard.settings.twitter")}</label>
            <input dir="ltr" value={siteForm.twitterUrl} onChange={(e) => setSiteForm({ ...siteForm, twitterUrl: e.target.value })} className={inputClass} />
          </div>
          <div>
            <label className={labelClass}>{t("dashboard.settings.linkedin")}</label>
            <input dir="ltr" value={siteForm.linkedinUrl} onChange={(e) => setSiteForm({ ...siteForm, linkedinUrl: e.target.value })} className={inputClass} />
          </div>
          <div>
            <label className={labelClass}>{t("dashboard.settings.whatsapp")}</label>
            <input dir="ltr" value={siteForm.whatsappUrl} onChange={(e) => setSiteForm({ ...siteForm, whatsappUrl: e.target.value })} className={inputClass} />
          </div>
          <div>
            <label className={labelClass}>{t("dashboard.settings.youtube")}</label>
            <input dir="ltr" value={siteForm.youtubeUrl} onChange={(e) => setSiteForm({ ...siteForm, youtubeUrl: e.target.value })} className={inputClass} />
          </div>
          <div className="sm:col-span-2">
            <label className={labelClass}>{t("dashboard.settings.metaDescription")}</label>
            <textarea rows={2} value={siteForm.metaDescription} onChange={(e) => setSiteForm({ ...siteForm, metaDescription: e.target.value })} className={inputClass} />
          </div>
          <div className="sm:col-span-2 flex items-center gap-3">
            <button type="submit" className="rounded-lg bg-brand px-5 py-2 font-bold text-white">
              {t("dashboard.postForm.save")}
            </button>
            {siteSaved && <p className="text-brand">{t("dashboard.settings.saved")}</p>}
          </div>
        </form>
      </div>

      <div>
        <h1 className="mb-4 text-2xl font-bold">{t("dashboard.settings.themeTitle")}</h1>
        <form onSubmit={(e) => void handleThemeSubmit(e)} className="grid grid-cols-1 gap-4 sm:grid-cols-2">
          <div>
            <label className={labelClass}>{t("dashboard.settings.brandColor")}</label>
            <input
              type="color"
              value={themeForm.brandColor}
              onChange={(e) => setThemeForm({ ...themeForm, brandColor: e.target.value })}
              className="h-10 w-full rounded-lg border border-line bg-card"
            />
          </div>
          <div>
            <label className={labelClass}>{t("dashboard.settings.accentColor")}</label>
            <input
              type="color"
              value={themeForm.accentColor}
              onChange={(e) => setThemeForm({ ...themeForm, accentColor: e.target.value })}
              className="h-10 w-full rounded-lg border border-line bg-card"
            />
          </div>
          <div>
            <label className={labelClass}>{t("dashboard.settings.defaultMode")}</label>
            <select
              value={themeForm.defaultMode}
              onChange={(e) => setThemeForm({ ...themeForm, defaultMode: e.target.value as "Light" | "Dark" })}
              className={inputClass}
            >
              <option value="Light">{t("dashboard.settings.light")}</option>
              <option value="Dark">{t("dashboard.settings.dark")}</option>
            </select>
          </div>
          <div className="sm:col-span-2 flex items-center gap-3">
            <button type="submit" className="rounded-lg bg-brand px-5 py-2 font-bold text-white">
              {t("dashboard.postForm.save")}
            </button>
            {themeSaved && <p className="text-brand">{t("dashboard.settings.saved")}</p>}
            {themeError && <p className="text-danger">{t("dashboard.postForm.error")}</p>}
          </div>
        </form>
      </div>

      <HeroSlidesManager />

      <div>
        <h1 className="mb-4 text-2xl font-bold">{t("dashboard.settings.landingTitle")}</h1>
        <form onSubmit={(e) => void handleLandingSubmit(e)} className="grid grid-cols-1 gap-4 sm:grid-cols-2">
          <div>
            <label className={labelClass}>{t("dashboard.settings.heroBadgeFa")}</label>
            <input value={landingForm.heroBadgeFa} onChange={(e) => setLandingForm({ ...landingForm, heroBadgeFa: e.target.value })} className={inputClass} />
          </div>
          <div>
            <label className={labelClass}>{t("dashboard.settings.heroBadgeCkb")}</label>
            <input value={landingForm.heroBadgeCkb} onChange={(e) => setLandingForm({ ...landingForm, heroBadgeCkb: e.target.value })} className={inputClass} />
          </div>
          <div>
            <label className={labelClass}>{t("dashboard.settings.heroSubtitleFa")}</label>
            <input value={landingForm.heroSubtitleFa} onChange={(e) => setLandingForm({ ...landingForm, heroSubtitleFa: e.target.value })} className={inputClass} />
          </div>
          <div>
            <label className={labelClass}>{t("dashboard.settings.heroSubtitleCkb")}</label>
            <input value={landingForm.heroSubtitleCkb} onChange={(e) => setLandingForm({ ...landingForm, heroSubtitleCkb: e.target.value })} className={inputClass} />
          </div>
          <div>
            <label className={labelClass}>{t("dashboard.settings.heroDescriptionFa")}</label>
            <textarea rows={2} value={landingForm.heroDescriptionFa} onChange={(e) => setLandingForm({ ...landingForm, heroDescriptionFa: e.target.value })} className={inputClass} />
            <p className="mt-1 text-xs text-ink-faint">{t("dashboard.settings.heroDescriptionHint")}</p>
          </div>
          <div>
            <label className={labelClass}>{t("dashboard.settings.heroDescriptionCkb")}</label>
            <textarea rows={2} value={landingForm.heroDescriptionCkb} onChange={(e) => setLandingForm({ ...landingForm, heroDescriptionCkb: e.target.value })} className={inputClass} />
            <p className="mt-1 text-xs text-ink-faint">{t("dashboard.settings.heroDescriptionHint")}</p>
          </div>
          <div>
            <label className={labelClass}>{t("dashboard.settings.aboutRoleFa")}</label>
            <input value={landingForm.aboutRoleFa} onChange={(e) => setLandingForm({ ...landingForm, aboutRoleFa: e.target.value })} className={inputClass} />
          </div>
          <div>
            <label className={labelClass}>{t("dashboard.settings.aboutRoleCkb")}</label>
            <input value={landingForm.aboutRoleCkb} onChange={(e) => setLandingForm({ ...landingForm, aboutRoleCkb: e.target.value })} className={inputClass} />
          </div>
          <div>
            <label className={labelClass}>{t("dashboard.settings.aboutBioFa")}</label>
            <textarea rows={4} value={landingForm.aboutBioFa} onChange={(e) => setLandingForm({ ...landingForm, aboutBioFa: e.target.value })} className={inputClass} />
          </div>
          <div>
            <label className={labelClass}>{t("dashboard.settings.aboutBioCkb")}</label>
            <textarea rows={4} value={landingForm.aboutBioCkb} onChange={(e) => setLandingForm({ ...landingForm, aboutBioCkb: e.target.value })} className={inputClass} />
          </div>
          <div className="sm:col-span-2">
            <FileUploadField
              kind="image"
              label={t("dashboard.settings.aboutPhotoUrl")}
              value={landingForm.aboutPhotoUrl}
              onChange={(url) => setLandingForm({ ...landingForm, aboutPhotoUrl: url })}
            />
          </div>
          <div>
            <label className={labelClass}>{t("dashboard.settings.aboutGithub")}</label>
            <input dir="ltr" value={landingForm.aboutGithubUrl} onChange={(e) => setLandingForm({ ...landingForm, aboutGithubUrl: e.target.value })} className={inputClass} />
          </div>
          <div>
            <label className={labelClass}>{t("dashboard.settings.aboutYoutube")}</label>
            <input dir="ltr" value={landingForm.aboutYoutubeUrl} onChange={(e) => setLandingForm({ ...landingForm, aboutYoutubeUrl: e.target.value })} className={inputClass} />
          </div>
          <div className="sm:col-span-2 flex items-center gap-3">
            <button type="submit" className="rounded-lg bg-brand px-5 py-2 font-bold text-white">
              {t("dashboard.postForm.save")}
            </button>
            {landingSaved && <p className="text-brand">{t("dashboard.settings.saved")}</p>}
          </div>
        </form>
      </div>
    </div>
  );
}
