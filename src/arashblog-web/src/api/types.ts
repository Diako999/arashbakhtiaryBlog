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
