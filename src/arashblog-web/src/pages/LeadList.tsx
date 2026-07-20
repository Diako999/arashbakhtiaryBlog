import { Link, useParams } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { useQuery } from "@tanstack/react-query";
import { leadsApi } from "../api/leads";
import { defaultLanguage } from "../i18n";

export default function LeadList() {
  const { lang = defaultLanguage } = useParams();
  const { t } = useTranslation();

  const { data, isLoading } = useQuery({
    queryKey: ["leads", lang],
    queryFn: () => leadsApi.list(lang),
  });

  if (isLoading) return <p>{t("common.loading")}</p>;
  if (data?.length === 0) return <p>{t("leads.none")}</p>;

  return (
    <div className="flex flex-col gap-6">
      {data?.map((lead) => (
        <article key={lead.slug} className="rounded-xl border border-line bg-card p-5">
          <h2 className="mb-2 text-xl font-bold">
            <Link to={`/${lang}/free-resource/${encodeURIComponent(lead.slug)}`} className="text-ink no-underline hover:text-brand">
              {lead.title}
            </Link>
          </h2>
          {lead.description && <p className="text-ink-muted">{lead.description}</p>}
        </article>
      ))}
    </div>
  );
}
