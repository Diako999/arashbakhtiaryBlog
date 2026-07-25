import { useState, type FormEvent } from "react";
import { useTranslation } from "react-i18next";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { dashboardApi } from "../../api/dashboard";
import type { DashboardFlatPageDto, UpsertFlatPageRequest } from "../../api/types";

// Mirrors the Django dashboard's Pages screen — the admin-facing half of
// the phased-rollout mechanism. "blog" never appears here since it's
// always live, not part of the phased rollout.
//
// Also folds in flat-page content editing (About/Contact), which in the
// Django source was left to the stock admin as a fallback — this rewrite
// has no admin surface, so it lives here instead since it's thematically
// the same "Pages" concern.
export default function PagesVisibility() {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const { data: navItems, isLoading: navLoading } = useQuery({ queryKey: ["dashboard-nav-items"], queryFn: dashboardApi.navItems });
  const { data: flatPages, isLoading: pagesLoading } = useQuery({ queryKey: ["dashboard-flat-pages"], queryFn: dashboardApi.flatPages });

  const [editingId, setEditingId] = useState<number | null>(null);
  const [form, setForm] = useState<UpsertFlatPageRequest | null>(null);

  const toggleMutation = useMutation({
    mutationFn: (id: number) => dashboardApi.toggleNavItem(id),
    onSuccess: () => void queryClient.invalidateQueries({ queryKey: ["dashboard-nav-items"] }),
  });

  const saveFlatPageMutation = useMutation({
    mutationFn: () => dashboardApi.updateFlatPage(editingId!, form!),
    onSuccess: () => {
      setEditingId(null);
      setForm(null);
      void queryClient.invalidateQueries({ queryKey: ["dashboard-flat-pages"] });
    },
  });

  function startEdit(page: DashboardFlatPageDto) {
    setEditingId(page.id);
    setForm(page);
  }

  function handleSubmit(e: FormEvent) {
    e.preventDefault();
    saveFlatPageMutation.mutate();
  }

  const inputClass = "w-full rounded-lg border border-line bg-card px-3 py-2 text-sm";

  return (
    <div>
      <h1 className="mb-6 text-2xl font-bold">{t("dashboard.pages.title")}</h1>
      <p className="mb-6 max-w-lg text-sm text-ink-muted">{t("dashboard.pages.description")}</p>

      {navLoading && <p>{t("common.loading")}</p>}

      <div className="mb-10 flex flex-col gap-2">
        {navItems?.map((item) => (
          <div key={item.id} className="flex items-center justify-between card-hover-soft border border-line bg-card p-4">
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

      <h2 className="mb-3 text-lg font-bold">{t("dashboard.pages.content")}</h2>
      {pagesLoading && <p>{t("common.loading")}</p>}

      <div className="flex flex-col gap-3">
        {flatPages?.map((page) => (
          <div key={page.id} className="card-hover-soft border border-line bg-card p-4">
            {editingId === page.id && form ? (
              <form onSubmit={handleSubmit} className="grid grid-cols-1 gap-3 sm:grid-cols-2">
                <input
                  required
                  placeholder={t("dashboard.postForm.titleFa")}
                  value={form.titleFa}
                  onChange={(e) => setForm({ ...form, titleFa: e.target.value })}
                  className={inputClass}
                />
                <input
                  required
                  placeholder={t("dashboard.postForm.titleCkb")}
                  value={form.titleCkb}
                  onChange={(e) => setForm({ ...form, titleCkb: e.target.value })}
                  className={inputClass}
                />
                <textarea
                  required
                  dir="rtl"
                  rows={5}
                  placeholder={t("dashboard.postForm.bodyFa")}
                  value={form.bodyFa}
                  onChange={(e) => setForm({ ...form, bodyFa: e.target.value })}
                  className={`${inputClass} sm:col-span-2`}
                />
                <textarea
                  required
                  dir="rtl"
                  rows={5}
                  placeholder={t("dashboard.postForm.bodyCkb")}
                  value={form.bodyCkb}
                  onChange={(e) => setForm({ ...form, bodyCkb: e.target.value })}
                  className={`${inputClass} sm:col-span-2`}
                />
                <div className="flex gap-2 sm:col-span-2">
                  <button type="submit" className="btn-primary">
                    {t("dashboard.postForm.save")}
                  </button>
                  <button type="button" onClick={() => setEditingId(null)} className="rounded-lg px-4 py-2 text-sm text-ink-muted">
                    {t("common.back")}
                  </button>
                </div>
              </form>
            ) : (
              <div className="flex items-center justify-between">
                <span className="font-bold">{page.titleFa}</span>
                <button type="button" onClick={() => startEdit(page)} className="text-sm text-brand hover:underline">
                  {t("dashboard.content.edit")}
                </button>
              </div>
            )}
          </div>
        ))}
      </div>
    </div>
  );
}
