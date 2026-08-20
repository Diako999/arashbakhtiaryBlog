import { useState, type FormEvent } from "react";
import { useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { useQuery } from "@tanstack/react-query";
import { authApi } from "../api/auth";
import { ApiError } from "../api/client";
import { useSeo } from "../hooks/useSeo";

export default function OtpSetup() {
  const { t } = useTranslation();
  useSeo({ title: t("auth.otpSetup.title") });
  const navigate = useNavigate();
  const [code, setCode] = useState("");
  const [error, setError] = useState(false);
  const [recoveryCodes, setRecoveryCodes] = useState<string[] | null>(null);

  const { data: setup, isLoading } = useQuery({
    queryKey: ["otp-setup"],
    queryFn: () => authApi.otpSetup(),
  });

  async function handleConfirm(e: FormEvent) {
    e.preventDefault();
    setError(false);
    try {
      const result = await authApi.otpSetupConfirm(code);
      setRecoveryCodes(result.recoveryCodes);
    } catch (err) {
      if (err instanceof ApiError) setError(true);
      else throw err;
    }
  }

  if (recoveryCodes) {
    return (
      <div dir="rtl" className="mx-auto mt-16 max-w-sm rounded-xl border border-line bg-card p-6">
        <h1 className="mb-2 text-xl font-bold">{t("auth.otpSetup.recoveryCodesTitle")}</h1>
        <p className="mb-4 text-sm text-danger">{t("auth.otpSetup.recoveryCodesWarning")}</p>
        <ul className="mb-4 grid grid-cols-2 gap-2 font-mono text-sm">
          {recoveryCodes.map((rc) => (
            <li key={rc} className="rounded bg-surface-soft px-2 py-1">
              {rc}
            </li>
          ))}
        </ul>
        <button
          type="button"
          onClick={() => navigate("/dashboard")}
          className="rounded-lg bg-brand px-4 py-2 font-bold text-white"
        >
          {t("auth.otpSetup.continue")}
        </button>
      </div>
    );
  }

  return (
    <div dir="rtl" className="mx-auto mt-16 max-w-sm rounded-xl border border-line bg-card p-6">
      <h1 className="mb-4 text-xl font-bold">{t("auth.otpSetup.title")}</h1>
      {isLoading && <p>{t("common.loading")}</p>}
      {setup && (
        <>
          <p className="mb-2">{t("auth.otpSetup.scanQr")}</p>
          <img src={setup.qrCodeDataUri} alt="QR" className="mb-4 h-40 w-40" />
          <p className="mb-1 text-sm text-ink-muted">{t("auth.otpSetup.manualKey")}</p>
          <p className="mb-4 font-mono">{setup.manualKey}</p>
          <form onSubmit={(e) => void handleConfirm(e)} className="flex flex-col gap-3">
            <input
              required
              inputMode="numeric"
              placeholder={t("auth.otpSetup.codeLabel")}
              value={code}
              onChange={(e) => setCode(e.target.value)}
              className="rounded-lg border border-line px-3 py-2"
            />
            <button type="submit" className="rounded-lg bg-brand px-4 py-2 font-bold text-white">
              {t("auth.otpSetup.confirm")}
            </button>
            {error && <p className="text-danger">{t("auth.otpSetup.error")}</p>}
          </form>
        </>
      )}
    </div>
  );
}
