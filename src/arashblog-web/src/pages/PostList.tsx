import { useState, type FormEvent } from "react";
import { Link, useParams, useSearchParams } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { useQuery } from "@tanstack/react-query";
import { blogApi } from "../api/blog";
import { defaultLanguage } from "../i18n";

export default function PostList() {
  const { lang = defaultLanguage } = useParams();
  const { t } = useTranslation();
  const [searchParams, setSearchParams] = useSearchParams();
  const [q, setQ] = useState(searchParams.get("q") ?? "");

  const category = searchParams.get("category") ?? undefined;
  const tag = searchParams.get("tag") ?? undefined;
  const page = Number(searchParams.get("page") ?? "1");

  const { data, isLoading } = useQuery({
    queryKey: ["posts", lang, category, tag, searchParams.get("q"), page],
    queryFn: () => blogApi.list({ lang, category, tag, q: searchParams.get("q") ?? undefined, page }),
  });

  const { data: categories } = useQuery({
    queryKey: ["categories", lang],
    queryFn: () => blogApi.categories(lang),
  });

  function submitSearch(e: FormEvent) {
    e.preventDefault();
    const next = new URLSearchParams(searchParams);
    if (q) next.set("q", q);
    else next.delete("q");
    next.delete("page");
    setSearchParams(next);
  }

  return (
    <div>
      <form onSubmit={submitSearch} className="mb-6 flex gap-2">
        <input
          value={q}
          onChange={(e) => setQ(e.target.value)}
          placeholder={t("blog.searchPlaceholder")}
          className="flex-1 rounded-lg border border-line bg-card px-3 py-2"
        />
        <button type="submit" className="rounded-lg bg-brand px-4 py-2 font-bold text-white">
          {t("blog.searchPlaceholder")}
        </button>
      </form>

      {categories && categories.length > 0 && (
        <div className="mb-6 flex flex-wrap gap-3 text-sm">
          <Link
            to={`/${lang}/blog`}
            className={!category ? "font-bold text-brand no-underline" : "text-ink-muted no-underline hover:text-brand"}
          >
            {t("blog.allCategories")}
          </Link>
          {categories.map((c) => (
            <Link
              key={c.slug}
              to={`/${lang}/blog?category=${encodeURIComponent(c.slug)}`}
              className={
                category === c.slug ? "font-bold text-brand no-underline" : "text-ink-muted no-underline hover:text-brand"
              }
            >
              {c.name}
            </Link>
          ))}
        </div>
      )}

      {isLoading && <p>{t("common.loading")}</p>}
      {!isLoading && data?.items.length === 0 && <p>{t("blog.noPosts")}</p>}

      <div className="flex flex-col gap-6">
        {data?.items.map((post) => (
          <article key={post.slug} className="rounded-xl border border-line bg-card p-5">
            <h2 className="mb-2 text-xl font-bold">
              <Link
                to={`/${lang}/blog/${encodeURIComponent(post.slug)}`}
                className="text-ink no-underline hover:text-brand"
              >
                {post.title}
              </Link>
            </h2>
            {post.excerpt && <p className="text-ink-muted">{post.excerpt}</p>}
            <div className="mt-3 flex flex-wrap gap-2 text-xs text-ink-faint">
              {post.categoryName && <span>{post.categoryName}</span>}
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
          </article>
        ))}
      </div>
    </div>
  );
}
