import { useState, type FormEvent } from "react";
import { Link, useParams } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { useMutation, useQuery } from "@tanstack/react-query";
import { offeringsApi } from "../api/offerings";
import { useSeo } from "../hooks/useSeo";
import { defaultLanguage } from "../i18n";

export default function OfferingDetail() {
  const { lang = defaultLanguage, slug = "" } = useParams();
  const { t } = useTranslation();
  const [form, setForm] = useState({ sessionId: "", name: "", email: "", phone: "", notes: "" });
  const [submitted, setSubmitted] = useState(false);
  const [error, setError] = useState(false);

  const { data: offering, isLoading } = useQuery({
    queryKey: ["offering", lang, slug],
    queryFn: () => offeringsApi.detail(slug, lang),
  });

  const enrollMutation = useMutation({
    mutationFn: () =>
      offeringsApi.enroll(slug, {
        sessionId: form.sessionId ? Number(form.sessionId) : null,
        name: form.name,
        email: form.email,
        phone: form.phone,
        notes: form.notes,
      }),
    onSuccess: () => {
      setSubmitted(true);
      setError(false);
    },
    onError: () => setError(true),
  });

  function handleSubmit(e: FormEvent) {
    e.preventDefault();
    enrollMutation.mutate();
  }

  useSeo({ title: offering?.title ?? t("common.loading"), description: offering?.summary, image: offering?.coverImageUrl });

  if (isLoading) return <p>{t("common.loading")}</p>;
  if (!offering) return <p>{t("offerings.none")}</p>;

  return (
    <article>
      <Link to={`/${lang}/offerings`} className="text-sm text-ink-muted no-underline hover:text-brand">
        ← {t("common.back")}
      </Link>
      <h1 className="mb-2 mt-3 text-3xl font-bold">{offering.title}</h1>
      {offering.price !== null && (
        <p className="mb-4 text-lg font-bold text-brand">
          {offering.price.toLocaleString()} {t("offerings.currency")}
        </p>
      )}
      <div dangerouslySetInnerHTML={{ __html: offering.bodyHtml }} />

      <section className="mt-10 border-t border-line pt-6">
        <h2 className="mb-4 text-xl font-bold">{t("offerings.enrollTitle")}</h2>

        {submitted ? (
          <p className="text-brand">{t("offerings.enrollSuccess")}</p>
        ) : (
          <form onSubmit={handleSubmit} className="flex max-w-md flex-col gap-3">
            {offering.sessions.length > 0 && (
              <select
                value={form.sessionId}
                onChange={(e) => setForm({ ...form, sessionId: e.target.value })}
                className="rounded-lg border border-line bg-card px-3 py-2"
              >
                <option value="">{t("offerings.noSessionPreference")}</option>
                {offering.sessions.map((s) => (
                  <option key={s.id} value={s.id}>
                    {new Date(s.startsAt).toLocaleString()} {s.location && `— ${s.location}`}
                  </option>
                ))}
              </select>
            )}
            <input
              required
              placeholder={t("offerings.form.name")}
              value={form.name}
              onChange={(e) => setForm({ ...form, name: e.target.value })}
              className="rounded-lg border border-line bg-card px-3 py-2"
            />
            <input
              required
              type="email"
              placeholder={t("offerings.form.email")}
              value={form.email}
              onChange={(e) => setForm({ ...form, email: e.target.value })}
              className="rounded-lg border border-line bg-card px-3 py-2"
            />
            <input
              placeholder={t("offerings.form.phone")}
              value={form.phone}
              onChange={(e) => setForm({ ...form, phone: e.target.value })}
              className="rounded-lg border border-line bg-card px-3 py-2"
            />
            <textarea
              placeholder={t("offerings.form.notes")}
              value={form.notes}
              onChange={(e) => setForm({ ...form, notes: e.target.value })}
              rows={3}
              className="rounded-lg border border-line bg-card px-3 py-2"
            />
            <button type="submit" className="self-start btn-primary">
              {t("offerings.form.submit")}
            </button>
            {error && <p className="text-danger">{t("offerings.form.error")}</p>}
          </form>
        )}
      </section>
    </article>
  );
}
