import { api } from "./client";
import type { CreateSubmissionRequest, LeadMagnetDetailDto, LeadMagnetSummaryDto } from "./types";

export const leadsApi = {
  list: (lang: string) => api.get<LeadMagnetSummaryDto[]>(`/leads?lang=${lang}`),
  detail: (slug: string, lang: string) =>
    api.get<LeadMagnetDetailDto>(`/leads/${encodeURIComponent(slug)}?lang=${lang}`),
  submit: (slug: string, data: CreateSubmissionRequest) =>
    api.post<{ message: string }>(`/leads/${encodeURIComponent(slug)}/submit`, data),
};
