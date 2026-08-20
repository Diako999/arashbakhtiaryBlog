import { useEffect, useState, type FormEvent } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { useQuery } from "@tanstack/react-query";
import { dashboardApi } from "../../api/dashboard";
import type { UpsertPostRequest } from "../../api/types";
import FileUploadField from "../../components/FileUploadField";
import RichTextEditor from "@/components/ui/rich-text-editor";
import { useSeo } from "../../hooks/useSeo";

const emptyForm: UpsertPostRequest = {
  slug: "",
  categoryId: null,
  tags: "",
  coverImageUrl: "",
  status: "Draft",
  publishedAt: null,
  titleFa: "",
  titleCkb: "",
  excerptFa: "",
  excerptCkb: "",
  bodyFa: "",
  bodyCkb: "",
  metaTitleFa: "",
  metaTitleCkb: "",
  metaDescriptionFa: "",
  metaDescriptionCkb: "",
};

export default function PostForm() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const { id } = useParams();
  const isEdit = id !== undefined;
  useSeo({ title: isEdit ? t("dashboard.postForm.editTitle") : t("dashboard.postForm.newTitle") });
  const [form, setForm] = useState<UpsertPostRequest>(emptyForm);
  const [error, setError] = useState<"bodyFa" | "bodyCkb" | "network" | null>(null);

  const { data: categories } = useQuery({ queryKey: ["dashboard-categories"], queryFn: dashboardApi.categories });
  const { data: existing } = useQuery({
    queryKey: ["dashboard-post", id],
    queryFn: () => dashboardApi.post(Number(id)),
    enabled: isEdit,
  });

  useEffect(() => {
    if (existing) setForm(existing);
  }, [existing]);

  function isBodyEmpty(html: string) {
    return html.replace(/<[^>]*>/g, "").trim().length === 0;
  }

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    setError(null);
    if (isBodyEmpty(form.bodyFa)) {
      setError("bodyFa");
      return;
    }
    if (isBodyEmpty(form.bodyCkb)) {
      setError("bodyCkb");
      return;
    }
    try {
      if (isEdit) await dashboardApi.updatePost(Number(id), form);
      else await dashboardApi.createPost(form);
      navigate("/dashboard/content");
    } catch (err) {
      console.error("Failed to save post:", err);
      setError("network");
    }
  }

  const inputClass = "w-full rounded-lg border border-line bg-card px-3 py-2";
  const labelClass = "mb-1 block text-sm text-ink-muted";

  return (
    <div>
      <h1 className="mb-6 text-2xl font-bold">
        {isEdit ? t("dashboard.postForm.editTitle") : t("dashboard.postForm.newTitle")}
      </h1>
      <form onSubmit={(e) => void handleSubmit(e)} className="grid max-w-3xl grid-cols-1 gap-4 sm:grid-cols-2">
        <div>
          <label className={labelClass}>{t("dashboard.postForm.titleFa")}</label>
          <input
            required
            value={form.titleFa}
            onChange={(e) => setForm({ ...form, titleFa: e.target.value })}
            className={inputClass}
          />
        </div>
        <div>
          <label className={labelClass}>{t("dashboard.postForm.titleCkb")}</label>
          <input
            required
            value={form.titleCkb}
            onChange={(e) => setForm({ ...form, titleCkb: e.target.value })}
            className={inputClass}
          />
        </div>

        <div>
          <label className={labelClass}>{t("dashboard.postForm.excerptFa")}</label>
          <input
            value={form.excerptFa}
            onChange={(e) => setForm({ ...form, excerptFa: e.target.value })}
            className={inputClass}
          />
        </div>
        <div>
          <label className={labelClass}>{t("dashboard.postForm.excerptCkb")}</label>
          <input
            value={form.excerptCkb}
            onChange={(e) => setForm({ ...form, excerptCkb: e.target.value })}
            className={inputClass}
          />
        </div>

        <div className="sm:col-span-2">
          <label className={labelClass}>{t("dashboard.postForm.bodyFa")}</label>
          <RichTextEditor value={form.bodyFa} onChange={(html) => setForm({ ...form, bodyFa: html })} />
          {error === "bodyFa" && <p className="mt-1 text-sm text-danger">{t("dashboard.postForm.bodyRequired")}</p>}
        </div>
        <div className="sm:col-span-2">
          <label className={labelClass}>{t("dashboard.postForm.bodyCkb")}</label>
          <RichTextEditor value={form.bodyCkb} onChange={(html) => setForm({ ...form, bodyCkb: html })} />
          {error === "bodyCkb" && <p className="mt-1 text-sm text-danger">{t("dashboard.postForm.bodyRequired")}</p>}
        </div>

        <div>
          <label className={labelClass}>{t("dashboard.postForm.category")}</label>
          <select
            value={form.categoryId ?? ""}
            onChange={(e) => setForm({ ...form, categoryId: e.target.value ? Number(e.target.value) : null })}
            className={inputClass}
          >
            <option value="">{t("dashboard.analytics.uncategorized")}</option>
            {categories?.map((c) => (
              <option key={c.id} value={c.id}>
                {c.nameFa}
              </option>
            ))}
          </select>
        </div>
        <div>
          <label className={labelClass}>{t("dashboard.postForm.tags")}</label>
          <input value={form.tags} onChange={(e) => setForm({ ...form, tags: e.target.value })} className={inputClass} />
        </div>

        <div>
          <label className={labelClass}>{t("dashboard.postForm.status")}</label>
          <select
            value={form.status}
            onChange={(e) => setForm({ ...form, status: e.target.value as "Draft" | "Published" })}
            className={inputClass}
          >
            <option value="Draft">{t("dashboard.content.draft")}</option>
            <option value="Published">{t("dashboard.content.published")}</option>
          </select>
        </div>
        <div>
          <label className={labelClass}>{t("dashboard.postForm.publishedAt")}</label>
          <input
            type="datetime-local"
            value={form.publishedAt ? form.publishedAt.slice(0, 16) : ""}
            onChange={(e) => setForm({ ...form, publishedAt: e.target.value ? new Date(e.target.value).toISOString() : null })}
            className={inputClass}
          />
        </div>

        <div className="sm:col-span-2">
          <FileUploadField
            kind="image"
            label={t("dashboard.postForm.coverImageUrl")}
            value={form.coverImageUrl}
            onChange={(url) => setForm({ ...form, coverImageUrl: url })}
          />
        </div>

        <div className="sm:col-span-2 flex gap-3 pt-2">
          <button type="submit" className="rounded-lg bg-brand px-5 py-2 font-bold text-white">
            {t("dashboard.postForm.save")}
          </button>
          {error === "network" && <p className="self-center text-danger">{t("dashboard.postForm.error")}</p>}
        </div>
      </form>
    </div>
  );
}
