import { useState, type ChangeEvent } from "react";
import { useTranslation } from "react-i18next";
import { uploadsApi } from "../api/uploads";

interface FileUploadFieldProps {
  kind: "image" | "document";
  label: string;
  value: string | null;
  onChange: (url: string) => void;
  required?: boolean;
}

// Keeps the plain URL text input from earlier milestones (still useful for
// pasting an already-hosted URL) and adds a real upload path alongside it
// — either way, the field just ends up holding a URL string, so no schema
// change was needed anywhere this is used.
export default function FileUploadField({ kind, label, value, onChange, required }: FileUploadFieldProps) {
  const { t } = useTranslation();
  const [uploading, setUploading] = useState(false);
  const [error, setError] = useState(false);

  async function handleFileChange(e: ChangeEvent<HTMLInputElement>) {
    const file = e.target.files?.[0];
    if (!file) return;

    setUploading(true);
    setError(false);
    try {
      const result = kind === "image" ? await uploadsApi.image(file) : await uploadsApi.document(file);
      onChange(result.url);
    } catch {
      setError(true);
    } finally {
      setUploading(false);
      e.target.value = "";
    }
  }

  return (
    <div>
      <label className="mb-1 block text-sm text-ink-muted">{label}</label>
      <div className="flex gap-2">
        <input
          required={required}
          value={value ?? ""}
          onChange={(e) => onChange(e.target.value)}
          placeholder={t("dashboard.upload.urlPlaceholder")}
          className="flex-1 rounded-lg border border-line bg-card px-3 py-2"
        />
        <label className="cursor-pointer whitespace-nowrap rounded-lg border border-line px-3 py-2 text-sm hover:bg-surface-soft">
          {uploading ? t("common.loading") : t("dashboard.upload.choose")}
          <input
            type="file"
            accept={kind === "image" ? "image/jpeg,image/png,image/webp" : "application/pdf"}
            onChange={(e) => void handleFileChange(e)}
            className="hidden"
          />
        </label>
      </div>
      {error && <p className="mt-1 text-sm text-danger">{t("dashboard.upload.error")}</p>}
    </div>
  );
}
