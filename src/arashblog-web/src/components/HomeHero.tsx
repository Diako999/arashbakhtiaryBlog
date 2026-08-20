import { useMemo } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { useQuery } from "@tanstack/react-query";
import { siteApi } from "@/api/site";
import { blogApi } from "@/api/blog";
import { ScrollGlobe, type ScrollGlobeSection } from "@/components/ui/scroll-globe";
import { PostCardSlider } from "@/components/ui/post-card-slider";
import AboutProfileSection from "@/components/AboutProfileSection";
import HeroSlider from "@/components/HeroSlider";
import { defaultLanguage } from "@/i18n";

export default function HomeHero() {
  const { lang = defaultLanguage } = useParams();
  const { t } = useTranslation();
  const navigate = useNavigate();

  const { data: siteSettings } = useQuery({ queryKey: ["site-settings"], queryFn: siteApi.settings });
  const { data: landing } = useQuery({ queryKey: ["landing-settings", lang], queryFn: () => siteApi.landing(lang) });
  const { data: posts } = useQuery({
    queryKey: ["posts", lang, "hero"],
    queryFn: () => blogApi.list({ lang, page: 1 }),
  });

  const sections = useMemo<ScrollGlobeSection[]>(() => {
    const result: ScrollGlobeSection[] = [
      {
        id: "hero",
        media: <HeroSlider />,
        badge: landing?.heroBadge || t("home.hero.badge"),
        title: siteSettings?.siteName ?? t("home.hero.title"),
        subtitle: landing?.heroSubtitle || t("home.hero.subtitle"),
        description: landing?.heroDescription || siteSettings?.metaDescription || t("home.hero.description"),
        align: "start",
        actions: [
          { label: t("home.hero.ctaBlog"), variant: "primary", onClick: () => navigate(`/${lang}/blog`) },
          { label: t("home.hero.ctaOfferings"), variant: "secondary", onClick: () => navigate(`/${lang}/offerings`) },
        ],
      },
    ];

    const posts_ = posts?.items ?? [];
    if (posts_.length > 0) {
      result.push({
        id: "posts",
        badge: t("home.posts.badge"),
        title: t("home.posts.title"),
        description: t("home.posts.description"),
        align: "center",
        customContent: (
          <PostCardSlider
            items={posts_.slice(0, 6).map((p) => ({
              key: p.slug,
              title: p.title,
              coverImageUrl: p.coverImageUrl,
              badge: p.categoryName ?? undefined,
              onClick: () => navigate(`/${lang}/blog/${encodeURIComponent(p.slug)}`),
            }))}
            viewAllLabel={t("home.posts.viewAll")}
            onViewAll={() => navigate(`/${lang}/blog`)}
          />
        ),
      });
    }

    result.push({
      id: "about",
      badge: t("home.profile.badge"),
      title: t("home.profile.sectionTitle"),
      align: "center",
      customContent: <AboutProfileSection />,
    });

    result.push({
      id: "cta",
      badge: t("home.cta.badge"),
      title: t("home.cta.title"),
      subtitle: t("home.cta.subtitle"),
      description: t("home.cta.description"),
      align: "center",
      actions: [
        { label: t("home.cta.contact"), variant: "primary", onClick: () => navigate(`/${lang}/contact`) },
        { label: t("home.cta.testimonials"), variant: "secondary", onClick: () => navigate(`/${lang}/testimonials`) },
      ],
    });

    return result;
  }, [t, navigate, lang, siteSettings, landing, posts]);

  return <ScrollGlobe sections={sections} />;
}
