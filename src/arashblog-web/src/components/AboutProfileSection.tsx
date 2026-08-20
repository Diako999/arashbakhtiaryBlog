import { useParams } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { useQuery } from "@tanstack/react-query";
import { siteApi } from "../api/site";
import { defaultLanguage } from "../i18n";
import { ProfileCard } from "./ui/profile-card";

// Shared by the Home page's "about" section and the /about flat page, so
// both read from the same source instead of duplicating the bio/social copy.
export default function AboutProfileSection() {
  const { lang = defaultLanguage } = useParams();
  const { t } = useTranslation();
  const { data: siteSettings } = useQuery({ queryKey: ["site-settings"], queryFn: siteApi.settings });
  const { data: landing } = useQuery({ queryKey: ["landing-settings", lang], queryFn: () => siteApi.landing(lang) });

  return (
    <ProfileCard
      name={siteSettings?.siteName ?? t("home.hero.title")}
      title={landing?.aboutRole || t("home.profile.role")}
      description={landing?.aboutBio || t("home.profile.description")}
      imageUrl={landing?.aboutPhotoUrl}
      githubUrl={landing?.aboutGithubUrl || undefined}
      youtubeUrl={landing?.aboutYoutubeUrl || undefined}
      twitterUrl={siteSettings?.twitterUrl || undefined}
      linkedinUrl={siteSettings?.linkedinUrl || undefined}
    />
  );
}
