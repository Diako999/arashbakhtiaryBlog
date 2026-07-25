import { useState, type FormEvent } from "react";
import { useTranslation } from "react-i18next";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { dashboardApi } from "../../api/dashboard";
import type { DashboardLandingSectionDto, UpsertLandingSectionRequest } from "../../api/types";
import FileUploadField from "../../components/FileUploadField";

function toForm(section: DashboardLandingSectionDto): UpsertLandingSectionRequest {
  const { id: _id, type: _type, order: _order, isVisible: _isVisible, ...rest } = section;
  return rest;
}

// Fixed 5 rows, no create/delete UI — matches the backend's deliberate
// no-POST/no-DELETE design (see LandingSectionsController). Which fields
// are shown in the edit form depends on the section's type: Hero uses
// everything, the teaser sections mainly just need a heading (their real
// content comes from the Offerings/Posts/Testimonials lists), CtaBanner
// uses heading/subheading/primary CTA only.
export default function LandingSections() {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const [editingId, setEditingId] = useState<number | null>(null);
  const [form, setForm] = useState<UpsertLandingSectionRequest | null>(null);

  const { data, isLoading } = useQuery({ queryKey: ["dashboard-landing-sections"], queryFn: dashboardApi.landingSections });

  const invalidate = () => void queryClient.invalidateQueries({ queryKey: ["dashboard-landing-sections"] });

  const saveMutation = useMutation({
    mutationFn: () => dashboardApi.updateLandingSection(editingId!, form!),
    onSuccess: () => {
      setEditingId(null);
      setForm(null);
      invalidate();
    },
  });

  const toggleMutation = useMutation({ mutationFn: (id: number) => dashboardApi.toggleLandingSection(id), onSuccess: invalidate });
  const moveMutation = useMutation({
    mutationFn: ({ id, direction }: { id: number; direction: "up" | "down" }) => dashboardApi.moveLandingSection(id, direction),
    onSuccess: invalidate,
  });

  function startEdit(section: DashboardLandingSectionDto) {
    setEditingId(section.id);
    setForm(toForm(section));
  }

  function handleSubmit(e: FormEvent) {
    e.preventDefault();
    saveMutation.mutate();
  }

  const inputClass = "w-full rounded-lg border border-line bg-card px-3 py-2 text-sm";
  const editingSection = data?.find((s) => s.id === editingId);
  const showCtas = editingSection?.type === "Hero" || editingSection?.type === "CtaBanner";
  const showSecondaryCta = editingSection?.type === "Hero";
  const showImage = editingSection?.type === "Hero";

  return (
    <div>
      <div className="mb-2">
        <h1 className="text-2xl font-bold">{t("dashboard.landing.title")}</h1>
        <p className="text-sm text-ink-muted">{t("dashboard.landing.description")}</p>
      </div>

      {isLoading && <p>{t("common.loading")}</p>}

      <div className="mt-6 flex flex-col gap-2">
        {data?.map((section) => (
          <div key={section.id} className="card-hover-soft border border-line bg-card p-3">
            <div className="mb-1 flex items-center justify-between">
              <span className="font-bold">{t(`dashboard.landing.sections.${section.type}`)}</span>
              <span
                className={`rounded-full px-2 py-0.5 text-xs ${
                  section.isVisible ? "bg-brand-soft text-brand" : "bg-surface-soft text-ink-muted"
                }`}
              >
                {section.isVisible ? t("dashboard.landing.visible") : t("dashboard.landing.hidden")}
              </span>
            </div>
            <p className="mb-2 text-sm text-ink-muted">{section.headingFa}</p>
            <div className="flex gap-3 text-sm">
              <button type="button" onClick={() => toggleMutation.mutate(section.id)} className="text-brand hover:underline">
                {section.isVisible ? t("dashboard.comments.unapprove") : t("dashboard.comments.approve")}
              </button>
              <button type="button" onClick={() => moveMutation.mutate({ id: section.id, direction: "up" })} className="hover:underline">
                ↑
              </button>
              <button type="button" onClick={() => moveMutation.mutate({ id: section.id, direction: "down" })} className="hover:underline">
                ↓
              </button>
              <button type="button" onClick={() => startEdit(section)} className="text-brand hover:underline">
                {t("dashboard.content.edit")}
              </button>
            </div>

            {editingId === section.id && form && (
              <form onSubmit={handleSubmit} className="mt-3 grid grid-cols-1 gap-3 border-t border-line pt-3 sm:grid-cols-2">
                <input
                  placeholder={t("dashboard.landing.headingFa")}
                  value={form.headingFa}
                  onChange={(e) => setForm({ ...form, headingFa: e.target.value })}
                  className={inputClass}
                />
                <input
                  placeholder={t("dashboard.landing.headingCkb")}
                  value={form.headingCkb}
                  onChange={(e) => setForm({ ...form, headingCkb: e.target.value })}
                  className={inputClass}
                />
                {showCtas && (
                  <>
                    <input
                      placeholder={t("dashboard.landing.subheadingFa")}
                      value={form.subheadingFa}
                      onChange={(e) => setForm({ ...form, subheadingFa: e.target.value })}
                      className={inputClass}
                    />
                    <input
                      placeholder={t("dashboard.landing.subheadingCkb")}
                      value={form.subheadingCkb}
                      onChange={(e) => setForm({ ...form, subheadingCkb: e.target.value })}
                      className={inputClass}
                    />
                    <input
                      placeholder={t("dashboard.landing.primaryCtaTextFa")}
                      value={form.primaryCtaTextFa}
                      onChange={(e) => setForm({ ...form, primaryCtaTextFa: e.target.value })}
                      className={inputClass}
                    />
                    <input
                      placeholder={t("dashboard.landing.primaryCtaTextCkb")}
                      value={form.primaryCtaTextCkb}
                      onChange={(e) => setForm({ ...form, primaryCtaTextCkb: e.target.value })}
                      className={inputClass}
                    />
                    <input
                      placeholder={t("dashboard.landing.primaryCtaUrl")}
                      value={form.primaryCtaUrl}
                      onChange={(e) => setForm({ ...form, primaryCtaUrl: e.target.value })}
                      className={`${inputClass} sm:col-span-2`}
                    />
                  </>
                )}
                {showSecondaryCta && (
                  <>
                    <input
                      placeholder={t("dashboard.landing.secondaryCtaTextFa")}
                      value={form.secondaryCtaTextFa}
                      onChange={(e) => setForm({ ...form, secondaryCtaTextFa: e.target.value })}
                      className={inputClass}
                    />
                    <input
                      placeholder={t("dashboard.landing.secondaryCtaTextCkb")}
                      value={form.secondaryCtaTextCkb}
                      onChange={(e) => setForm({ ...form, secondaryCtaTextCkb: e.target.value })}
                      className={inputClass}
                    />
                    <input
                      placeholder={t("dashboard.landing.secondaryCtaUrl")}
                      value={form.secondaryCtaUrl}
                      onChange={(e) => setForm({ ...form, secondaryCtaUrl: e.target.value })}
                      className={`${inputClass} sm:col-span-2`}
                    />
                  </>
                )}
                {showImage && (
                  <div className="sm:col-span-2">
                    <FileUploadField
                      kind="image"
                      label={t("dashboard.landing.imageUrl")}
                      value={form.imageUrl}
                      onChange={(url) => setForm({ ...form, imageUrl: url })}
                    />
                  </div>
                )}
                <div className="flex gap-2 sm:col-span-2">
                  <button type="submit" className="btn-primary">
                    {t("dashboard.landing.save")}
                  </button>
                  <button
                    type="button"
                    onClick={() => {
                      setEditingId(null);
                      setForm(null);
                    }}
                    className="rounded-lg px-4 py-2 text-sm text-ink-muted"
                  >
                    {t("common.back")}
                  </button>
                </div>
              </form>
            )}
          </div>
        ))}
      </div>
    </div>
  );
}
