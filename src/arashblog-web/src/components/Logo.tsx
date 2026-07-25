// Gradient badge fallback shown wherever no custom logo has been uploaded
// via Settings (siteSettings.logoUrl) — ported from the design handoff's
// Logo.dc.html: radius scales with size (28% of size), a plain white "A"
// glyph over a brand→accent gradient.
export default function Logo({ size = 36 }: { size?: number }) {
  const radius = Math.round(size * 0.28);

  return (
    <div
      style={{
        width: size,
        height: size,
        borderRadius: radius,
        background: "linear-gradient(135deg, var(--brand) 0%, var(--accent) 100%)",
        boxShadow: "0 4px 14px -4px rgba(0, 0, 0, 0.45)",
      }}
      className="flex shrink-0 items-center justify-center"
      aria-hidden="true"
    >
      <svg viewBox="0 0 24 24" width={size * 0.58} height={size * 0.58} fill="none">
        <path
          d="M12 3L20 21H16.5L14.8 17H9.2L7.5 21H4L12 3ZM12 8.5L10 13.5H14L12 8.5Z"
          fill="#FFFFFF"
        />
      </svg>
    </div>
  );
}
