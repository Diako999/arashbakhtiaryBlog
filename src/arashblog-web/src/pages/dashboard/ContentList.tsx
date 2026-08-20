import { useState, type FormEvent } from "react";
import { Link } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { dashboardApi } from "../../api/dashboard";
import { useSeo } from "../../hooks/useSeo";

export default function ContentList() {
  const { t } = useTranslation();
  useSeo({ title: t("dashboard.content.title") });
  const queryClient = useQueryClient();
  const [newCategory, setNewCategory] = useState({ nameFa: "", nameCkb: "" });

  const { data: postsData, isLoading } = useQuery({ queryKey: ["dashboard-posts"], queryFn: () => dashboardApi.posts() });
  const { data: categories } = useQuery({ queryKey: ["dashboard-categories"], queryFn: dashboardApi.categories });

  const deleteMutation = useMutation({
    mutationFn: (id: number) => dashboardApi.deletePost(id),
    onSuccess: () => void queryClient.invalidateQueries({ queryKey: ["dashboard-posts"] }),
  });

  const createCategoryMutation = useMutation({
    mutationFn: () => dashboardApi.createCategory({ slug: "", nameFa: newCategory.nameFa, nameCkb: newCategory.nameCkb }),
    onSuccess: () => {
      setNewCategory({ nameFa: "", nameCkb: "" });
      void queryClient.invalidateQueries({ queryKey: ["dashboard-categories"] });
    },
  });

  function handleCreateCategory(e: FormEvent) {
    e.preventDefault();
    createCategoryMutation.mutate();
  }

  return (
    <div>
      <div className="mb-6 flex items-center justify-between">
        <h1 className="text-2xl font-bold">{t("dashboard.content.title")}</h1>
        <Link to="/dashboard/content/new" className="rounded-lg bg-brand px-4 py-2 font-bold text-white no-underline">
          {t("dashboard.content.newPost")}
        </Link>
      </div>

      <details className="mb-6 rounded-xl border border-line bg-card p-4">
        <summary className="cursor-pointer font-bold">{t("dashboard.content.categories")}</summary>
        <ul className="my-3 flex flex-wrap gap-2 text-sm">
          {categories?.map((c) => (
            <li key={c.id} className="rounded-full bg-surface-soft px-3 py-1">
              {c.nameFa}
            </li>
          ))}
        </ul>
        <form onSubmit={handleCreateCategory} className="flex flex-wrap gap-2">
          <input
            required
            placeholder={t("dashboard.content.categoryNameFa")}
            value={newCategory.nameFa}
            onChange={(e) => setNewCategory({ ...newCategory, nameFa: e.target.value })}
            className="rounded-lg border border-line px-3 py-1.5 text-sm"
          />
          <input
            required
            placeholder={t("dashboard.content.categoryNameCkb")}
            value={newCategory.nameCkb}
            onChange={(e) => setNewCategory({ ...newCategory, nameCkb: e.target.value })}
            className="rounded-lg border border-line px-3 py-1.5 text-sm"
          />
          <button type="submit" className="rounded-lg bg-brand px-3 py-1.5 text-sm font-bold text-white">
            {t("dashboard.content.addCategory")}
          </button>
        </form>
      </details>

      {isLoading && <p>{t("common.loading")}</p>}

      <div className="flex flex-col gap-2">
        {postsData?.items.map((post) => (
          <div key={post.id} className="flex items-center justify-between rounded-lg border border-line bg-card p-3">
            <div>
              <p className="font-bold">{post.titleFa}</p>
              <p className="text-xs text-ink-faint">
                {post.categoryName ?? t("dashboard.analytics.uncategorized")} ·{" "}
                {post.status === "Published" ? t("dashboard.content.published") : t("dashboard.content.draft")}
              </p>
            </div>
            <div className="flex gap-2">
              <Link to={`/dashboard/content/${post.id}/edit`} className="text-sm text-brand no-underline hover:underline">
                {t("dashboard.content.edit")}
              </Link>
              <button
                type="button"
                onClick={() => {
                  if (confirm(t("dashboard.content.confirmDelete"))) deleteMutation.mutate(post.id);
                }}
                className="text-sm text-danger hover:underline"
              >
                {t("dashboard.content.delete")}
              </button>
            </div>
          </div>
        ))}
        {postsData?.items.length === 0 && <p className="text-ink-muted">{t("blog.noPosts")}</p>}
      </div>
    </div>
  );
}
