import { Link } from "react-router-dom";
import { useTranslation } from "react-i18next";
import type { LandingSectionDto } from "../../api/types";
import OfferingCard from "../../components/OfferingCard";

export default function OfferingsTeaser({ section, lang }: { section: LandingSectionDto; lang: string }) {
  const { t } = useTranslation();
  if (!section.offerings || section.offerings.length === 0) return null;

  return (
    <section className="flex flex-col gap-6">
      <div className="flex items-center justify-between gap-4">
        <h2 className="text-2xl font-bold">{section.heading}</h2>
        <Link to={`/${lang}/offerings`} className="text-sm font-bold text-brand no-underline">
          {t("landing.seeAll")}
        </Link>
      </div>
      <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3">
        {section.offerings.map((offering) => (
          <OfferingCard key={offering.slug} offering={offering} lang={lang} />
        ))}
      </div>
    </section>
  );
}
