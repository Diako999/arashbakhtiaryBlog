import { GithubIcon, LinkedinIcon, XIcon, YoutubeIcon } from "@/components/ui/social-icons";
import { GlassCard } from "@/components/ui/gradient-blob-card";
import { cn } from "@/lib/utils";

export interface ProfileCardProps {
  name: string;
  title: string;
  description: string;
  imageUrl?: string | null;
  githubUrl?: string;
  twitterUrl?: string;
  youtubeUrl?: string;
  linkedinUrl?: string;
  className?: string;
}

// Adapted from a Next.js/framer-motion demo: next/image -> plain <img>,
// next/link -> plain <a> (these are external profile links, not app routes),
// framer-motion -> a CSS fade-in-up (the original only animated on mount,
// not on scroll-into-view, so a plain CSS animation matches it exactly).
// No real headshot is wired up (no bio-photo field in the backend, and this
// sandbox couldn't even reach Unsplash to borrow a stock one) — falls back
// to an initial-letter avatar, same "graceful placeholder" pattern used for
// posts/offerings without a cover image.
export function ProfileCard({
  name,
  title,
  description,
  imageUrl,
  githubUrl,
  twitterUrl,
  youtubeUrl,
  linkedinUrl,
  className,
}: ProfileCardProps) {
  const socialLinks = [
    { url: githubUrl, Icon: GithubIcon, label: "GitHub" },
    { url: twitterUrl, Icon: XIcon, label: "Twitter" },
    { url: youtubeUrl, Icon: YoutubeIcon, label: "YouTube" },
    { url: linkedinUrl, Icon: LinkedinIcon, label: "LinkedIn" },
  ].filter((s): s is typeof s & { url: string } => !!s.url);

  const photo = (
    <div className="flex h-full w-full items-center justify-center bg-gradient-to-br from-brand/30 to-accent/30">
      {imageUrl ? (
        <img src={imageUrl} alt={name} className="h-full w-full object-cover" draggable={false} />
      ) : (
        <span className="text-6xl font-bold text-brand">{name.charAt(0)}</span>
      )}
    </div>
  );

  const socialRow = socialLinks.length > 0 && (
    <div className="flex justify-center gap-3 sm:justify-start">
      {socialLinks.map(({ url, Icon, label }) => (
        <a
          key={label}
          href={url}
          target="_blank"
          rel="noreferrer noopener"
          aria-label={label}
          className="flex h-11 w-11 items-center justify-center rounded-full bg-ink text-surface transition-transform hover:scale-105"
        >
          <Icon size={18} />
        </a>
      ))}
    </div>
  );

  return (
    <div className={cn("mx-auto w-full max-w-4xl animate-[fade-in-up_0.5s_ease-out]", className)}>
      {/* Desktop: square photo with an overlapping glass card */}
      <div className="hidden items-center sm:flex">
        <div className="h-72 w-72 shrink-0 overflow-hidden rounded-3xl lg:h-80 lg:w-80">{photo}</div>
        <GlassCard className="-ms-16 z-10 flex-1" contentClassName="p-8">
          <h2 className="mb-1 text-2xl font-bold text-ink">{name}</h2>
          <p className="mb-4 text-sm font-medium text-ink-muted">{title}</p>
          <p className="mb-6 leading-relaxed text-ink-muted">{description}</p>
          {socialRow}
        </GlassCard>
      </div>

      {/* Mobile: stacked, centered */}
      <div className="text-center sm:hidden">
        <div className="mx-auto mb-6 aspect-square w-full max-w-xs overflow-hidden rounded-3xl">{photo}</div>
        <h2 className="mb-1 text-xl font-bold text-ink">{name}</h2>
        <p className="mb-4 text-sm font-medium text-ink-muted">{title}</p>
        <p className="mb-6 leading-relaxed text-ink-muted">{description}</p>
        {socialRow}
      </div>
    </div>
  );
}
