import { api } from "./client";
import type { NavItemDto } from "./types";

export const navApi = {
  list: (lang: string) => api.get<NavItemDto[]>(`/nav?lang=${lang}`),
};
