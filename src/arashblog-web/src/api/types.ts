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

// --- Offerings ---

export interface SessionDto {
  id: number;
  startsAt: string;
  endsAt: string | null;
  location: string;
  capacity: number | null;
}

export interface OfferingSummaryDto {
  slug: string;
  title: string;
  summary: string;
  coverImageUrl: string | null;
  price: number | null;
}

export interface OfferingDetailDto extends OfferingSummaryDto {
  bodyHtml: string;
  sessions: SessionDto[];
}

export interface CreateEnrollmentRequest {
  sessionId: number | null;
  name: string;
  email: string;
  phone: string;
  notes: string;
}

export interface DashboardSessionDto {
  id: number | null;
  startsAt: string;
  endsAt: string | null;
  location: string;
  capacity: number | null;
}

export interface DashboardOfferingListItemDto {
  id: number;
  titleFa: string;
  slug: string;
  price: number | null;
  status: "Draft" | "Published";
  createdAt: string;
}

export interface DashboardOfferingListResponse {
  items: DashboardOfferingListItemDto[];
}

export interface DashboardEnrollmentDto {
  id: number;
  name: string;
  email: string;
  phone: string;
  sessionLabel: string | null;
  createdAt: string;
}

export interface DashboardOfferingDetailDto {
  id: number;
  slug: string;
  coverImageUrl: string | null;
  price: number | null;
  status: "Draft" | "Published";
  titleFa: string;
  titleCkb: string;
  summaryFa: string;
  summaryCkb: string;
  bodyFa: string;
  bodyCkb: string;
  metaTitleFa: string;
  metaTitleCkb: string;
  metaDescriptionFa: string;
  metaDescriptionCkb: string;
  sessions: DashboardSessionDto[];
  enrollments: DashboardEnrollmentDto[];
}

export type UpsertOfferingRequest = Omit<DashboardOfferingDetailDto, "id" | "enrollments">;

// --- Testimonials ---

export interface TestimonialDto {
  authorName: string;
  authorRole: string;
  quote: string;
  photoUrl: string | null;
  videoUrl: string;
}

export interface DashboardTestimonialDto {
  id: number;
  authorName: string;
  authorRoleFa: string;
  authorRoleCkb: string;
  quoteFa: string;
  quoteCkb: string;
  photoUrl: string | null;
  videoUrl: string;
  offeringId: number | null;
  isApproved: boolean;
  order: number;
}

export type UpsertTestimonialRequest = Omit<DashboardTestimonialDto, "id" | "isApproved" | "order">;

// --- Leads ---

export interface LeadMagnetSummaryDto {
  slug: string;
  title: string;
  description: string;
  coverImageUrl: string | null;
}

export interface LeadMagnetDetailDto extends LeadMagnetSummaryDto {
  fileUrl: string;
}

export interface CreateSubmissionRequest {
  name: string;
  email: string;
}

export interface DashboardLeadMagnetListItemDto {
  id: number;
  titleFa: string;
  slug: string;
  status: "Draft" | "Published";
  createdAt: string;
}

export interface DashboardLeadMagnetDetailDto {
  id: number;
  slug: string;
  coverImageUrl: string | null;
  fileUrl: string;
  status: "Draft" | "Published";
  titleFa: string;
  titleCkb: string;
  descriptionFa: string;
  descriptionCkb: string;
  metaTitleFa: string;
  metaTitleCkb: string;
  metaDescriptionFa: string;
  metaDescriptionCkb: string;
}

export type UpsertLeadMagnetRequest = Omit<DashboardLeadMagnetDetailDto, "id">;

export interface DashboardSubmissionDto {
  id: number;
  name: string;
  email: string;
  leadMagnetTitle: string;
  isContacted: boolean;
  createdAt: string;
}

// --- Pages visibility ---

export interface DashboardNavItemDto {
  id: number;
  key: string;
  title: string;
  isVisible: boolean;
}

// --- Flat pages (about/contact) ---

export interface FlatPageDto {
  slug: string;
  title: string;
  bodyHtml: string;
}

export interface DashboardFlatPageDto {
  id: number;
  slug: string;
  titleFa: string;
  titleCkb: string;
  bodyFa: string;
  bodyCkb: string;
  metaTitleFa: string;
  metaTitleCkb: string;
  metaDescriptionFa: string;
  metaDescriptionCkb: string;
}

export type UpsertFlatPageRequest = Omit<DashboardFlatPageDto, "id" | "slug">;

// --- Site settings + theme ---

export interface SiteSettingDto {
  siteName: string;
  logoUrl: string | null;
  contactEmail: string;
  contactPhone: string;
  instagramUrl: string;
  telegramUrl: string;
  twitterUrl: string;
  linkedinUrl: string;
  whatsappUrl: string;
  metaDescription: string;
}

export type UpsertSiteSettingRequest = SiteSettingDto;

export interface ThemeDto {
  brandColor: string;
  accentColor: string;
  defaultMode: "Light" | "Dark";
}

export type UpsertThemeRequest = ThemeDto;
