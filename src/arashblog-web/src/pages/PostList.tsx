import { useState, type FormEvent } from "react";
import { Link, useParams, useSearchParams } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { useQuery } from "@tanstack/react-query";
import { blogApi } from "../api/blog";
import { defaultLanguage } from "../i18n";
import PostCard from "../components/PostCard";

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
        <button type="submit" className="btn-primary">
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

      <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3">
        {data?.items.map((post) => (
          <PostCard key={post.slug} post={post} lang={lang} />
        ))}
      </div>
    </div>
  );
}
