import { useParams } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { useQuery } from "@tanstack/react-query";
import { pagesApi } from "../api/pages";
import { useSeo } from "../hooks/useSeo";
import { defaultLanguage } from "../i18n";
import AboutProfileSection from "../components/AboutProfileSection";

// Renders both /about and /contact — static content pages, no form (the
// dashboard-editable About/Contact rows carry whatever contact info the
// admin puts in the body, plus SiteSetting.contactEmail/contactPhone are
// available separately for a footer/contact-info display if needed).
export default function FlatPage({ slug }: { slug: "about" | "contact" }) {
  const { lang = defaultLanguage } = useParams();
  const { t } = useTranslation();

  const { data: page, isLoading } = useQuery({
    queryKey: ["page", lang, slug],
    queryFn: () => pagesApi.detail(slug, lang),
  });

  useSeo({ title: page?.title ?? t("common.loading") });

  if (isLoading) return <p>{t("common.loading")}</p>;
  if (!page) return <p>{t("blog.noPosts")}</p>;

  return (
    <article>
      {slug === "about" && <AboutProfileSection />}
      <h1 className="mb-6 mt-8 text-3xl font-bold">{page.title}</h1>
      <div dangerouslySetInnerHTML={{ __html: page.bodyHtml }} />
    </article>
  );
}
