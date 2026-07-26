import type { LandingSectionDto } from "../../api/types";

export default function Hero({ section }: { section: LandingSectionDto }) {
  return (
    <section className="relative flex flex-col items-center gap-10 overflow-hidden py-6 md:flex-row md:gap-14 md:py-12">
      <div
        aria-hidden="true"
        className="hero-blob-float pointer-events-none absolute -top-16 -start-24 h-72 w-72 rounded-full"
        style={{ background: "radial-gradient(circle, var(--brand-soft), transparent 70%)" }}
      />
      <div
        aria-hidden="true"
        className="hero-blob-float pointer-events-none absolute -bottom-20 -end-16 h-64 w-64 rounded-full"
        style={{ background: "radial-gradient(circle, var(--brand-soft), transparent 70%)", animationDelay: "2.5s" }}
      />

      <div className="flex min-w-0 flex-1 flex-col items-center gap-5 text-center md:min-w-[280px] md:items-start md:pe-4 md:text-start">
        <h1 className="gradient-text max-w-xl text-4xl font-extrabold leading-snug sm:text-5xl">{section.heading}</h1>
        {section.subheading && (
          <p className="max-w-lg text-base leading-8 text-ink-muted sm:text-lg">{section.subheading}</p>
        )}
        {(section.primaryCtaText || section.secondaryCtaText) && (
          <div className="mt-2 flex flex-wrap justify-center gap-3 md:justify-start">
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
        )}
      </div>

      <div className="w-full max-w-[320px] shrink-0 md:min-w-[260px] md:max-w-[360px]">
        <div
          className="aspect-[4/3] w-full rounded-[28px] border border-line transition-transform duration-300 ease-out hover:scale-[1.02]"
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
