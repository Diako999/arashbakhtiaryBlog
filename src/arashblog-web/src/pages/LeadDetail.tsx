import { useState, type FormEvent } from "react";
import { Link, useParams } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { useMutation, useQuery } from "@tanstack/react-query";
import { leadsApi } from "../api/leads";
import { defaultLanguage } from "../i18n";

export default function LeadDetail() {
  const { lang = defaultLanguage, slug = "" } = useParams();
  const { t } = useTranslation();
  const [form, setForm] = useState({ name: "", email: "" });
  const [submitted, setSubmitted] = useState(false);
  const [error, setError] = useState(false);

  const { data: lead, isLoading } = useQuery({
    queryKey: ["lead", lang, slug],
    queryFn: () => leadsApi.detail(slug, lang),
  });

  const submitMutation = useMutation({
    mutationFn: () => leadsApi.submit(slug, form),
    onSuccess: () => {
      setSubmitted(true);
      setError(false);
    },
    onError: () => setError(true),
  });

  function handleSubmit(e: FormEvent) {
    e.preventDefault();
    submitMutation.mutate();
  }

  if (isLoading) return <p>{t("common.loading")}</p>;
  if (!lead) return <p>{t("leads.none")}</p>;

  return (
    <article>
      <Link to={`/${lang}/free-resource`} className="text-sm text-ink-muted no-underline hover:text-brand">
        ← {t("common.back")}
      </Link>
      <h1 className="mb-4 mt-3 text-3xl font-bold">{lead.title}</h1>
      {lead.description && <p className="mb-6 text-ink-muted">{lead.description}</p>}

      {submitted ? (
        <div>
          <p className="mb-3 text-brand">{t("leads.submitSuccess")}</p>
          <a href={lead.fileUrl} target="_blank" rel="noopener noreferrer" className="text-brand underline">
            {t("leads.download")}
          </a>
        </div>
      ) : (
        <form onSubmit={handleSubmit} className="flex max-w-md flex-col gap-3">
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
          <button type="submit" className="self-start rounded-lg bg-brand px-4 py-2 font-bold text-white">
            {t("leads.getIt")}
          </button>
          {error && <p className="text-danger">{t("offerings.form.error")}</p>}
        </form>
      )}
    </article>
  );
}
