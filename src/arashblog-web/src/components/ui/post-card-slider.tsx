import { useRef } from "react";
import { ChevronLeft, ChevronRight } from "lucide-react";
import { GradientBlobCard } from "@/components/ui/gradient-blob-card";
import { cn } from "@/lib/utils";

export interface PostCardSliderItem {
  key: string;
  title: string;
  coverImageUrl?: string | null;
  badge?: string;
  onClick?: () => void;
}

interface PostCardSliderProps {
  items: PostCardSliderItem[];
  viewAllLabel: string;
  onViewAll: () => void;
  className?: string;
}

export function PostCardSlider({ items, viewAllLabel, onViewAll, className }: PostCardSliderProps) {
  const trackRef = useRef<HTMLDivElement>(null);

  function scrollByCard(direction: 1 | -1) {
    const track = trackRef.current;
    if (!track) return;
    const cardWidth = track.firstElementChild?.clientWidth ?? 240;
    // RTL: scrollLeft grows negative toward the start, so "forward" (next
    // card, toward the end) is the same sign as a plain LTR "next".
    track.scrollBy({ left: direction * (cardWidth + 16), behavior: "smooth" });
  }

  return (
    <div className={cn("w-full", className)}>
      <div className="relative">
        <div
          ref={trackRef}
          className="flex gap-4 overflow-x-auto scroll-smooth snap-x snap-mandatory pb-4 [scrollbar-width:none] [&::-webkit-scrollbar]:hidden"
        >
          {items.map((item) => (
            <GradientBlobCard
              key={item.key}
              title={item.title}
              coverImageUrl={item.coverImageUrl}
              badge={item.badge}
              onClick={item.onClick}
            />
          ))}
        </div>

        {items.length > 1 && (
          <div className="mt-2 hidden justify-center gap-2 sm:flex">
            <button
              type="button"
              onClick={() => scrollByCard(-1)}
              aria-label="Previous"
              className="rounded-full border border-line bg-card p-2 text-ink-muted hover:bg-surface-soft hover:text-ink"
            >
              <ChevronRight size={18} />
            </button>
            <button
              type="button"
              onClick={() => scrollByCard(1)}
              aria-label="Next"
              className="rounded-full border border-line bg-card p-2 text-ink-muted hover:bg-surface-soft hover:text-ink"
            >
              <ChevronLeft size={18} />
            </button>
          </div>
        )}
      </div>

      <div className="mt-4 flex justify-center">
        <button
          type="button"
          onClick={onViewAll}
          className="rounded-lg border-2 border-line bg-surface/50 px-6 py-2.5 text-sm font-medium text-ink backdrop-blur-sm hover:border-brand/30 hover:bg-surface-soft"
        >
          {viewAllLabel}
        </button>
      </div>
    </div>
  );
}
