import type { LandingSectionDto } from "../../api/types";

export default function Hero({ section }: { section: LandingSectionDto }) {
  return (
    <section className="relative flex flex-wrap items-center gap-10 md:gap-14">
      <div
        aria-hidden="true"
        className="pointer-events-none absolute -top-16 -start-24 h-72 w-72 rounded-full"
        style={{ background: "radial-gradient(circle, var(--brand-soft), transparent 70%)" }}
      />

      <div className="flex min-w-[280px] flex-1 flex-col gap-5">
        <h1 className="max-w-xl text-4xl font-extrabold leading-snug sm:text-5xl">{section.heading}</h1>
        {section.subheading && (
          <p className="max-w-lg text-base leading-8 text-ink-muted sm:text-lg">{section.subheading}</p>
        )}
        <div className="mt-2 flex flex-wrap gap-3">
          {section.primaryCtaText && section.primaryCtaUrl && (
            <a href={section.primaryCtaUrl} className="btn-primary no-underline">
              {section.primaryCtaText}
            </a>
          )}
          {section.secondaryCtaText && section.secondaryCtaUrl && (
            <a href={section.secondaryCtaUrl} className="btn-secondary no-underline">
              {section.secondaryCtaText}
            </a>
          )}
        </div>
      </div>

      <div className="min-w-[240px] flex-1 sm:max-w-[400px]">
        <div
          className="aspect-[4/5] w-full rounded-[28px] border border-line"
          style={{
            boxShadow: "0 30px 60px -18px rgba(0, 0, 0, 0.35)",
            ...(section.imageUrl
              ? { backgroundImage: `url(${section.imageUrl})`, backgroundSize: "cover", backgroundPosition: "center" }
              : { background: "linear-gradient(135deg, var(--brand) 0%, var(--accent) 100%)" }),
          }}
        />
      </div>
    </section>
  );
}
