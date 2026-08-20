import { api } from "./client";
import type { HeroSlideDto, LandingPageSettingsDto, SiteSettingDto, ThemeDto } from "./types";

export const siteApi = {
  settings: () => api.get<SiteSettingDto>("/site/settings"),
  theme: () => api.get<ThemeDto>("/site/theme"),
  landing: (lang: string) => api.get<LandingPageSettingsDto>(`/site/landing?lang=${lang}`),
  heroSlides: () => api.get<HeroSlideDto[]>("/site/hero-slides"),
};
