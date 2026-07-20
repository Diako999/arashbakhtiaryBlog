import i18n from "i18next";
import { initReactI18next } from "react-i18next";
import fa from "./fa.json";
import ckb from "./ckb.json";

export const supportedLanguages = ["fa", "ckb"] as const;
export type SupportedLanguage = (typeof supportedLanguages)[number];
export const defaultLanguage: SupportedLanguage = "fa";

// Site is RTL-only in both supported languages — no LTR fallback needed,
// unlike a typical i18n setup that toggles direction per-language.
void i18n.use(initReactI18next).init({
  resources: {
    fa: { translation: fa },
    ckb: { translation: ckb },
  },
  lng: defaultLanguage,
  fallbackLng: defaultLanguage,
  interpolation: { escapeValue: false },
});

export default i18n;
