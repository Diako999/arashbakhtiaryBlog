import { api } from "./client";
import type { TestimonialDto } from "./types";

export const testimonialsApi = {
  list: (lang: string) => api.get<TestimonialDto[]>(`/testimonials?lang=${lang}`),
};
