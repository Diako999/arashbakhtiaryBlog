import { useLocation, useNavigate } from "react-router-dom";
import { supportedLanguages, type SupportedLanguage } from "../i18n";

const LABELS: Record<SupportedLanguage, string> = {
  fa: "فارسی",
  ckb: "کوردی",
};

export default function LanguageSwitcher({ activeLang }: { activeLang: SupportedLanguage }) {
  const location = useLocation();
  const navigate = useNavigate();

  function switchTo(lang: SupportedLanguage) {
    const rest = location.pathname.split("/").slice(2).join("/");
    navigate(`/${lang}/${rest}${location.search}`);
  }

  return (
    <div className="flex gap-2 text-sm">
      {supportedLanguages.map((lang) => (
        <button
          key={lang}
          type="button"
          onClick={() => switchTo(lang)}
          disabled={lang === activeLang}
          className={lang === activeLang ? "font-bold text-brand" : "text-ink-muted hover:text-brand"}
        >
          {LABELS[lang]}
        </button>
      ))}
    </div>
  );
}
