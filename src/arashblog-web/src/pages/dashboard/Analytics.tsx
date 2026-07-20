import { useTranslation } from "react-i18next";
import { useQuery } from "@tanstack/react-query";
import { dashboardApi } from "../../api/dashboard";

export default function Analytics() {
  const { t } = useTranslation();
  const { data, isLoading } = useQuery({ queryKey: ["dashboard-analytics"], queryFn: dashboardApi.analytics });

  if (isLoading) return <p>{t("common.loading")}</p>;

  return (
    <div>
      <h1 className="mb-6 text-2xl font-bold">{t("dashboard.analytics.title")}</h1>
      <div className="mb-8 grid grid-cols-3 gap-4">
        <div className="rounded-xl border border-line bg-card p-4">
          <p className="text-sm text-ink-muted">{t("dashboard.analytics.totalViews")}</p>
          <p className="text-2xl font-bold">{data?.totalViews}</p>
        </div>
        <div className="rounded-xl border border-line bg-card p-4">
          <p className="text-sm text-ink-muted">{t("dashboard.analytics.postCount")}</p>
          <p className="text-2xl font-bold">{data?.postCount}</p>
        </div>
        <div className="rounded-xl border border-line bg-card p-4">
          <p className="text-sm text-ink-muted">{t("dashboard.analytics.avgViews")}</p>
          <p className="text-2xl font-bold">{data?.avgViews}</p>
        </div>
      </div>

      <h2 className="mb-3 text-lg font-bold">{t("dashboard.analytics.topPosts")}</h2>
      <div className="mb-8 flex flex-col gap-2">
        {data?.topPosts.map((post) => (
          <div key={post.slug} className="flex items-center gap-3">
            <span className="w-40 shrink-0 truncate text-sm">{post.title}</span>
            <div className="h-3 flex-1 overflow-hidden rounded-full bg-surface-soft">
              <div className="h-full rounded-full bg-brand" style={{ width: `${post.barPct}%` }} />
            </div>
            <span className="w-12 shrink-0 text-end text-sm text-ink-muted">{post.viewCount}</span>
          </div>
        ))}
      </div>

      <h2 className="mb-3 text-lg font-bold">{t("dashboard.analytics.byCategory")}</h2>
      <div className="flex flex-col gap-2">
        {data?.categoryStats.map((row) => (
          <div key={row.categoryName ?? "none"} className="flex items-center gap-3">
            <span className="w-40 shrink-0 truncate text-sm">{row.categoryName ?? t("dashboard.analytics.uncategorized")}</span>
            <div className="h-3 flex-1 overflow-hidden rounded-full bg-surface-soft">
              <div className="h-full rounded-full bg-accent" style={{ width: `${row.barPct}%` }} />
            </div>
            <span className="w-12 shrink-0 text-end text-sm text-ink-muted">{row.totalViews}</span>
          </div>
        ))}
      </div>
    </div>
  );
}
