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
        // Checked here, not just inside each *Teaser component: those
        // components already return null when empty, but by then they're
        // already wrapped in <Reveal>, and an empty wrapper still consumes
        // a full gap-16/24 slot in this flex column — reads as a large
        // dead blank space between sections that do have content.
        const content = (() => {
          switch (section.type) {
            case "Hero":
              return <Hero section={section} />;
            case "OfferingsTeaser":
              if (!section.offerings || section.offerings.length === 0) return null;
              return <OfferingsTeaser section={section} lang={lang} />;
            case "PostsTeaser":
              if (!section.posts || section.posts.length === 0) return null;
              return <PostsTeaser section={section} lang={lang} />;
            case "TestimonialsTeaser":
              if (!section.testimonials || section.testimonials.length === 0) return null;
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
