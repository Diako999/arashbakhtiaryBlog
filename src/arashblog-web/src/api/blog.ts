import { api } from "./client";
import type { CategoryDto, PostDetail, PostListResponse } from "./types";

export interface PostListParams {
  lang: string;
  category?: string;
  tag?: string;
  q?: string;
  page?: number;
}

function toQuery(params: Record<string, string | number | undefined>) {
  const usp = new URLSearchParams();
  for (const [key, value] of Object.entries(params)) {
    if (value !== undefined && value !== "") usp.set(key, String(value));
  }
  const qs = usp.toString();
  return qs ? `?${qs}` : "";
}

export const blogApi = {
  categories: (lang: string) => api.get<CategoryDto[]>(`/blog/categories${toQuery({ lang })}`),
  list: (params: PostListParams) => api.get<PostListResponse>(`/blog/posts${toQuery({ ...params })}`),
  detail: (slug: string, lang: string) =>
    api.get<PostDetail>(`/blog/posts/${encodeURIComponent(slug)}${toQuery({ lang })}`),
  comment: (slug: string, data: { name: string; email: string; body: string }) =>
    api.post<{ message: string }>(`/blog/posts/${encodeURIComponent(slug)}/comments`, data),
};
