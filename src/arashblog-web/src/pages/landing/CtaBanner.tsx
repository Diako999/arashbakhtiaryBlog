import type { LandingSectionDto } from "../../api/types";

export default function CtaBanner({ section }: { section: LandingSectionDto }) {
  return (
    <section
      className="flex flex-wrap items-center justify-between gap-6 rounded-[22px] border border-line p-8 transition-[border-color,transform] duration-300 ease-out hover:-translate-y-1 hover:border-brand sm:p-11"
      style={{ background: "linear-gradient(135deg, var(--brand-soft), transparent)" }}
    >
      <div className="flex flex-col gap-2">
        <h2 className="text-xl font-bold sm:text-2xl">{section.heading}</h2>
        {section.subheading && <p className="text-ink-muted">{section.subheading}</p>}
      </div>
      {section.primaryCtaText && section.primaryCtaUrl && (
        <a href={section.primaryCtaUrl} className="btn-primary no-underline">
          {section.primaryCtaText}
        </a>
      )}
    </section>
  );
}
