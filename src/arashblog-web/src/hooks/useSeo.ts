import { useEffect } from "react";
import { useQuery } from "@tanstack/react-query";
import { siteApi } from "../api/site";

interface SeoOptions {
  title: string;
  description?: string;
  image?: string | null;
}

// Client-side head-tag management — no SSR in this SPA, so search engines
// only see these tags if they execute JS (Googlebot does, per current
// crawler behavior). A pragmatic stand-in for the Django project's
// server-rendered canonical/OG tags in base/layout.html without pulling
// in a full SSR framework just for meta tags.
function upsertMeta(attr: "name" | "property", key: string, content: string) {
  const selector = `meta[${attr}="${key}"]`;
  let el = document.querySelector<HTMLMetaElement>(selector);
  if (!el) {
    el = document.createElement("meta");
    el.setAttribute(attr, key);
    document.head.appendChild(el);
  }
  el.setAttribute("content", content);
}

export function useSeo({ title, description, image }: SeoOptions) {
  // Same query key Layout already fetches with — React Query dedupes, no
  // extra request — just so every page's tab title gets the "Page | Site"
  // suffix without each caller needing to know the site name.
  const { data: siteSettings } = useQuery({ queryKey: ["site-settings"], queryFn: siteApi.settings });
  const fullTitle = siteSettings?.siteName ? `${title} | ${siteSettings.siteName}` : title;

  useEffect(() => {
    document.title = fullTitle;
    upsertMeta("property", "og:title", fullTitle);
    upsertMeta("name", "twitter:title", fullTitle);
    upsertMeta("property", "og:url", window.location.href);
    upsertMeta("property", "og:type", "website");

    if (description) {
      upsertMeta("name", "description", description);
      upsertMeta("property", "og:description", description);
      upsertMeta("name", "twitter:description", description);
    }

    if (image) {
      upsertMeta("property", "og:image", image);
      upsertMeta("name", "twitter:card", "summary_large_image");
    }

    let canonical = document.querySelector<HTMLLinkElement>('link[rel="canonical"]');
    if (!canonical) {
      canonical = document.createElement("link");
      canonical.setAttribute("rel", "canonical");
      document.head.appendChild(canonical);
    }
    canonical.setAttribute("href", window.location.href);
  }, [fullTitle, description, image]);
}
