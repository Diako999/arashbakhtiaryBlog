import { useState, type FormEvent } from "react";
import { useTranslation } from "react-i18next";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { dashboardApi } from "../../api/dashboard";
import { authApi } from "../../api/auth";
import { ApiError } from "../../api/client";

export default function AdminUsers() {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState<string | null>(null);

  const { data: admins, isLoading } = useQuery({ queryKey: ["dashboard-admins"], queryFn: dashboardApi.admins });
  const { data: me } = useQuery({ queryKey: ["auth-me"], queryFn: authApi.me });

  const invalidate = () => void queryClient.invalidateQueries({ queryKey: ["dashboard-admins"] });

  const createMutation = useMutation({
    mutationFn: () => dashboardApi.createAdmin({ username, password }),
    onSuccess: () => {
      setUsername("");
      setPassword("");
      setError(null);
      invalidate();
    },
    onError: (err) => {
      if (err instanceof ApiError && err.body && typeof err.body === "object" && "details" in err.body) {
        setError((err.body.details as string[]).join(" "));
      } else {
        setError(t("dashboard.postForm.error"));
      }
    },
  });

  const deleteMutation = useMutation({ mutationFn: (id: string) => dashboardApi.deleteAdmin(id), onSuccess: invalidate });

  function handleSubmit(e: FormEvent) {
    e.preventDefault();
    setError(null);
    createMutation.mutate();
  }

  const inputClass = "w-full rounded-lg border border-line bg-card px-3 py-2";
  const labelClass = "mb-1 block text-sm text-ink-muted";

  return (
    <div className="max-w-2xl">
      <div className="mb-6">
        <h1 className="text-2xl font-bold">{t("dashboard.admins.title")}</h1>
        <p className="text-sm text-ink-muted">{t("dashboard.admins.description")}</p>
      </div>

      {isLoading && <p>{t("common.loading")}</p>}

      <div className="mb-8 flex flex-col gap-2">
        {admins?.map((admin) => (
          <div key={admin.id} className="card-hover-soft flex items-center justify-between border border-line bg-card p-3">
            <div>
              <span className="font-bold">{admin.userName}</span>
              {admin.userName === me?.username && (
                <span className="ms-2 rounded-full bg-brand-soft px-2 py-0.5 text-xs text-brand">{t("dashboard.admins.you")}</span>
              )}
              <span
                className={`ms-2 rounded-full px-2 py-0.5 text-xs ${
                  admin.twoFactorEnabled ? "bg-brand-soft text-brand" : "bg-surface-soft text-ink-muted"
                }`}
              >
                {admin.twoFactorEnabled ? t("dashboard.admins.otpEnabled") : t("dashboard.admins.otpPending")}
              </span>
            </div>
            {admin.userName !== me?.username && admins.length > 1 && (
              <button
                type="button"
                onClick={() => deleteMutation.mutate(admin.id)}
                className="text-sm text-danger hover:underline"
              >
                {t("dashboard.content.delete")}
              </button>
            )}
          </div>
        ))}
      </div>

      <div className="card-hover-soft border border-line bg-card p-4">
        <h2 className="mb-3 text-lg font-bold">{t("dashboard.admins.addTitle")}</h2>
        <form onSubmit={handleSubmit} className="grid grid-cols-1 gap-4 sm:grid-cols-2">
          <div>
            <label className={labelClass}>{t("dashboard.admins.username")}</label>
            <input
              required
              dir="ltr"
              value={username}
              onChange={(e) => setUsername(e.target.value)}
              className={inputClass}
            />
          </div>
          <div>
            <label className={labelClass}>{t("dashboard.admins.password")}</label>
            <input
              required
              dir="ltr"
              type="password"
              minLength={10}
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              className={inputClass}
            />
          </div>
          <div className="sm:col-span-2 flex items-center gap-3">
            <button type="submit" className="rounded-lg bg-brand px-5 py-2 font-bold text-white">
              {t("dashboard.admins.addSubmit")}
            </button>
            {error && <p className="text-danger">{error}</p>}
          </div>
        </form>
        <p className="mt-2 text-xs text-ink-faint">{t("dashboard.admins.addHint")}</p>
      </div>
    </div>
  );
}
