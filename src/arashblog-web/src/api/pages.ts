import { api } from "./client";
import type { FlatPageDto } from "./types";

export const pagesApi = {
  detail: (slug: string, lang: string) => api.get<FlatPageDto>(`/pages/${encodeURIComponent(slug)}?lang=${lang}`),
};
