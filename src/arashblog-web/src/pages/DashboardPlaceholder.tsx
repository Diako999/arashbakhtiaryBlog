import { useEffect, useState } from "react";
import { Navigate } from "react-router-dom";
import { api, ApiError } from "../api/client";

// M2 replaces this with the real Dashboard UI (Overview, Content CRUD,
// etc.). For M1 this only proves the login -> 2FA-setup -> 2FA-verify ->
// protected-access chain works end to end against /api/dashboard/ping.
export default function DashboardPlaceholder() {
  const [status, setStatus] = useState<"loading" | "ok" | "setup" | "login">("loading");

  useEffect(() => {
    api
      .get("/dashboard/ping")
      .then(() => setStatus("ok"))
      .catch((err: unknown) => {
        if (err instanceof ApiError && err.status === 403) setStatus("setup");
        else setStatus("login");
      });
  }, []);

  if (status === "loading") return <p className="p-8">...</p>;
  if (status === "setup") return <Navigate to="/dashboard/otp/setup" replace />;
  if (status === "login") return <Navigate to="/dashboard/login" replace />;
  return (
    <div dir="rtl" className="p-8">
      Dashboard placeholder — M2 builds the real UI here.
    </div>
  );
}
