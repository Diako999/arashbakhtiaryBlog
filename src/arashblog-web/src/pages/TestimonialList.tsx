import { useParams } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { useQuery } from "@tanstack/react-query";
import { testimonialsApi } from "../api/testimonials";
import { defaultLanguage } from "../i18n";

export default function TestimonialList() {
  const { lang = defaultLanguage } = useParams();
  const { t } = useTranslation();

  const { data, isLoading } = useQuery({
    queryKey: ["testimonials", lang],
    queryFn: () => testimonialsApi.list(lang),
  });

  if (isLoading) return <p>{t("common.loading")}</p>;
  if (data?.length === 0) return <p>{t("testimonials.none")}</p>;

  return (
    <div className="grid grid-cols-1 gap-6 sm:grid-cols-2">
      {data?.map((testimonial, index) => (
        <blockquote key={index} className="rounded-xl border border-line bg-card p-5">
          <p className="mb-4 text-ink-muted">"{testimonial.quote}"</p>
          <footer className="flex items-center gap-3">
            {testimonial.photoUrl && (
              <img src={testimonial.photoUrl} alt="" className="h-10 w-10 rounded-full object-cover" />
            )}
            <div>
              <p className="font-bold">{testimonial.authorName}</p>
              {testimonial.authorRole && <p className="text-xs text-ink-faint">{testimonial.authorRole}</p>}
            </div>
          </footer>
        </blockquote>
      ))}
    </div>
  );
}
