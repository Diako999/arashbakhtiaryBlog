import { useEffect, useRef, useState } from "react";
import { useLocation, useNavigate } from "react-router-dom";
import { ChevronDown } from "lucide-react";
import { supportedLanguages, type SupportedLanguage } from "../i18n";

const LABELS: Record<SupportedLanguage, string> = {
  fa: "فارسی",
  ckb: "کوردی",
};

export default function LanguageSwitcher({ activeLang }: { activeLang: SupportedLanguage }) {
  const location = useLocation();
  const navigate = useNavigate();
  const [open, setOpen] = useState(false);
  const rootRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    function onClickOutside(e: MouseEvent) {
      if (rootRef.current && !rootRef.current.contains(e.target as Node)) setOpen(false);
    }
    document.addEventListener("mousedown", onClickOutside);
    return () => document.removeEventListener("mousedown", onClickOutside);
  }, []);

  function switchTo(lang: SupportedLanguage) {
    const rest = location.pathname.split("/").slice(2).join("/");
    navigate(`/${lang}/${rest}${location.search}`);
    setOpen(false);
  }

  return (
    <div ref={rootRef} className="relative text-sm">
      <button
        type="button"
        onClick={() => setOpen((v) => !v)}
        className="flex h-11 items-center gap-1 rounded-lg px-3 text-ink hover:bg-surface-soft"
      >
        {LABELS[activeLang]}
        <ChevronDown size={14} className={open ? "rotate-180 transition-transform" : "transition-transform"} />
      </button>

      <div
        className={
          "absolute end-0 top-full z-30 mt-1 min-w-28 origin-top overflow-hidden rounded-lg border border-line bg-card/90 shadow-lg backdrop-blur-md transition-all duration-150 ease-out " +
          (open ? "visible scale-y-100 opacity-100" : "invisible scale-y-95 opacity-0")
        }
      >
        {supportedLanguages.map((lang) => (
          <button
            key={lang}
            type="button"
            onClick={() => switchTo(lang)}
            disabled={lang === activeLang}
            className={
              lang === activeLang
                ? "block w-full px-3 py-2 text-start font-bold text-brand"
                : "block w-full px-3 py-2 text-start text-ink hover:bg-surface-soft"
            }
          >
            {LABELS[lang]}
          </button>
        ))}
      </div>
    </div>
  );
}
