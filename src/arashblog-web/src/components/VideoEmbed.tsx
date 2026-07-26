// Admins paste a plain URL (YouTube/Vimeo watch link, or a direct video
// file link) rather than uploading a video file — real video hosting needs
// far more storage/bandwidth than this app's image/document uploads, so a
// link-based embed is what's actually offered. Normalizes the common
// watch-page URL shapes into their embeddable form; anything else is
// assumed to be a direct file link and rendered with a plain <video> tag.
function toEmbedSrc(url: string): { kind: "iframe" | "video"; src: string } | null {
  try {
    const parsed = new URL(url);
    const host = parsed.hostname.replace(/^www\./, "");

    if (host === "youtube.com" || host === "m.youtube.com") {
      const id = parsed.searchParams.get("v");
      if (parsed.pathname.startsWith("/embed/")) return { kind: "iframe", src: url };
      if (id) return { kind: "iframe", src: `https://www.youtube.com/embed/${id}` };
      return null;
    }
    if (host === "youtu.be") {
      const id = parsed.pathname.slice(1);
      return id ? { kind: "iframe", src: `https://www.youtube.com/embed/${id}` } : null;
    }
    if (host === "vimeo.com") {
      const id = parsed.pathname.split("/").filter(Boolean)[0];
      return id ? { kind: "iframe", src: `https://player.vimeo.com/video/${id}` } : null;
    }
    if (host === "player.vimeo.com") {
      return { kind: "iframe", src: url };
    }
    if (/\.(mp4|webm|ogg)$/i.test(parsed.pathname)) {
      return { kind: "video", src: url };
    }
    return null;
  } catch {
    return null;
  }
}

export default function VideoEmbed({ url }: { url: string }) {
  const embed = toEmbedSrc(url);
  if (!embed) return null;

  return (
    <div className="aspect-video w-full overflow-hidden rounded-[18px] border border-line">
      {embed.kind === "iframe" ? (
        <iframe
          src={embed.src}
          className="h-full w-full"
          allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture"
          allowFullScreen
        />
      ) : (
        <video src={embed.src} controls className="h-full w-full" />
      )}
    </div>
  );
}
