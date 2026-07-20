export interface MeResponse {
  isAuthenticated: boolean;
  requiresOtpSetup: boolean;
  requiresOtpVerify: boolean;
  username: string | null;
}

export interface LoginResponse {
  succeeded: boolean;
  requiresOtpSetup: boolean;
  requiresOtpVerify: boolean;
}

export interface OtpSetupResponse {
  qrCodeDataUri: string;
  manualKey: string;
}

export interface OtpConfirmResponse {
  recoveryCodes: string[];
}

export interface OtpVerifyResponse {
  usedRecoveryCode: boolean;
}

export interface CategoryDto {
  slug: string;
  name: string;
}

export interface PostSummary {
  slug: string;
  title: string;
  excerpt: string;
  coverImageUrl: string | null;
  categoryName: string | null;
  categorySlug: string | null;
  tags: string[];
  publishedAt: string | null;
  authorName: string;
}

export interface PostListResponse {
  items: PostSummary[];
  page: number;
  pageSize: number;
  totalCount: number;
}

export interface CommentDto {
  id: number;
  name: string;
  body: string;
  createdAt: string;
}

export interface PostDetail extends PostSummary {
  bodyHtml: string;
  viewCount: number;
  comments: CommentDto[];
}

export interface NavItemDto {
  key: string;
  title: string;
  path: string;
}

export interface OverviewDto {
  draftCount: number;
  publishedCount: number;
  recentPosts: PostSummary[];
}

export interface TopPostDto {
  slug: string;
  title: string;
  viewCount: number;
  barPct: number;
}

export interface CategoryStatDto {
  categoryName: string | null;
  totalViews: number;
  barPct: number;
}

export interface AnalyticsDto {
  totalViews: number;
  postCount: number;
  avgViews: number;
  topPosts: TopPostDto[];
  categoryStats: CategoryStatDto[];
}

export interface DashboardCategoryDto {
  id: number;
  slug: string;
  nameFa: string;
  nameCkb: string;
}

export interface UpsertCategoryRequest {
  slug: string;
  nameFa: string;
  nameCkb: string;
}

export interface DashboardPostListItemDto {
  id: number;
  titleFa: string;
  slug: string;
  categoryName: string | null;
  status: "Draft" | "Published";
  createdAt: string;
}

export interface DashboardPostListResponse {
  items: DashboardPostListItemDto[];
  page: number;
  pageSize: number;
  totalCount: number;
}

export interface DashboardPostDetailDto {
  id: number;
  slug: string;
  categoryId: number | null;
  tags: string;
  coverImageUrl: string | null;
  status: "Draft" | "Published";
  publishedAt: string | null;
  titleFa: string;
  titleCkb: string;
  excerptFa: string;
  excerptCkb: string;
  bodyFa: string;
  bodyCkb: string;
  metaTitleFa: string;
  metaTitleCkb: string;
  metaDescriptionFa: string;
  metaDescriptionCkb: string;
}

export type UpsertPostRequest = Omit<DashboardPostDetailDto, "id">;

export interface DashboardCommentDto {
  id: number;
  postId: number;
  postTitle: string;
  name: string;
  email: string;
  body: string;
  isApproved: boolean;
  createdAt: string;
}
