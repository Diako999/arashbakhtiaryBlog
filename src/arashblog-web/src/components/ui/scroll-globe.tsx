import { useCallback, useEffect, useMemo, useRef, useState, type ReactNode } from "react";
import Globe from "@/components/ui/globe";
import { GlassCard } from "@/components/ui/gradient-blob-card";
import { cn } from "@/lib/utils";

export interface ScrollGlobeSection {
  id: string;
  badge?: string;
  title: string;
  subtitle?: string;
  description?: string;
  align?: "start" | "center" | "end";
  features?: { title: string; description: string; onClick?: () => void }[];
  actions?: { label: string; variant: "primary" | "secondary"; onClick?: () => void }[];
  /** Replaces the features/actions block with arbitrary content (e.g. a card slider) while keeping the badge/title/description heading. */
  customContent?: ReactNode;
}

interface ScrollGlobeProps {
  sections: ScrollGlobeSection[];
  globeConfig?: {
    positions: {
      top: string;
      left: string;
      scale: number;
    }[];
  };
  className?: string;
}

const parsePercent = (str: string): number => parseFloat(str.replace("%", ""));

// RTL note: `left`/`top` here are plain positioning percentages for the
// fixed-position globe overlay (not text direction), so they stay physical
// regardless of document dir. Text alignment below uses logical
// start/center/end so it flips correctly under the site's RTL layout.
export function ScrollGlobe({ sections, globeConfig, className }: ScrollGlobeProps) {
  const [activeSection, setActiveSection] = useState(0);
  const [scrollProgress, setScrollProgress] = useState(0);
  const [globeTransform, setGlobeTransform] = useState("");
  const sectionRefs = useRef<(HTMLElement | null)[]>([]);
  const animationFrameId = useRef<number | undefined>(undefined);

  const resolvedGlobeConfig = useMemo(
    () =>
      globeConfig ?? {
        positions: sections.map((_, i) => ({
          top: "50%",
          left: `${20 + (i * 60) / Math.max(sections.length - 1, 1)}%`,
          scale: 1.2,
        })),
      },
    [globeConfig, sections.length],
  );

  const calculatedPositions = useMemo(
    () =>
      resolvedGlobeConfig.positions.map((pos) => ({
        top: parsePercent(pos.top),
        left: parsePercent(pos.left),
        scale: pos.scale,
      })),
    [resolvedGlobeConfig.positions],
  );

  const updateScrollPosition = useCallback(() => {
    const scrollTop = window.pageYOffset;
    const docHeight = document.documentElement.scrollHeight - window.innerHeight;
    const progress = docHeight > 0 ? Math.min(Math.max(scrollTop / docHeight, 0), 1) : 0;

    setScrollProgress(progress);

    const viewportCenter = window.innerHeight / 2;
    let newActiveSection = 0;
    let minDistance = Infinity;

    sectionRefs.current.forEach((ref, index) => {
      if (ref) {
        const rect = ref.getBoundingClientRect();
        const sectionCenter = rect.top + rect.height / 2;
        const distance = Math.abs(sectionCenter - viewportCenter);

        if (distance < minDistance) {
          minDistance = distance;
          newActiveSection = index;
        }
      }
    });

    const currentPos = calculatedPositions[newActiveSection];
    if (currentPos) {
      setGlobeTransform(
        `translate3d(${currentPos.left}vw, ${currentPos.top}vh, 0) translate3d(-50%, -50%, 0) scale3d(${currentPos.scale}, ${currentPos.scale}, 1)`,
      );
    }

    setActiveSection(newActiveSection);
  }, [calculatedPositions]);

  useEffect(() => {
    let ticking = false;

    const handleScroll = () => {
      if (!ticking) {
        animationFrameId.current = requestAnimationFrame(() => {
          updateScrollPosition();
          ticking = false;
        });
        ticking = true;
      }
    };

    window.addEventListener("scroll", handleScroll, { passive: true });
    updateScrollPosition();

    return () => {
      window.removeEventListener("scroll", handleScroll);
      if (animationFrameId.current) {
        cancelAnimationFrame(animationFrameId.current);
      }
    };
  }, [updateScrollPosition]);

  return (
    <div className={cn("relative w-full max-w-screen overflow-x-hidden bg-surface text-ink", className)}>
      {/* Progress bar */}
      <div className="fixed top-0 inset-x-0 h-0.5 bg-gradient-to-r from-line/20 via-line/40 to-line/20 z-50">
        <div
          className="h-full bg-gradient-to-r from-brand via-brand to-accent will-change-transform shadow-sm"
          style={{
            transform: `scaleX(${scrollProgress})`,
            transformOrigin: "inline-start",
            transition: "transform 0.15s ease-out",
          }}
        />
      </div>

      {/* Section nav dots */}
      <div className="hidden sm:flex fixed end-2 sm:end-4 lg:end-8 top-1/2 -translate-y-1/2 z-40">
        <div className="space-y-3 sm:space-y-4 lg:space-y-6">
          {sections.map((section, index) => (
            <div key={section.id} className="relative group">
              <div
                className={cn(
                  "absolute end-5 sm:end-6 lg:end-8 top-1/2 -translate-y-1/2",
                  "px-2 sm:px-3 lg:px-4 py-1 sm:py-1.5 lg:py-2 rounded-md sm:rounded-lg text-xs sm:text-sm font-medium whitespace-nowrap",
                  "bg-surface/95 backdrop-blur-md border border-line/60 shadow-xl z-50 transition-opacity duration-500",
                  activeSection === index ? "opacity-100" : "opacity-0 pointer-events-none",
                )}
              >
                <div className="flex items-center gap-1 sm:gap-1.5 lg:gap-2">
                  <div className="w-1 sm:w-1.5 lg:w-2 h-1 sm:h-1.5 lg:h-2 rounded-full bg-brand animate-pulse" />
                  <span>{section.badge ?? section.title}</span>
                </div>
              </div>

              <button
                type="button"
                onClick={() =>
                  sectionRefs.current[index]?.scrollIntoView({ behavior: "smooth", block: "center" })
                }
                className={cn(
                  "relative w-2 h-2 sm:w-2.5 sm:h-2.5 lg:w-3 lg:h-3 rounded-full border-2 transition-all duration-300 hover:scale-125",
                  activeSection === index
                    ? "bg-brand border-brand shadow-lg"
                    : "bg-transparent border-ink-faint/40 hover:border-brand/60 hover:bg-brand/10",
                )}
                aria-label={section.badge ?? section.title}
              />
            </div>
          ))}
        </div>
        <div className="absolute start-1/2 top-0 bottom-0 w-0.5 lg:w-px bg-gradient-to-b from-transparent via-brand/20 to-transparent -translate-x-1/2 -z-10" />
      </div>

      {/* Globe overlay */}
      <div
        className="fixed z-10 pointer-events-none will-change-transform transition-all duration-[1400ms] ease-[cubic-bezier(0.23,1,0.32,1)]"
        style={{
          transform: globeTransform,
          opacity: activeSection === sections.length - 1 ? 0.4 : 0.85,
        }}
      >
        <div className="scale-75 sm:scale-90 lg:scale-100">
          <Globe />
        </div>
      </div>

      {sections.map((section, index) => (
        <section
          key={section.id}
          ref={(el) => {
            sectionRefs.current[index] = el;
          }}
          className={cn(
            // Full-viewport height (for the scroll-jacking globe effect) only
            // from lg: up — on mobile/tablet it just forced huge empty gaps
            // around short content, so let sections size to their content there.
            "relative min-h-0 lg:min-h-svh flex flex-col justify-center px-4 sm:px-6 md:px-8 lg:px-12 z-20 py-10 sm:py-12 lg:py-16",
            "w-full max-w-full overflow-x-hidden",
            section.align === "center" && "items-center text-center",
            section.align === "end" && "items-end text-end",
            (!section.align || section.align === "start") && "items-start text-start",
          )}
        >
          <div className="w-full max-w-sm sm:max-w-lg md:max-w-2xl lg:max-w-4xl xl:max-w-5xl">
            <h1
              className={cn(
                "font-bold mb-8 sm:mb-12 tracking-tight",
                index === 0
                  ? "text-3xl sm:text-4xl md:text-5xl lg:text-6xl xl:text-7xl"
                  : "text-2xl sm:text-3xl md:text-4xl lg:text-5xl xl:text-6xl",
                // Must come after the text-size classes: Tailwind's text-{size}
                // utilities bundle their own line-height, so tailwind-merge
                // treats them as the same conflict group and keeps whichever
                // is last — putting leading-[] first silently drops it.
                "leading-[1.4]",
              )}
            >
              {section.subtitle ? (
                <div className="space-y-2 sm:space-y-3">
                  <div className="bg-gradient-to-r from-ink to-ink/80 bg-clip-text text-transparent">{section.title}</div>
                  <div className="text-ink-muted text-[0.6em] sm:text-[0.7em] font-medium tracking-wider">
                    {section.subtitle}
                  </div>
                </div>
              ) : (
                <div className="bg-gradient-to-r from-ink via-ink to-ink/80 bg-clip-text text-transparent">
                  {section.title}
                </div>
              )}
            </h1>

            {section.description && (
              <p
                className={cn(
                  "text-ink-muted mb-8 sm:mb-10 text-base sm:text-lg lg:text-xl font-light leading-relaxed",
                  section.align === "center" ? "max-w-full mx-auto" : "max-w-full",
                )}
              >
                {section.description}
              </p>
            )}

            {section.features && (
              <div className="grid gap-3 sm:gap-4 mb-8 sm:mb-10">
                {section.features.map((feature) => (
                  <GlassCard
                    key={feature.title}
                    onClick={feature.onClick}
                    className="w-full"
                    contentClassName="flex-row items-start gap-3 sm:gap-4 p-4 sm:p-5 lg:p-6"
                  >
                    <div className="w-1.5 sm:w-2 h-1.5 sm:h-2 rounded-full bg-brand/60 mt-1.5 sm:mt-2 group-hover:bg-brand transition-colors flex-shrink-0" />
                    <div className="flex-1 space-y-1.5 sm:space-y-2 min-w-0">
                      <h3 className="font-semibold text-ink text-base sm:text-lg">{feature.title}</h3>
                      <p className="text-ink-muted text-sm sm:text-base leading-relaxed">{feature.description}</p>
                    </div>
                  </GlassCard>
                ))}
              </div>
            )}

            {section.customContent && <div className="mb-8 sm:mb-10">{section.customContent}</div>}

            {section.actions && (
              <div
                className={cn(
                  "flex flex-col sm:flex-row flex-wrap gap-3 sm:gap-4",
                  section.align === "center" && "justify-center",
                  section.align === "end" && "justify-end",
                  (!section.align || section.align === "start") && "justify-start",
                )}
              >
                {section.actions.map((action) => (
                  <button
                    key={action.label}
                    type="button"
                    onClick={action.onClick}
                    className={cn(
                      "group relative px-6 sm:px-8 py-3 sm:py-4 rounded-lg sm:rounded-xl font-medium transition-all duration-300 hover:scale-[1.02] active:scale-[0.98] text-sm sm:text-base",
                      "hover:shadow-lg focus:outline-none focus:ring-2 focus:ring-brand/20 w-full sm:w-auto",
                      action.variant === "primary"
                        ? "bg-brand text-white hover:bg-brand/90 shadow-lg shadow-brand/20"
                        : "border-2 border-line bg-surface/50 backdrop-blur-sm hover:bg-surface-soft hover:border-brand/30 text-ink",
                    )}
                  >
                    {action.label}
                  </button>
                ))}
              </div>
            )}
          </div>
        </section>
      ))}
    </div>
  );
}
