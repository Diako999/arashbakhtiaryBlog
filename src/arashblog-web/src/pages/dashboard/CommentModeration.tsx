import { useTranslation } from "react-i18next";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { dashboardApi } from "../../api/dashboard";

export default function CommentModeration() {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const { data: comments, isLoading } = useQuery({ queryKey: ["dashboard-comments"], queryFn: dashboardApi.comments });

  const toggleMutation = useMutation({
    mutationFn: (id: number) => dashboardApi.toggleComment(id),
    onSuccess: () => void queryClient.invalidateQueries({ queryKey: ["dashboard-comments"] }),
  });

  const deleteMutation = useMutation({
    mutationFn: (id: number) => dashboardApi.deleteComment(id),
    onSuccess: () => void queryClient.invalidateQueries({ queryKey: ["dashboard-comments"] }),
  });

  if (isLoading) return <p>{t("common.loading")}</p>;

  return (
    <div>
      <h1 className="mb-6 text-2xl font-bold">{t("dashboard.comments.title")}</h1>
      <div className="flex flex-col gap-3">
        {comments?.map((c) => (
          <div key={c.id} className="rounded-lg border border-line bg-card p-4">
            <div className="mb-2 flex items-center justify-between">
              <div>
                <span className="font-bold">{c.name}</span>{" "}
                <span className="text-xs text-ink-faint">
                  · {t("dashboard.comments.on")} {c.postTitle}
                </span>
              </div>
              <span
                className={`rounded-full px-2 py-0.5 text-xs ${
                  c.isApproved ? "bg-brand-soft text-brand" : "bg-surface-soft text-ink-muted"
                }`}
              >
                {c.isApproved ? t("dashboard.comments.approved") : t("dashboard.comments.pending")}
              </span>
            </div>
            <p className="mb-3 text-ink-muted">{c.body}</p>
            <div className="flex gap-3 text-sm">
              <button type="button" onClick={() => toggleMutation.mutate(c.id)} className="text-brand hover:underline">
                {c.isApproved ? t("dashboard.comments.unapprove") : t("dashboard.comments.approve")}
              </button>
              <button
                type="button"
                onClick={() => {
                  if (confirm(t("dashboard.content.confirmDelete"))) deleteMutation.mutate(c.id);
                }}
                className="text-danger hover:underline"
              >
                {t("dashboard.content.delete")}
              </button>
            </div>
          </div>
        ))}
        {comments?.length === 0 && <p className="text-ink-muted">{t("dashboard.comments.none")}</p>}
      </div>
    </div>
  );
}
