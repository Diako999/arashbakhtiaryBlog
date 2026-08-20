import type { SVGProps } from "react";

// lucide-react dropped all brand/trademark icons (Instagram, Linkedin,
// Twitter/X, Telegram, WhatsApp) — these are small generic pictograms
// standing in for each platform, not reproductions of their logos.
type IconProps = SVGProps<SVGSVGElement> & { size?: number };

function base({ size = 18, ...props }: IconProps) {
  return { width: size, height: size, viewBox: "0 0 24 24", fill: "none", stroke: "currentColor", strokeWidth: 1.8, strokeLinecap: "round" as const, strokeLinejoin: "round" as const, ...props };
}

export function InstagramIcon(props: IconProps) {
  return (
    <svg {...base(props)}>
      <rect x="3" y="3" width="18" height="18" rx="5" />
      <circle cx="12" cy="12" r="4" />
      <circle cx="17.5" cy="6.5" r="1" fill="currentColor" stroke="none" />
    </svg>
  );
}

export function TelegramIcon(props: IconProps) {
  return (
    <svg {...base(props)}>
      <path d="M21 4 3 11.5l6 2m12-9.5-3.5 16-8.5-6.5M21 4 9 13.5" />
    </svg>
  );
}

export function XIcon(props: IconProps) {
  return (
    <svg {...base(props)}>
      <path d="M4 4l16 16M20 4 4 20" />
    </svg>
  );
}

export function LinkedinIcon(props: IconProps) {
  return (
    <svg {...base(props)}>
      <rect x="3" y="3" width="18" height="18" rx="3" />
      <path d="M8 10v7M8 7v.01M12 17v-4.5a2.5 2.5 0 0 1 5 0V17" />
    </svg>
  );
}

export function WhatsappIcon(props: IconProps) {
  return (
    <svg {...base(props)}>
      <path d="M6 18l-1.5 3.5L8 20a8 8 0 1 0-3-3Z" />
      <path d="M8.5 9.5c0 3.5 2.5 6 6 6l1-2-2.5-1-1 1c-1-.6-2-1.6-2.5-2.5l1-1-1-2.5Z" />
    </svg>
  );
}

export function GithubIcon(props: IconProps) {
  return (
    <svg {...base(props)}>
      <path d="M9 19c-4.3 1.4-4.3-2.5-6-3m12 5v-3.5c0-1 .1-1.4-.5-2 2-.2 4.5-1 4.5-4.5 0-1-.5-2-1-2.5.5-1 .5-2 0-3 0 0-1 0-2.5 1a8 8 0 0 0-5 0C9 5 8 5 8 5c-.5 1-.5 2 0 3-.5.5-1 1.5-1 2.5 0 3.5 2.5 4.3 4.5 4.5-.6.6-.5 1-.5 2V21" />
    </svg>
  );
}

export function YoutubeIcon(props: IconProps) {
  return (
    <svg {...base(props)}>
      <rect x="2.5" y="6" width="19" height="12" rx="3" />
      <path d="M10.5 9.5v5l4.5-2.5-4.5-2.5Z" fill="currentColor" stroke="none" />
    </svg>
  );
}
