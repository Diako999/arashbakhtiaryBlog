import { useParams } from "react-router-dom";
import { useQuery } from "@tanstack/react-query";
import { landingApi } from "../api/landing";
import { defaultLanguage } from "../i18n";
import Hero from "./landing/Hero";
import OfferingsTeaser from "./landing/OfferingsTeaser";
import PostsTeaser from "./landing/PostsTeaser";
import TestimonialsTeaser from "./landing/TestimonialsTeaser";
import CtaBanner from "./landing/CtaBanner";

export default function LandingPage() {
  const { lang = defaultLanguage } = useParams();

  const { data } = useQuery({
    queryKey: ["landing", lang],
    queryFn: () => landingApi.get(lang),
  });

  return (
    <div className="flex flex-col gap-16 md:gap-24">
      {data?.map((section) => {
        switch (section.type) {
          case "Hero":
            return <Hero key={section.type} section={section} />;
          case "OfferingsTeaser":
            return <OfferingsTeaser key={section.type} section={section} lang={lang} />;
          case "PostsTeaser":
            return <PostsTeaser key={section.type} section={section} lang={lang} />;
          case "TestimonialsTeaser":
            return <TestimonialsTeaser key={section.type} section={section} lang={lang} />;
          case "CtaBanner":
            return <CtaBanner key={section.type} section={section} />;
          default:
            return null;
        }
      })}
    </div>
  );
}
