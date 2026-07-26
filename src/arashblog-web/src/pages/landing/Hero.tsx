import type { LandingSectionDto } from "../../api/types";

export default function Hero({ section }: { section: LandingSectionDto }) {
  return (
    <section className="relative flex flex-col items-center gap-10 py-6 md:flex-row md:gap-14 md:py-12">
      <div className="flex min-w-0 flex-1 flex-col items-center gap-5 text-center md:min-w-[280px] md:items-start md:pe-10 md:text-start lg:pe-16">
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
