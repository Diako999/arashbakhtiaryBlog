import { useTranslation } from "react-i18next";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { dashboardApi } from "../../api/dashboard";

export default function SubmissionInbox() {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const { data, isLoading } = useQuery({ queryKey: ["dashboard-submissions"], queryFn: dashboardApi.submissions });

  const toggleMutation = useMutation({
    mutationFn: (id: number) => dashboardApi.toggleSubmissionContacted(id),
    onSuccess: () => void queryClient.invalidateQueries({ queryKey: ["dashboard-submissions"] }),
  });

  return (
    <div>
      <div className="mb-6 flex items-center justify-between">
        <h1 className="text-2xl font-bold">{t("dashboard.leads.inbox")}</h1>
        {/* Same-origin GET with cookies attached — the browser's normal navigation carries the auth cookie. */}
        <a href="/api/dashboard/leads/submissions/export" className="rounded-lg border border-line px-4 py-2 font-bold no-underline">
          {t("dashboard.leads.exportCsv")}
        </a>
      </div>

      {isLoading && <p>{t("common.loading")}</p>}

      <div className="flex flex-col gap-2">
        {data?.map((submission) => (
          <div key={submission.id} className="flex items-center justify-between rounded-lg border border-line bg-card p-3">
            <div>
              <p className="font-bold">
                {submission.name} <span className="text-ink-faint">— {submission.email}</span>
              </p>
              <p className="text-xs text-ink-faint">{submission.leadMagnetTitle}</p>
            </div>
            <button
              type="button"
              onClick={() => toggleMutation.mutate(submission.id)}
              className={`rounded-full px-3 py-1 text-xs ${
                submission.isContacted ? "bg-brand-soft text-brand" : "bg-surface-soft text-ink-muted"
              }`}
            >
              {submission.isContacted ? t("dashboard.leads.contacted") : t("dashboard.leads.notContacted")}
            </button>
          </div>
        ))}
        {data?.length === 0 && <p className="text-ink-muted">{t("dashboard.leads.noSubmissions")}</p>}
      </div>
    </div>
  );
}
