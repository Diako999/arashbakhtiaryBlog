import { useNavigate, useParams } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { useQuery } from "@tanstack/react-query";
import { leadsApi } from "../api/leads";
import { defaultLanguage } from "../i18n";
import { GlassCard } from "@/components/ui/gradient-blob-card";
import { useSeo } from "../hooks/useSeo";

export default function LeadList() {
  const { lang = defaultLanguage } = useParams();
  const { t } = useTranslation();
  const navigate = useNavigate();
  useSeo({ title: t("dashboard.nav.leads") });

  const { data, isLoading } = useQuery({
    queryKey: ["leads", lang],
    queryFn: () => leadsApi.list(lang),
  });

  if (isLoading) return <p>{t("common.loading")}</p>;
  if (data?.length === 0) return <p>{t("leads.none")}</p>;

  return (
    <div className="flex flex-col gap-6">
      {data?.map((lead) => (
        <GlassCard
          key={lead.slug}
          onClick={() => navigate(`/${lang}/free-resource/${encodeURIComponent(lead.slug)}`)}
          className="w-full text-start"
          contentClassName="p-0"
        >
          <div className="h-48 w-full shrink-0 overflow-hidden rounded-t-2xl bg-gradient-to-br from-brand/25 to-accent/25 sm:h-64">
            {lead.coverImageUrl && <img src={lead.coverImageUrl} alt="" className="h-full w-full object-cover" />}
          </div>
          <div className="flex flex-col gap-2 p-4 sm:p-5">
            <h2 className="text-lg font-semibold text-ink">{lead.title}</h2>
            {lead.description && <p className="text-ink-muted">{lead.description}</p>}
          </div>
        </GlassCard>
      ))}
    </div>
  );
}
