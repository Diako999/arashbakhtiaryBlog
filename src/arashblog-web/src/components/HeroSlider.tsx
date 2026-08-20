import { useEffect, useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { siteApi } from "@/api/site";

const AUTOPLAY_MS = 5000;

// The literal first thing on the homepage — an admin-managed image
// carousel (Settings → Hero Slider) that sits above the scroll-driven
// hero content below it. Renders nothing when no slides are configured
// yet, so a fresh install isn't stuck with an empty gray box.
export default function HeroSlider() {
  const { data: slides } = useQuery({ queryKey: ["hero-slides"], queryFn: siteApi.heroSlides });
  const [index, setIndex] = useState(0);

  useEffect(() => {
    if (!slides || slides.length < 2) return;
    const timer = setInterval(() => setIndex((i) => (i + 1) % slides.length), AUTOPLAY_MS);
    return () => clearInterval(timer);
  }, [slides]);

  useEffect(() => {
    setIndex(0);
  }, [slides?.length]);

  if (!slides || slides.length === 0) return null;

  return (
    <div className="relative mb-8 w-full overflow-hidden rounded-2xl border border-line">
      <div className="relative h-56 w-full sm:h-80 lg:h-[28rem]">
        {slides.map((slide, i) => {
          const image = (
            <img
              src={slide.imageUrl}
              alt=""
              className={
                "absolute inset-0 h-full w-full object-cover transition-opacity duration-700 ease-out " +
                (i === index ? "opacity-100" : "opacity-0")
              }
            />
          );
          return slide.linkUrl ? (
            <a key={slide.imageUrl + i} href={slide.linkUrl} target="_blank" rel="noreferrer noopener" aria-hidden={i !== index} tabIndex={i === index ? 0 : -1}>
              {image}
            </a>
          ) : (
            <div key={slide.imageUrl + i} aria-hidden={i !== index}>
              {image}
            </div>
          );
        })}
      </div>

      {slides.length > 1 && (
        <div className="absolute inset-x-0 bottom-3 flex justify-center gap-2">
          {slides.map((slide, i) => (
            <button
              key={slide.imageUrl + i}
              type="button"
              onClick={() => setIndex(i)}
              aria-label={`${i + 1}`}
              className={"h-2 rounded-full transition-all " + (i === index ? "w-6 bg-white" : "w-2 bg-white/50")}
            />
          ))}
        </div>
      )}
    </div>
  );
}
