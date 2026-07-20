import { useEffect, useState, type FormEvent } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { useQuery } from "@tanstack/react-query";
import { dashboardApi } from "../../api/dashboard";
import type { DashboardSessionDto, UpsertOfferingRequest } from "../../api/types";
import FileUploadField from "../../components/FileUploadField";

const emptyForm: UpsertOfferingRequest = {
  slug: "",
  coverImageUrl: "",
  price: null,
  status: "Draft",
  titleFa: "",
  titleCkb: "",
  summaryFa: "",
  summaryCkb: "",
  bodyFa: "",
  bodyCkb: "",
  metaTitleFa: "",
  metaTitleCkb: "",
  metaDescriptionFa: "",
  metaDescriptionCkb: "",
  sessions: [],
};

let tempIdCounter = -1;

export default function OfferingForm() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const { id } = useParams();
  const isEdit = id !== undefined;
  const [form, setForm] = useState<UpsertOfferingRequest>(emptyForm);
  const [error, setError] = useState(false);

  const { data: existing } = useQuery({
    queryKey: ["dashboard-offering", id],
    queryFn: () => dashboardApi.offering(Number(id)),
    enabled: isEdit,
  });

  useEffect(() => {
    if (existing) setForm(existing);
  }, [existing]);

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    setError(false);
    // Sessions added in this form carry a negative temp id — the API
    // treats null as "new", so strip temp ids before submitting.
    const payload: UpsertOfferingRequest = {
      ...form,
      sessions: form.sessions.map((s) => ({ ...s, id: s.id !== null && s.id < 0 ? null : s.id })),
    };
    try {
      if (isEdit) await dashboardApi.updateOffering(Number(id), payload);
      else await dashboardApi.createOffering(payload);
      navigate("/dashboard/offerings");
    } catch {
      setError(true);
    }
  }

  function addSession() {
    const newSession: DashboardSessionDto = {
      id: tempIdCounter--,
      startsAt: new Date().toISOString(),
      endsAt: null,
      location: "",
      capacity: null,
    };
    setForm({ ...form, sessions: [...form.sessions, newSession] });
  }

  function updateSession(index: number, patch: Partial<DashboardSessionDto>) {
    const sessions = [...form.sessions];
    sessions[index] = { ...sessions[index], ...patch };
    setForm({ ...form, sessions });
  }

  function removeSession(index: number) {
    setForm({ ...form, sessions: form.sessions.filter((_, i) => i !== index) });
  }

  const inputClass = "w-full rounded-lg border border-line bg-card px-3 py-2";
  const labelClass = "mb-1 block text-sm text-ink-muted";

  return (
    <div>
      <h1 className="mb-6 text-2xl font-bold">
        {isEdit ? t("dashboard.offeringForm.editTitle") : t("dashboard.offeringForm.newTitle")}
      </h1>
      <form onSubmit={(e) => void handleSubmit(e)} className="max-w-3xl">
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
          <div>
            <label className={labelClass}>{t("dashboard.postForm.titleFa")}</label>
            <input required value={form.titleFa} onChange={(e) => setForm({ ...form, titleFa: e.target.value })} className={inputClass} />
          </div>
          <div>
            <label className={labelClass}>{t("dashboard.postForm.titleCkb")}</label>
            <input required value={form.titleCkb} onChange={(e) => setForm({ ...form, titleCkb: e.target.value })} className={inputClass} />
          </div>
          <div>
            <label className={labelClass}>{t("dashboard.offeringForm.summaryFa")}</label>
            <input value={form.summaryFa} onChange={(e) => setForm({ ...form, summaryFa: e.target.value })} className={inputClass} />
          </div>
          <div>
            <label className={labelClass}>{t("dashboard.offeringForm.summaryCkb")}</label>
            <input value={form.summaryCkb} onChange={(e) => setForm({ ...form, summaryCkb: e.target.value })} className={inputClass} />
          </div>
          <div className="sm:col-span-2">
            <label className={labelClass}>{t("dashboard.postForm.bodyFa")}</label>
            <textarea required rows={5} dir="rtl" value={form.bodyFa} onChange={(e) => setForm({ ...form, bodyFa: e.target.value })} className={inputClass} />
          </div>
          <div className="sm:col-span-2">
            <label className={labelClass}>{t("dashboard.postForm.bodyCkb")}</label>
            <textarea required rows={5} dir="rtl" value={form.bodyCkb} onChange={(e) => setForm({ ...form, bodyCkb: e.target.value })} className={inputClass} />
          </div>
          <div>
            <label className={labelClass}>{t("dashboard.offeringForm.price")}</label>
            <input
              type="number"
              step="0.01"
              value={form.price ?? ""}
              onChange={(e) => setForm({ ...form, price: e.target.value ? Number(e.target.value) : null })}
              className={inputClass}
            />
          </div>
          <div>
            <label className={labelClass}>{t("dashboard.postForm.status")}</label>
            <select value={form.status} onChange={(e) => setForm({ ...form, status: e.target.value as "Draft" | "Published" })} className={inputClass}>
              <option value="Draft">{t("dashboard.content.draft")}</option>
              <option value="Published">{t("dashboard.content.published")}</option>
            </select>
          </div>
          <div className="sm:col-span-2">
            <FileUploadField
              kind="image"
              label={t("dashboard.postForm.coverImageUrl")}
              value={form.coverImageUrl}
              onChange={(url) => setForm({ ...form, coverImageUrl: url })}
            />
          </div>
        </div>

        <div className="mt-8">
          <div className="mb-3 flex items-center justify-between">
            <h2 className="text-lg font-bold">{t("dashboard.offeringForm.sessions")}</h2>
            <button type="button" onClick={addSession} className="rounded-lg bg-brand-soft px-3 py-1.5 text-sm font-bold text-brand">
              {t("dashboard.offeringForm.addSession")}
            </button>
          </div>
          <div className="flex flex-col gap-3">
            {form.sessions.map((session, index) => (
              <div key={session.id ?? index} className="grid grid-cols-1 gap-2 rounded-lg border border-line p-3 sm:grid-cols-4">
                <input
                  type="datetime-local"
                  required
                  value={session.startsAt.slice(0, 16)}
                  onChange={(e) => updateSession(index, { startsAt: new Date(e.target.value).toISOString() })}
                  className={inputClass}
                />
                <input
                  placeholder={t("dashboard.offeringForm.location")}
                  value={session.location}
                  onChange={(e) => updateSession(index, { location: e.target.value })}
                  className={inputClass}
                />
                <input
                  type="number"
                  placeholder={t("dashboard.offeringForm.capacity")}
                  value={session.capacity ?? ""}
                  onChange={(e) => updateSession(index, { capacity: e.target.value ? Number(e.target.value) : null })}
                  className={inputClass}
                />
                <button type="button" onClick={() => removeSession(index)} className="rounded-lg text-sm text-danger hover:underline">
                  {t("dashboard.content.delete")}
                </button>
              </div>
            ))}
            {form.sessions.length === 0 && <p className="text-sm text-ink-muted">{t("dashboard.offeringForm.noSessions")}</p>}
          </div>
        </div>

        {isEdit && existing && existing.enrollments.length > 0 && (
          <div className="mt-8">
            <h2 className="mb-3 text-lg font-bold">{t("dashboard.offeringForm.enrollments")}</h2>
            <div className="flex flex-col gap-2">
              {existing.enrollments.map((e) => (
                <div key={e.id} className="rounded-lg border border-line bg-card p-3 text-sm">
                  <span className="font-bold">{e.name}</span> — {e.email} {e.sessionLabel && `· ${e.sessionLabel}`}
                </div>
              ))}
            </div>
          </div>
        )}

        <div className="mt-8 flex items-center gap-3">
          <button type="submit" className="rounded-lg bg-brand px-5 py-2 font-bold text-white">
            {t("dashboard.postForm.save")}
          </button>
          {error && <p className="text-danger">{t("dashboard.postForm.error")}</p>}
        </div>
      </form>
    </div>
  );
}
