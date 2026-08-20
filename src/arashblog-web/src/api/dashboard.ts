import { api } from "./client";
import type {
  AnalyticsDto,
  DashboardCategoryDto,
  DashboardCommentDto,
  DashboardFlatPageDto,
  DashboardHeroSlideDto,
  DashboardLandingPageSettingsDto,
  DashboardLeadMagnetDetailDto,
  DashboardLeadMagnetListItemDto,
  DashboardNavItemDto,
  DashboardOfferingDetailDto,
  DashboardOfferingListResponse,
  DashboardPostDetailDto,
  DashboardPostListResponse,
  DashboardSubmissionDto,
  DashboardTestimonialDto,
  OverviewDto,
  SiteSettingDto,
  ThemeDto,
  UpsertCategoryRequest,
  UpsertFlatPageRequest,
  UpsertHeroSlideRequest,
  UpsertLandingPageSettingsRequest,
  UpsertLeadMagnetRequest,
  UpsertOfferingRequest,
  UpsertPostRequest,
  UpsertSiteSettingRequest,
  UpsertTestimonialRequest,
  UpsertThemeRequest,
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

  offerings: () => api.get<DashboardOfferingListResponse>("/dashboard/offerings"),
  offering: (id: number) => api.get<DashboardOfferingDetailDto>(`/dashboard/offerings/${id}`),
  createOffering: (data: UpsertOfferingRequest) => api.post<DashboardOfferingDetailDto>("/dashboard/offerings", data),
  updateOffering: (id: number, data: UpsertOfferingRequest) =>
    api.put<DashboardOfferingDetailDto>(`/dashboard/offerings/${id}`, data),
  deleteOffering: (id: number) => api.del(`/dashboard/offerings/${id}`),

  testimonials: () => api.get<DashboardTestimonialDto[]>("/dashboard/testimonials"),
  createTestimonial: (data: UpsertTestimonialRequest) =>
    api.post<DashboardTestimonialDto>("/dashboard/testimonials", data),
  updateTestimonial: (id: number, data: UpsertTestimonialRequest) =>
    api.put<DashboardTestimonialDto>(`/dashboard/testimonials/${id}`, data),
  deleteTestimonial: (id: number) => api.del(`/dashboard/testimonials/${id}`),
  toggleTestimonialApproved: (id: number) => api.post<void>(`/dashboard/testimonials/${id}/toggle`),
  moveTestimonial: (id: number, direction: "up" | "down") =>
    api.post<void>(`/dashboard/testimonials/${id}/move/${direction}`),

  leadMagnets: () => api.get<DashboardLeadMagnetListItemDto[]>("/dashboard/leads"),
  leadMagnet: (id: number) => api.get<DashboardLeadMagnetDetailDto>(`/dashboard/leads/${id}`),
  createLeadMagnet: (data: UpsertLeadMagnetRequest) => api.post<DashboardLeadMagnetDetailDto>("/dashboard/leads", data),
  updateLeadMagnet: (id: number, data: UpsertLeadMagnetRequest) =>
    api.put<DashboardLeadMagnetDetailDto>(`/dashboard/leads/${id}`, data),
  deleteLeadMagnet: (id: number) => api.del(`/dashboard/leads/${id}`),
  submissions: () => api.get<DashboardSubmissionDto[]>("/dashboard/leads/submissions"),
  toggleSubmissionContacted: (id: number) => api.post<void>(`/dashboard/leads/submissions/${id}/toggle`),

  navItems: () => api.get<DashboardNavItemDto[]>("/dashboard/nav-items"),
  toggleNavItem: (id: number) => api.post<void>(`/dashboard/nav-items/${id}/toggle`),

  flatPages: () => api.get<DashboardFlatPageDto[]>("/dashboard/flat-pages"),
  flatPage: (id: number) => api.get<DashboardFlatPageDto>(`/dashboard/flat-pages/${id}`),
  updateFlatPage: (id: number, data: UpsertFlatPageRequest) =>
    api.put<DashboardFlatPageDto>(`/dashboard/flat-pages/${id}`, data),

  siteSetting: () => api.get<SiteSettingDto>("/dashboard/settings/site"),
  updateSiteSetting: (data: UpsertSiteSettingRequest) => api.put<SiteSettingDto>("/dashboard/settings/site", data),
  theme: () => api.get<ThemeDto>("/dashboard/settings/theme"),
  updateTheme: (data: UpsertThemeRequest) => api.put<ThemeDto>("/dashboard/settings/theme", data),
  landingSettings: () => api.get<DashboardLandingPageSettingsDto>("/dashboard/settings/landing"),
  updateLandingSettings: (data: UpsertLandingPageSettingsRequest) =>
    api.put<DashboardLandingPageSettingsDto>("/dashboard/settings/landing", data),

  heroSlides: () => api.get<DashboardHeroSlideDto[]>("/dashboard/hero-slides"),
  createHeroSlide: (data: UpsertHeroSlideRequest) => api.post<DashboardHeroSlideDto>("/dashboard/hero-slides", data),
  updateHeroSlide: (id: number, data: UpsertHeroSlideRequest) =>
    api.put<DashboardHeroSlideDto>(`/dashboard/hero-slides/${id}`, data),
  deleteHeroSlide: (id: number) => api.del(`/dashboard/hero-slides/${id}`),
  moveHeroSlide: (id: number, direction: "up" | "down") =>
    api.post<void>(`/dashboard/hero-slides/${id}/move/${direction}`),
};
