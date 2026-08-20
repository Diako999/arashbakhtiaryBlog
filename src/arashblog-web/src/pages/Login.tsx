import { useState, type FormEvent } from "react";
import { useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { authApi } from "../api/auth";
import { ApiError } from "../api/client";
import { useSeo } from "../hooks/useSeo";

export default function Login() {
  const { t } = useTranslation();
  useSeo({ title: t("auth.login.title") });
  const navigate = useNavigate();
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState(false);
  const [loading, setLoading] = useState(false);

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    setLoading(true);
    setError(false);
    try {
      const result = await authApi.login(username, password);
      if (result.requiresOtpVerify) navigate("/dashboard/otp/verify");
      else if (result.requiresOtpSetup) navigate("/dashboard/otp/setup");
      else navigate("/dashboard");
    } catch (err) {
      if (err instanceof ApiError) setError(true);
      else throw err;
    } finally {
      setLoading(false);
    }
  }

  return (
    <div dir="rtl" className="mx-auto mt-16 max-w-sm rounded-xl border border-line bg-card p-6">
      <h1 className="mb-4 text-xl font-bold">{t("auth.login.title")}</h1>
      <form onSubmit={(e) => void handleSubmit(e)} className="flex flex-col gap-3">
        <input
          required
          placeholder={t("auth.login.username")}
          value={username}
          onChange={(e) => setUsername(e.target.value)}
          className="rounded-lg border border-line px-3 py-2"
        />
        <input
          required
          type="password"
          placeholder={t("auth.login.password")}
          value={password}
          onChange={(e) => setPassword(e.target.value)}
          className="rounded-lg border border-line px-3 py-2"
        />
        <button disabled={loading} type="submit" className="rounded-lg bg-brand px-4 py-2 font-bold text-white">
          {t("auth.login.submit")}
        </button>
        {error && <p className="text-danger">{t("auth.login.error")}</p>}
      </form>
    </div>
  );
}
