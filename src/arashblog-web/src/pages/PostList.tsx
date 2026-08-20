import { useState, type FormEvent } from "react";
import { Link, useParams, useSearchParams } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { useQuery } from "@tanstack/react-query";
import { ArrowLeft } from "lucide-react";
import { blogApi } from "../api/blog";
import { defaultLanguage } from "../i18n";
import { GlassCard } from "@/components/ui/gradient-blob-card";
import { useSeo } from "../hooks/useSeo";

export default function PostList() {
  const { lang = defaultLanguage } = useParams();
  const { t } = useTranslation();
  useSeo({ title: t("nav.blog") });
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

  const totalPages = data ? Math.max(Math.ceil(data.totalCount / data.pageSize), 1) : 1;

  function goToPage(nextPage: number) {
    const next = new URLSearchParams(searchParams);
    if (nextPage <= 1) next.delete("page");
    else next.set("page", String(nextPage));
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
          <GlassCard key={post.slug} as="article" className="w-full" contentClassName="p-0">
            <div className="h-48 w-full shrink-0 overflow-hidden rounded-t-2xl bg-gradient-to-br from-brand/25 to-accent/25 sm:h-64">
              {post.coverImageUrl && <img src={post.coverImageUrl} alt="" className="h-full w-full object-cover" />}
            </div>
            <div className="flex flex-col gap-2 p-4 sm:p-5">
              {post.categoryName && <span className="text-xs font-medium text-brand">{post.categoryName}</span>}
              <h2 className="text-lg font-semibold">
                <Link
                  to={`/${lang}/blog/${encodeURIComponent(post.slug)}`}
                  className="text-ink no-underline hover:text-brand"
                >
                  {post.title}
                </Link>
              </h2>
              {post.excerpt && <p className="text-ink-muted">{post.excerpt}</p>}
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
                className="mt-1 flex items-center gap-1 text-sm font-medium text-brand no-underline"
              >
                {t("blog.readMore")}
                <ArrowLeft size={14} />
              </Link>
            </div>
          </GlassCard>
        ))}
      </div>

      {!isLoading && data && data.items.length > 0 && totalPages > 1 && (
        <div className="mt-6 flex items-center justify-center gap-3">
          <button
            type="button"
            disabled={page <= 1}
            onClick={() => goToPage(page - 1)}
            className="rounded-lg border border-line bg-card px-4 py-2 text-sm disabled:opacity-40"
          >
            {t("blog.prevPage")}
          </button>
          <span className="text-sm text-ink-muted">{t("blog.pageOf", { page, totalPages })}</span>
          <button
            type="button"
            disabled={page >= totalPages}
            onClick={() => goToPage(page + 1)}
            className="rounded-lg border border-line bg-card px-4 py-2 text-sm disabled:opacity-40"
          >
            {t("blog.nextPage")}
          </button>
        </div>
      )}
    </div>
  );
}
