import { api } from "./client";
import type {
  AnalyticsDto,
  DashboardCategoryDto,
  DashboardCommentDto,
  DashboardPostDetailDto,
  DashboardPostListResponse,
  OverviewDto,
  UpsertCategoryRequest,
  UpsertPostRequest,
} from "./types";

export const dashboardApi = {
  overview: () => api.get<OverviewDto>("/dashboard/overview"),
  analytics: () => api.get<AnalyticsDto>("/dashboard/analytics"),

  categories: () => api.get<DashboardCategoryDto[]>("/dashboard/categories"),
  createCategory: (data: UpsertCategoryRequest) => api.post<DashboardCategoryDto>("/dashboard/categories", data),
  updateCategory: (id: number, data: UpsertCategoryRequest) =>
    api.put<DashboardCategoryDto>(`/dashboard/categories/${id}`, data),

  posts: (page = 1) => api.get<DashboardPostListResponse>(`/dashboard/posts?page=${page}`),
  post: (id: number) => api.get<DashboardPostDetailDto>(`/dashboard/posts/${id}`),
  createPost: (data: UpsertPostRequest) => api.post<DashboardPostDetailDto>("/dashboard/posts", data),
  updatePost: (id: number, data: UpsertPostRequest) => api.put<DashboardPostDetailDto>(`/dashboard/posts/${id}`, data),
  deletePost: (id: number) => api.del(`/dashboard/posts/${id}`),

  comments: () => api.get<DashboardCommentDto[]>("/dashboard/comments"),
  toggleComment: (id: number) => api.post<void>(`/dashboard/comments/${id}/toggle`),
  deleteComment: (id: number) => api.del(`/dashboard/comments/${id}`),
};
