import { useState, type FormEvent } from "react";
import { Link, useParams } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { blogApi } from "../api/blog";
import { useSeo } from "../hooks/useSeo";
import { defaultLanguage } from "../i18n";

export default function PostDetail() {
  const { lang = defaultLanguage, slug = "" } = useParams();
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const [form, setForm] = useState({ name: "", email: "", body: "" });
  const [submitted, setSubmitted] = useState(false);
  const [error, setError] = useState(false);

  const { data: post, isLoading } = useQuery({
    queryKey: ["post", lang, slug],
    queryFn: () => blogApi.detail(slug, lang),
  });

  const commentMutation = useMutation({
    mutationFn: () => blogApi.comment(slug, form),
    onSuccess: () => {
      setSubmitted(true);
      setError(false);
      setForm({ name: "", email: "", body: "" });
      void queryClient.invalidateQueries({ queryKey: ["post", lang, slug] });
    },
    onError: () => setError(true),
  });

  function handleSubmit(e: FormEvent) {
    e.preventDefault();
    commentMutation.mutate();
  }

  useSeo({ title: post?.title ?? t("common.loading"), description: post?.excerpt, image: post?.coverImageUrl });

  if (isLoading) return <p>{t("common.loading")}</p>;
  if (!post) return <p>{t("blog.noPosts")}</p>;

  return (
    <article>
      <Link to={`/${lang}/blog`} className="text-sm text-ink-muted no-underline hover:text-brand">
        ← {t("common.back")}
      </Link>
      <h1 className="mb-2 mt-3 text-3xl font-bold">{post.title}</h1>
      <div className="mb-6 flex flex-wrap gap-3 text-xs text-ink-faint">
        {post.categoryName && <span>{post.categoryName}</span>}
        <span>
          {post.viewCount} {t("blog.views")}
        </span>
        {post.publishedAt && <span>{new Date(post.publishedAt).toLocaleDateString()}</span>}
      </div>
      <div dangerouslySetInnerHTML={{ __html: post.bodyHtml }} />

      <section className="mt-10 border-t border-line pt-6">
        <h2 className="mb-4 text-xl font-bold">{t("blog.comments")}</h2>
        {post.comments.length === 0 && <p className="text-ink-muted">{t("blog.noComments")}</p>}
        <ul className="mb-6 flex flex-col gap-4">
          {post.comments.map((c) => (
            <li key={c.id} className="rounded-lg border border-line bg-card p-4">
              <p className="font-bold">{c.name}</p>
              <p className="text-ink-muted">{c.body}</p>
            </li>
          ))}
        </ul>

        <form onSubmit={handleSubmit} className="flex flex-col gap-3">
          <input
            required
            placeholder={t("blog.commentForm.name")}
            value={form.name}
            onChange={(e) => setForm({ ...form, name: e.target.value })}
            className="rounded-lg border border-line bg-card px-3 py-2"
          />
          <input
            required
            type="email"
            placeholder={t("blog.commentForm.email")}
            value={form.email}
            onChange={(e) => setForm({ ...form, email: e.target.value })}
            className="rounded-lg border border-line bg-card px-3 py-2"
          />
          <textarea
            required
            placeholder={t("blog.commentForm.body")}
            value={form.body}
            onChange={(e) => setForm({ ...form, body: e.target.value })}
            rows={4}
            className="rounded-lg border border-line bg-card px-3 py-2"
          />
          <button type="submit" className="self-start rounded-lg bg-brand px-4 py-2 font-bold text-white">
            {t("blog.commentForm.submit")}
          </button>
          {submitted && <p className="text-brand">{t("blog.commentSuccess")}</p>}
          {error && <p className="text-danger">{t("blog.commentError")}</p>}
        </form>
      </section>
    </article>
  );
}
