import { api } from "./client";
import type { LandingSectionDto } from "./types";

export const landingApi = {
  get: (lang: string) => api.get<LandingSectionDto[]>(`/landing?lang=${lang}`),
};
