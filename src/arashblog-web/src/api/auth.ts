import { api } from "./client";
import type { LoginResponse, MeResponse, OtpConfirmResponse, OtpSetupResponse, OtpVerifyResponse } from "./types";

export const authApi = {
  me: () => api.get<MeResponse>("/auth/me"),
  login: (username: string, password: string) => api.post<LoginResponse>("/auth/login", { username, password }),
  logout: () => api.post<void>("/auth/logout"),
  otpSetup: () => api.get<OtpSetupResponse>("/auth/otp/setup"),
  otpSetupConfirm: (code: string) => api.post<OtpConfirmResponse>("/auth/otp/setup/confirm", { code }),
  otpVerify: (code: string) => api.post<OtpVerifyResponse>("/auth/otp/verify", { code }),
  regenerateRecoveryCodes: () => api.post<OtpConfirmResponse>("/auth/otp/recovery-codes/regenerate"),
};
