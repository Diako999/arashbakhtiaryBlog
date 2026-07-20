import { useState, type FormEvent } from "react";
import { useTranslation } from "react-i18next";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { dashboardApi } from "../../api/dashboard";
import type { DashboardTestimonialDto, UpsertTestimonialRequest } from "../../api/types";
import FileUploadField from "../../components/FileUploadField";

const emptyForm: UpsertTestimonialRequest = {
  authorName: "",
  authorRoleFa: "",
  authorRoleCkb: "",
  quoteFa: "",
  quoteCkb: "",
  photoUrl: "",
  videoUrl: "",
  offeringId: null,
};

export default function TestimonialsList() {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const [editingId, setEditingId] = useState<number | "new" | null>(null);
  const [form, setForm] = useState<UpsertTestimonialRequest>(emptyForm);

  const { data, isLoading } = useQuery({ queryKey: ["dashboard-testimonials"], queryFn: dashboardApi.testimonials });

  const invalidate = () => void queryClient.invalidateQueries({ queryKey: ["dashboard-testimonials"] });

  const saveMutation = useMutation({
    mutationFn: () =>
      editingId === "new" || editingId === null
        ? dashboardApi.createTestimonial(form)
        : dashboardApi.updateTestimonial(editingId, form),
    onSuccess: () => {
      setEditingId(null);
      setForm(emptyForm);
      invalidate();
    },
  });

  const deleteMutation = useMutation({ mutationFn: (id: number) => dashboardApi.deleteTestimonial(id), onSuccess: invalidate });
  const toggleMutation = useMutation({ mutationFn: (id: number) => dashboardApi.toggleTestimonialApproved(id), onSuccess: invalidate });
  const moveMutation = useMutation({
    mutationFn: ({ id, direction }: { id: number; direction: "up" | "down" }) => dashboardApi.moveTestimonial(id, direction),
    onSuccess: invalidate,
  });

  function startEdit(testimonial: DashboardTestimonialDto) {
    setEditingId(testimonial.id);
    setForm(testimonial);
  }

  function startNew() {
    setEditingId("new");
    setForm(emptyForm);
  }

  function handleSubmit(e: FormEvent) {
    e.preventDefault();
    saveMutation.mutate();
  }

  const inputClass = "w-full rounded-lg border border-line bg-card px-3 py-2 text-sm";

  return (
    <div>
      <div className="mb-6 flex items-center justify-between">
        <h1 className="text-2xl font-bold">{t("dashboard.testimonials.title")}</h1>
        {editingId === null && (
          <button type="button" onClick={startNew} className="rounded-lg bg-brand px-4 py-2 font-bold text-white">
            {t("dashboard.testimonials.newTestimonial")}
          </button>
        )}
      </div>

      {editingId !== null && (
        <form onSubmit={handleSubmit} className="mb-6 grid grid-cols-1 gap-3 rounded-xl border border-line bg-card p-4 sm:grid-cols-2">
          <input
            required
            placeholder={t("dashboard.testimonials.authorName")}
            value={form.authorName}
            onChange={(e) => setForm({ ...form, authorName: e.target.value })}
            className={inputClass}
          />
          <input
            placeholder={t("dashboard.testimonials.videoUrl")}
            value={form.videoUrl}
            onChange={(e) => setForm({ ...form, videoUrl: e.target.value })}
            className={inputClass}
          />
          <input
            placeholder={t("dashboard.testimonials.roleFa")}
            value={form.authorRoleFa}
            onChange={(e) => setForm({ ...form, authorRoleFa: e.target.value })}
            className={inputClass}
          />
          <input
            placeholder={t("dashboard.testimonials.roleCkb")}
            value={form.authorRoleCkb}
            onChange={(e) => setForm({ ...form, authorRoleCkb: e.target.value })}
            className={inputClass}
          />
          <textarea
            required
            dir="rtl"
            placeholder={t("dashboard.testimonials.quoteFa")}
            value={form.quoteFa}
            onChange={(e) => setForm({ ...form, quoteFa: e.target.value })}
            className={`${inputClass} sm:col-span-2`}
          />
          <textarea
            required
            dir="rtl"
            placeholder={t("dashboard.testimonials.quoteCkb")}
            value={form.quoteCkb}
            onChange={(e) => setForm({ ...form, quoteCkb: e.target.value })}
            className={`${inputClass} sm:col-span-2`}
          />
          <div className="sm:col-span-2">
            <FileUploadField
              kind="image"
              label={t("dashboard.testimonials.photo")}
              value={form.photoUrl}
              onChange={(url) => setForm({ ...form, photoUrl: url })}
            />
          </div>
          <div className="flex gap-2 sm:col-span-2">
            <button type="submit" className="rounded-lg bg-brand px-4 py-2 text-sm font-bold text-white">
              {t("dashboard.postForm.save")}
            </button>
            <button type="button" onClick={() => setEditingId(null)} className="rounded-lg px-4 py-2 text-sm text-ink-muted">
              {t("common.back")}
            </button>
          </div>
        </form>
      )}

      {isLoading && <p>{t("common.loading")}</p>}

      <div className="flex flex-col gap-2">
        {data?.map((testimonial) => (
          <div key={testimonial.id} className="rounded-lg border border-line bg-card p-3">
            <div className="mb-1 flex items-center justify-between">
              <span className="font-bold">{testimonial.authorName}</span>
              <span
                className={`rounded-full px-2 py-0.5 text-xs ${
                  testimonial.isApproved ? "bg-brand-soft text-brand" : "bg-surface-soft text-ink-muted"
                }`}
              >
                {testimonial.isApproved ? t("dashboard.comments.approved") : t("dashboard.comments.pending")}
              </span>
            </div>
            <p className="mb-2 text-sm text-ink-muted">{testimonial.quoteFa}</p>
            <div className="flex gap-3 text-sm">
              <button type="button" onClick={() => toggleMutation.mutate(testimonial.id)} className="text-brand hover:underline">
                {testimonial.isApproved ? t("dashboard.comments.unapprove") : t("dashboard.comments.approve")}
              </button>
              <button type="button" onClick={() => moveMutation.mutate({ id: testimonial.id, direction: "up" })} className="hover:underline">
                ↑
              </button>
              <button type="button" onClick={() => moveMutation.mutate({ id: testimonial.id, direction: "down" })} className="hover:underline">
                ↓
              </button>
              <button type="button" onClick={() => startEdit(testimonial)} className="text-brand hover:underline">
                {t("dashboard.content.edit")}
              </button>
              <button
                type="button"
                onClick={() => {
                  if (confirm(t("dashboard.content.confirmDelete"))) deleteMutation.mutate(testimonial.id);
                }}
                className="text-danger hover:underline"
              >
                {t("dashboard.content.delete")}
              </button>
            </div>
          </div>
        ))}
        {data?.length === 0 && <p className="text-ink-muted">{t("testimonials.none")}</p>}
      </div>
    </div>
  );
}
