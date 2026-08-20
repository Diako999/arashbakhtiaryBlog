import { useState } from "react";
import { useTranslation } from "react-i18next";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { ChevronDown, ChevronUp, Trash2 } from "lucide-react";
import { dashboardApi } from "../../api/dashboard";
import FileUploadField from "../../components/FileUploadField";

// Standalone add/reorder/delete list, unlike the rest of SettingsPage's
// single-form-per-section layout — each slide upload/delete/move hits the
// API immediately (no draft state to lose track of, no "unsaved changes"
// ambiguity when an image is already sitting on the server).
export default function HeroSlidesManager() {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const { data: slides } = useQuery({ queryKey: ["dashboard-hero-slides"], queryFn: dashboardApi.heroSlides });
  const [newImageUrl, setNewImageUrl] = useState("");
  const [newLinkUrl, setNewLinkUrl] = useState("");

  function invalidate() {
    void queryClient.invalidateQueries({ queryKey: ["dashboard-hero-slides"] });
    void queryClient.invalidateQueries({ queryKey: ["hero-slides"] });
  }

  async function handleAdd() {
    if (!newImageUrl) return;
    await dashboardApi.createHeroSlide({ imageUrl: newImageUrl, linkUrl: newLinkUrl });
    setNewImageUrl("");
    setNewLinkUrl("");
    invalidate();
  }

  async function handleDelete(id: number) {
    await dashboardApi.deleteHeroSlide(id);
    invalidate();
  }

  async function handleMove(id: number, direction: "up" | "down") {
    await dashboardApi.moveHeroSlide(id, direction);
    invalidate();
  }

  const inputClass = "w-full rounded-lg border border-line bg-card px-3 py-2";
  const labelClass = "mb-1 block text-sm text-ink-muted";

  return (
    <div>
      <h1 className="mb-1 text-2xl font-bold">{t("dashboard.settings.heroSlidesTitle")}</h1>
      <p className="mb-4 text-sm text-ink-muted">{t("dashboard.settings.heroSlidesHint")}</p>

      <div className="mb-6 flex flex-col gap-3">
        {slides?.length === 0 && <p className="text-sm text-ink-faint">{t("dashboard.settings.noSlides")}</p>}
        {slides?.map((slide, index) => (
          <div key={slide.id} className="flex items-center gap-3 rounded-lg border border-line bg-card p-3">
            <img src={slide.imageUrl} alt="" className="h-16 w-24 rounded object-cover" />
            <div className="min-w-0 flex-1">
              <p dir="ltr" className="truncate text-sm text-ink">{slide.imageUrl}</p>
              {slide.linkUrl && <p dir="ltr" className="truncate text-xs text-ink-muted">{slide.linkUrl}</p>}
            </div>
            <div className="flex items-center gap-1">
              <button
                type="button"
                onClick={() => void handleMove(slide.id, "up")}
                disabled={index === 0}
                aria-label={t("dashboard.settings.moveUp")}
                className="rounded-lg p-2 text-ink-muted hover:bg-surface-soft hover:text-ink disabled:opacity-30"
              >
                <ChevronUp size={18} />
              </button>
              <button
                type="button"
                onClick={() => void handleMove(slide.id, "down")}
                disabled={!slides || index === slides.length - 1}
                aria-label={t("dashboard.settings.moveDown")}
                className="rounded-lg p-2 text-ink-muted hover:bg-surface-soft hover:text-ink disabled:opacity-30"
              >
                <ChevronDown size={18} />
              </button>
              <button
                type="button"
                onClick={() => void handleDelete(slide.id)}
                aria-label={t("dashboard.content.delete")}
                className="rounded-lg p-2 text-danger hover:bg-surface-soft"
              >
                <Trash2 size={18} />
              </button>
            </div>
          </div>
        ))}
      </div>

      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
        <div className="sm:col-span-2">
          <FileUploadField kind="image" label={t("dashboard.settings.heroSlideImage")} value={newImageUrl} onChange={setNewImageUrl} />
        </div>
        <div>
          <label className={labelClass}>{t("dashboard.settings.heroSlideLink")}</label>
          <input dir="ltr" value={newLinkUrl} onChange={(e) => setNewLinkUrl(e.target.value)} className={inputClass} />
        </div>
        <div className="sm:col-span-2">
          <button
            type="button"
            onClick={() => void handleAdd()}
            disabled={!newImageUrl}
            className="rounded-lg bg-brand px-5 py-2 font-bold text-white disabled:opacity-50"
          >
            {t("dashboard.settings.addSlide")}
          </button>
        </div>
      </div>
    </div>
  );
}
