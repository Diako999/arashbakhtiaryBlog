import { api } from "./client";
import type { CreateEnrollmentRequest, OfferingDetailDto, OfferingSummaryDto } from "./types";

export const offeringsApi = {
  list: (lang: string) => api.get<OfferingSummaryDto[]>(`/offerings?lang=${lang}`),
  detail: (slug: string, lang: string) =>
    api.get<OfferingDetailDto>(`/offerings/${encodeURIComponent(slug)}?lang=${lang}`),
  enroll: (slug: string, data: CreateEnrollmentRequest) =>
    api.post<{ message: string }>(`/offerings/${encodeURIComponent(slug)}/enroll`, data),
};
