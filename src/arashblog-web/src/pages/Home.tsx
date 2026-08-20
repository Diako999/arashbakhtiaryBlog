import { useTranslation } from "react-i18next";
import HomeHero from "../components/HomeHero";
import { useSeo } from "../hooks/useSeo";

export default function Home() {
  const { t } = useTranslation();
  useSeo({ title: t("common.home") });

  return <HomeHero />;
}
