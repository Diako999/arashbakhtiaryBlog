import { useState, type FormEvent } from "react";
import { useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { authApi } from "../api/auth";
import { ApiError } from "../api/client";
import { useSeo } from "../hooks/useSeo";

export default function OtpVerify() {
  const { t } = useTranslation();
  useSeo({ title: t("auth.otpVerify.title") });
  const navigate = useNavigate();
  const [code, setCode] = useState("");
  const [error, setError] = useState(false);
  const [usedRecovery, setUsedRecovery] = useState(false);

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    setError(false);
    try {
      const result = await authApi.otpVerify(code);
      if (result.usedRecoveryCode) {
        setUsedRecovery(true);
        setTimeout(() => navigate("/dashboard"), 1500);
      } else {
        navigate("/dashboard");
      }
    } catch (err) {
      if (err instanceof ApiError) setError(true);
      else throw err;
    }
  }

  return (
    <div dir="rtl" className="mx-auto mt-16 max-w-sm rounded-xl border border-line bg-card p-6">
      <h1 className="mb-4 text-xl font-bold">{t("auth.otpVerify.title")}</h1>
      <form onSubmit={(e) => void handleSubmit(e)} className="flex flex-col gap-3">
        <input
          required
          placeholder={t("auth.otpVerify.codeLabel")}
          value={code}
          onChange={(e) => setCode(e.target.value)}
          className="rounded-lg border border-line px-3 py-2"
        />
        <button type="submit" className="rounded-lg bg-brand px-4 py-2 font-bold text-white">
          {t("auth.otpVerify.submit")}
        </button>
        {error && <p className="text-danger">{t("auth.otpVerify.error")}</p>}
        {usedRecovery && <p className="text-accent">{t("auth.otpVerify.recoveryUsed")}</p>}
      </form>
    </div>
  );
}
