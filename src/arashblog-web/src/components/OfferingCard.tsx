import { Link } from "react-router-dom";
import { useTranslation } from "react-i18next";
import type { OfferingSummaryDto } from "../api/types";

export default function OfferingCard({ offering, lang }: { offering: OfferingSummaryDto; lang: string }) {
  const { t } = useTranslation();

  return (
    <article className="card-hover overflow-hidden border border-line bg-card">
      <div
        className="aspect-[16/9] w-full"
        style={
          offering.coverImageUrl
            ? { backgroundImage: `url(${offering.coverImageUrl})`, backgroundSize: "cover", backgroundPosition: "center" }
            : { background: "linear-gradient(135deg, var(--accent) 0%, var(--brand) 100%)" }
        }
      />
      <div className="flex h-full flex-col gap-2 px-5 py-5">
        <h3 className="text-lg font-bold">
          <Link
            to={`/${lang}/offerings/${encodeURIComponent(offering.slug)}`}
            className="text-ink no-underline hover:text-brand"
          >
            {offering.title}
          </Link>
        </h3>
        {offering.summary && <p className="flex-1 text-sm text-ink-muted">{offering.summary}</p>}
        <div className="mt-3 flex items-center justify-between border-t border-line pt-3">
          {offering.price !== null ? (
            <span className="font-bold text-brand">
              {offering.price.toLocaleString()} {t("offerings.currency")}
            </span>
          ) : (
            <span />
          )}
          <Link
            to={`/${lang}/offerings/${encodeURIComponent(offering.slug)}`}
            className="text-sm font-bold text-brand no-underline"
          >
            {t("blog.readMore")}
          </Link>
        </div>
      </div>
    </article>
  );
}
