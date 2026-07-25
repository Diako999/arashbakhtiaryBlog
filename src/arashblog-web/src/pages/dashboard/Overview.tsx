import { useTranslation } from "react-i18next";
import { useQuery } from "@tanstack/react-query";
import { dashboardApi } from "../../api/dashboard";

export default function Overview() {
  const { t } = useTranslation();
  const { data, isLoading } = useQuery({ queryKey: ["dashboard-overview"], queryFn: dashboardApi.overview });

  if (isLoading) return <p>{t("common.loading")}</p>;

  return (
    <div>
      <h1 className="mb-6 text-2xl font-bold">{t("dashboard.overview.title")}</h1>
      <div className="mb-8 grid grid-cols-2 gap-4 sm:grid-cols-4">
        <div className="card-hover-soft border border-line bg-card p-4">
          <p className="text-sm text-ink-muted">{t("dashboard.overview.draft")}</p>
          <p className="text-2xl font-bold">{data?.draftCount}</p>
        </div>
        <div className="card-hover-soft border border-line bg-card p-4">
          <p className="text-sm text-ink-muted">{t("dashboard.overview.published")}</p>
          <p className="text-2xl font-bold">{data?.publishedCount}</p>
        </div>
      </div>

      <h2 className="mb-3 text-lg font-bold">{t("dashboard.overview.recentPosts")}</h2>
      <ul className="flex flex-col gap-2">
        {data?.recentPosts.map((post) => (
          <li key={post.slug} className="card-hover-soft border border-line bg-card p-3">
            {post.title}
          </li>
        ))}
        {data?.recentPosts.length === 0 && <p className="text-ink-muted">{t("blog.noPosts")}</p>}
      </ul>
    </div>
  );
}
