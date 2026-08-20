import type { ElementType, ReactNode } from "react";
import { useTranslation } from "react-i18next";
import { ArrowLeft } from "lucide-react";
import { cn } from "@/lib/utils";

interface GlassCardProps {
  as?: ElementType;
  children: ReactNode;
  onClick?: () => void;
  /** Outer shell: sizing, shrink, snap-align — not padding (that's swallowed by the glass panel's own edge). */
  className?: string;
  /** Inner glass panel: padding/layout for the actual content. Defaults to "p-5". */
  contentClassName?: string;
}

// The site's single card "shell": a blurred animated gradient blob behind a
// frosted glass panel. Used everywhere a bordered content-preview card
// appeared before (posts, offerings, testimonials, leads, hero teasers) so
// every card on the site shares the same look. Theme tokens only (no
// hardcoded light/dark shadow colors) so it follows brand color + dark mode.
export function GlassCard({ as, children, onClick, className, contentClassName }: GlassCardProps) {
  const Tag = as ?? (onClick ? "button" : "div");

  return (
    <Tag
      type={Tag === "button" ? "button" : undefined}
      onClick={onClick}
      className={cn(
        "group relative overflow-hidden rounded-2xl text-start",
        "shadow-xl shadow-black/10 transition-transform duration-300 hover:-translate-y-1",
        onClick && "cursor-pointer focus:outline-none focus:ring-2 focus:ring-brand/30",
        className,
      )}
    >
      <div
        className="absolute top-1/2 left-1/2 h-40 w-40 rounded-full opacity-70 blur-2xl"
        style={{
          background: "linear-gradient(135deg, var(--brand), var(--accent))",
          animation: "blob 8s ease-in-out infinite",
        }}
      />
      <div
        className={cn(
          "relative z-10 flex h-full flex-col rounded-2xl border border-line/60 bg-card/80 backdrop-blur-xl",
          contentClassName ?? "p-5",
        )}
      >
        {children}
      </div>
    </Tag>
  );
}

interface GradientBlobCardProps {
  title: string;
  coverImageUrl?: string | null;
  badge?: string;
  onClick?: () => void;
  className?: string;
}

// The compact teaser card used by the hero's post slider — image on top,
// title, "read more" link. A soft brand-gradient fills the image slot when
// the post has no cover image, so every card still gets one.
export function GradientBlobCard({ title, coverImageUrl, badge, onClick, className }: GradientBlobCardProps) {
  const { t } = useTranslation();

  return (
    <GlassCard
      onClick={onClick}
      className={cn("w-64 sm:w-72 shrink-0 snap-start", className)}
      contentClassName="p-0"
    >
      <div className="h-36 w-full shrink-0 overflow-hidden rounded-t-2xl bg-gradient-to-br from-brand/25 to-accent/25">
        {coverImageUrl && <img src={coverImageUrl} alt="" className="h-full w-full object-cover" />}
      </div>
      <div className="flex flex-1 flex-col gap-2 p-4">
        {badge && <span className="text-xs font-medium text-brand">{badge}</span>}
        <h3 className="line-clamp-2 font-semibold text-ink">{title}</h3>
        {onClick && (
          <span className="mt-auto flex items-center gap-1 text-sm font-medium text-brand">
            {t("blog.readMore")}
            <ArrowLeft size={14} />
          </span>
        )}
      </div>
    </GlassCard>
  );
}
