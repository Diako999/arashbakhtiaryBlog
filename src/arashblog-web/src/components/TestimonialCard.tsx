import type { TestimonialDto } from "../api/types";

export default function TestimonialCard({ testimonial }: { testimonial: TestimonialDto }) {
  return (
    <blockquote className="card-hover border border-line bg-card p-6">
      <p className="mb-4 leading-8 text-ink-muted">&ldquo;{testimonial.quote}&rdquo;</p>
      <footer className="flex items-center gap-3 border-t border-line pt-4">
        {testimonial.photoUrl ? (
          <img src={testimonial.photoUrl} alt="" className="h-11 w-11 rounded-full object-cover" />
        ) : (
          <div
            className="h-11 w-11 shrink-0 rounded-full"
            style={{ background: "linear-gradient(135deg, var(--brand), var(--accent))" }}
          />
        )}
        <div>
          <p className="font-bold">{testimonial.authorName}</p>
          {testimonial.authorRole && <p className="text-xs text-ink-muted">{testimonial.authorRole}</p>}
        </div>
      </footer>
    </blockquote>
  );
}
