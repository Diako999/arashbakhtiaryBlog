import type { CSSProperties } from "react";
import { Link } from "react-router-dom";
import { useTranslation } from "react-i18next";
import type { PostSummary } from "../api/types";

export default function PostCard({ post, lang }: { post: PostSummary; lang: string }) {
  const { t } = useTranslation();

  // Per-post overrides: background wins via inline style beating the
  // bg-card class outright. Text/accent must override --color-ink/
  // --color-brand directly (the vars .text-ink/.text-brand actually
  // consume), not --text/--brand — those are only aliased into
  // --color-ink/--color-brand at :root (`--color-ink: var(--text)`), and
  // that alias resolves once at :root, so overriding --text on a
  // descendant like this article never re-triggers it. --brand is kept
  // too since the cover-image gradient fallback below references it directly.
  const overrideStyle = {
    ...(post.bgColor ? { backgroundColor: post.bgColor } : {}),
    ...(post.textColor ? { "--color-ink": post.textColor } : {}),
    ...(post.accentColor ? { "--brand": post.accentColor, "--color-brand": post.accentColor } : {}),
  } as CSSProperties;

  return (
    <article className="card-hover overflow-hidden border border-line bg-card" style={overrideStyle}>
      <div
        className="aspect-[16/10] w-full"
        style={
          post.coverImageUrl
            ? { backgroundImage: `url(${post.coverImageUrl})`, backgroundSize: "cover", backgroundPosition: "center" }
            : { background: "linear-gradient(135deg, var(--brand) 0%, var(--accent) 100%)" }
        }
      />
      <div className="flex flex-col gap-2 px-5 py-4">
        {post.categoryName && <span className="text-sm font-bold text-brand">{post.categoryName}</span>}
        <h3 className="text-lg font-bold">
          <Link to={`/${lang}/blog/${encodeURIComponent(post.slug)}`} className="text-ink no-underline hover:text-brand">
            {post.title}
          </Link>
        </h3>
        {post.excerpt && <p className="text-sm leading-7 text-ink-muted">{post.excerpt}</p>}
        {post.tags.length > 0 && (
          <div className="flex flex-wrap gap-2 text-xs text-ink-faint">
            {post.tags.map((tagSlug) => (
              <Link
                key={tagSlug}
                to={`/${lang}/blog?tag=${encodeURIComponent(tagSlug)}`}
                className="text-ink-faint no-underline hover:text-brand"
              >
                #{tagSlug}
              </Link>
            ))}
          </div>
        )}
        <Link
          to={`/${lang}/blog/${encodeURIComponent(post.slug)}`}
          className="mt-1 text-sm font-bold text-brand no-underline"
        >
          {t("blog.readMore")}
        </Link>
      </div>
    </article>
  );
}
