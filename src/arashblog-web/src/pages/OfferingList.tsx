import { Link, useParams } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { useQuery } from "@tanstack/react-query";
import { offeringsApi } from "../api/offerings";
import { defaultLanguage } from "../i18n";

export default function OfferingList() {
  const { lang = defaultLanguage } = useParams();
  const { t } = useTranslation();

  const { data, isLoading } = useQuery({
    queryKey: ["offerings", lang],
    queryFn: () => offeringsApi.list(lang),
  });

  if (isLoading) return <p>{t("common.loading")}</p>;
  if (data?.length === 0) return <p>{t("offerings.none")}</p>;

  return (
    <div className="flex flex-col gap-6">
      {data?.map((offering) => (
        <article key={offering.slug} className="rounded-xl border border-line bg-card p-5">
          <h2 className="mb-2 text-xl font-bold">
            <Link
              to={`/${lang}/offerings/${encodeURIComponent(offering.slug)}`}
              className="text-ink no-underline hover:text-brand"
            >
              {offering.title}
            </Link>
          </h2>
          {offering.summary && <p className="text-ink-muted">{offering.summary}</p>}
          {offering.price !== null && (
            <p className="mt-2 text-sm font-bold text-brand">
              {offering.price.toLocaleString()} {t("offerings.currency")}
            </p>
          )}
        </article>
      ))}
    </div>
  );
}
