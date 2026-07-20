import { useEffect, useState, type FormEvent } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { useQuery } from "@tanstack/react-query";
import { dashboardApi } from "../../api/dashboard";
import type { UpsertLeadMagnetRequest } from "../../api/types";
import FileUploadField from "../../components/FileUploadField";

const emptyForm: UpsertLeadMagnetRequest = {
  slug: "",
  coverImageUrl: "",
  fileUrl: "",
  status: "Draft",
  titleFa: "",
  titleCkb: "",
  descriptionFa: "",
  descriptionCkb: "",
  metaTitleFa: "",
  metaTitleCkb: "",
  metaDescriptionFa: "",
  metaDescriptionCkb: "",
};

export default function LeadForm() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const { id } = useParams();
  const isEdit = id !== undefined;
  const [form, setForm] = useState<UpsertLeadMagnetRequest>(emptyForm);
  const [error, setError] = useState(false);

  const { data: existing } = useQuery({
    queryKey: ["dashboard-lead", id],
    queryFn: () => dashboardApi.leadMagnet(Number(id)),
    enabled: isEdit,
  });

  useEffect(() => {
    if (existing) setForm(existing);
  }, [existing]);

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    setError(false);
    try {
      if (isEdit) await dashboardApi.updateLeadMagnet(Number(id), form);
      else await dashboardApi.createLeadMagnet(form);
      navigate("/dashboard/leads");
    } catch {
      setError(true);
    }
  }

  const inputClass = "w-full rounded-lg border border-line bg-card px-3 py-2";
  const labelClass = "mb-1 block text-sm text-ink-muted";

  return (
    <div>
      <h1 className="mb-6 text-2xl font-bold">
        {isEdit ? t("dashboard.leadForm.editTitle") : t("dashboard.leadForm.newTitle")}
      </h1>
      <form onSubmit={(e) => void handleSubmit(e)} className="grid max-w-2xl grid-cols-1 gap-4 sm:grid-cols-2">
        <div>
          <label className={labelClass}>{t("dashboard.postForm.titleFa")}</label>
          <input required value={form.titleFa} onChange={(e) => setForm({ ...form, titleFa: e.target.value })} className={inputClass} />
        </div>
        <div>
          <label className={labelClass}>{t("dashboard.postForm.titleCkb")}</label>
          <input required value={form.titleCkb} onChange={(e) => setForm({ ...form, titleCkb: e.target.value })} className={inputClass} />
        </div>
        <div className="sm:col-span-2">
          <label className={labelClass}>{t("dashboard.leadForm.descriptionFa")}</label>
          <textarea rows={3} dir="rtl" value={form.descriptionFa} onChange={(e) => setForm({ ...form, descriptionFa: e.target.value })} className={inputClass} />
        </div>
        <div className="sm:col-span-2">
          <label className={labelClass}>{t("dashboard.leadForm.descriptionCkb")}</label>
          <textarea rows={3} dir="rtl" value={form.descriptionCkb} onChange={(e) => setForm({ ...form, descriptionCkb: e.target.value })} className={inputClass} />
        </div>
        <div className="sm:col-span-2">
          <FileUploadField
            kind="document"
            required
            label={t("dashboard.leadForm.fileUrl")}
            value={form.fileUrl}
            onChange={(url) => setForm({ ...form, fileUrl: url })}
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
        <div>
          <label className={labelClass}>{t("dashboard.postForm.status")}</label>
          <select value={form.status} onChange={(e) => setForm({ ...form, status: e.target.value as "Draft" | "Published" })} className={inputClass}>
            <option value="Draft">{t("dashboard.content.draft")}</option>
            <option value="Published">{t("dashboard.content.published")}</option>
          </select>
        </div>

        <div className="sm:col-span-2 flex items-center gap-3 pt-2">
          <button type="submit" className="rounded-lg bg-brand px-5 py-2 font-bold text-white">
            {t("dashboard.postForm.save")}
          </button>
          {error && <p className="text-danger">{t("dashboard.postForm.error")}</p>}
        </div>
      </form>
    </div>
  );
}
