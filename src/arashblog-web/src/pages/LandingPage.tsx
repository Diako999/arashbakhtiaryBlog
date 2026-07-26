import { useParams } from "react-router-dom";
import { useQuery } from "@tanstack/react-query";
import { landingApi } from "../api/landing";
import { defaultLanguage } from "../i18n";
import Reveal from "../components/Reveal";
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
      {data?.map((section, index) => {
        const content = (() => {
          switch (section.type) {
            case "Hero":
              return <Hero section={section} />;
            case "OfferingsTeaser":
              return <OfferingsTeaser section={section} lang={lang} />;
            case "PostsTeaser":
              return <PostsTeaser section={section} lang={lang} />;
            case "TestimonialsTeaser":
              return <TestimonialsTeaser section={section} lang={lang} />;
            case "CtaBanner":
              return <CtaBanner section={section} />;
            default:
              return null;
          }
        })();

        if (content === null) return null;
        // Hero (index 0) is above the fold and reveals immediately; later
        // sections get a small stagger as the user scrolls to them.
        return (
          <Reveal key={section.type} delayMs={index === 0 ? 0 : 100}>
            {content}
          </Reveal>
        );
      })}
    </div>
  );
}
