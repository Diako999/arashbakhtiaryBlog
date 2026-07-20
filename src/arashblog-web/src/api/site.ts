import { api } from "./client";
import type { SiteSettingDto, ThemeDto } from "./types";

export const siteApi = {
  settings: () => api.get<SiteSettingDto>("/site/settings"),
  theme: () => api.get<ThemeDto>("/site/theme"),
};
