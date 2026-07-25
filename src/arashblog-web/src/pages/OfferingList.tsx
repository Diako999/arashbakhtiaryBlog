import { useParams } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { useQuery } from "@tanstack/react-query";
import { offeringsApi } from "../api/offerings";
import { defaultLanguage } from "../i18n";
import OfferingCard from "../components/OfferingCard";

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
    <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3">
      {data?.map((offering) => (
        <OfferingCard key={offering.slug} offering={offering} lang={lang} />
      ))}
    </div>
  );
}
