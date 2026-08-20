import { useNavigate, useParams } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { useQuery } from "@tanstack/react-query";
import { offeringsApi } from "../api/offerings";
import { defaultLanguage } from "../i18n";
import { GlassCard } from "@/components/ui/gradient-blob-card";
import { useSeo } from "../hooks/useSeo";

export default function OfferingList() {
  const { lang = defaultLanguage } = useParams();
  const { t } = useTranslation();
  const navigate = useNavigate();
  useSeo({ title: t("dashboard.nav.offerings") });

  const { data, isLoading } = useQuery({
    queryKey: ["offerings", lang],
    queryFn: () => offeringsApi.list(lang),
  });

  if (isLoading) return <p>{t("common.loading")}</p>;
  if (data?.length === 0) return <p>{t("offerings.none")}</p>;

  return (
    <div className="flex flex-col gap-6">
      {data?.map((offering) => (
        <GlassCard
          key={offering.slug}
          onClick={() => navigate(`/${lang}/offerings/${encodeURIComponent(offering.slug)}`)}
          className="w-full text-start"
          contentClassName="p-0"
        >
          <div className="h-48 w-full shrink-0 overflow-hidden rounded-t-2xl bg-gradient-to-br from-brand/25 to-accent/25 sm:h-64">
            {offering.coverImageUrl && (
              <img src={offering.coverImageUrl} alt="" className="h-full w-full object-cover" />
            )}
          </div>
          <div className="flex flex-col gap-2 p-4 sm:p-5">
            <h2 className="text-lg font-semibold text-ink">{offering.title}</h2>
            {offering.summary && <p className="text-ink-muted">{offering.summary}</p>}
            {offering.price !== null && (
              <p className="font-bold text-brand">
                {offering.price.toLocaleString()} {t("offerings.currency")}
              </p>
            )}
          </div>
        </GlassCard>
      ))}
    </div>
  );
}
