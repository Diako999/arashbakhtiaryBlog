import { useTranslation } from "react-i18next";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { dashboardApi } from "../../api/dashboard";

// Mirrors the Django dashboard's Pages screen — the admin-facing half of
// the phased-rollout mechanism. "blog" never appears here since it's
// always live, not part of the phased rollout.
export default function PagesVisibility() {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const { data, isLoading } = useQuery({ queryKey: ["dashboard-nav-items"], queryFn: dashboardApi.navItems });

  const toggleMutation = useMutation({
    mutationFn: (id: number) => dashboardApi.toggleNavItem(id),
    onSuccess: () => void queryClient.invalidateQueries({ queryKey: ["dashboard-nav-items"] }),
  });

  return (
    <div>
      <h1 className="mb-6 text-2xl font-bold">{t("dashboard.pages.title")}</h1>
      <p className="mb-6 max-w-lg text-sm text-ink-muted">{t("dashboard.pages.description")}</p>

      {isLoading && <p>{t("common.loading")}</p>}

      <div className="flex flex-col gap-2">
        {data?.map((item) => (
          <div key={item.id} className="flex items-center justify-between rounded-lg border border-line bg-card p-4">
            <span className="font-bold">{item.title}</span>
            <button
              type="button"
              onClick={() => toggleMutation.mutate(item.id)}
              className={`rounded-full px-4 py-1.5 text-sm font-bold ${
                item.isVisible ? "bg-brand text-white" : "bg-surface-soft text-ink-muted"
              }`}
            >
              {item.isVisible ? t("dashboard.pages.published") : t("dashboard.pages.hidden")}
            </button>
          </div>
        ))}
      </div>
    </div>
  );
}
