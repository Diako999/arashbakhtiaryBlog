import { Link } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { dashboardApi } from "../../api/dashboard";

export default function LeadsList() {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const { data, isLoading } = useQuery({ queryKey: ["dashboard-leads"], queryFn: dashboardApi.leadMagnets });

  const deleteMutation = useMutation({
    mutationFn: (id: number) => dashboardApi.deleteLeadMagnet(id),
    onSuccess: () => void queryClient.invalidateQueries({ queryKey: ["dashboard-leads"] }),
  });

  return (
    <div>
      <div className="mb-6 flex items-center justify-between">
        <h1 className="text-2xl font-bold">{t("dashboard.leads.title")}</h1>
        <div className="flex gap-2">
          <Link to="/dashboard/leads/submissions" className="rounded-lg border border-line px-4 py-2 font-bold no-underline">
            {t("dashboard.leads.inbox")}
          </Link>
          <Link to="/dashboard/leads/new" className="rounded-lg bg-brand px-4 py-2 font-bold text-white no-underline">
            {t("dashboard.leads.newLead")}
          </Link>
        </div>
      </div>

      {isLoading && <p>{t("common.loading")}</p>}

      <div className="flex flex-col gap-2">
        {data?.map((lead) => (
          <div key={lead.id} className="flex items-center justify-between rounded-lg border border-line bg-card p-3">
            <div>
              <p className="font-bold">{lead.titleFa}</p>
              <p className="text-xs text-ink-faint">
                {lead.status === "Published" ? t("dashboard.content.published") : t("dashboard.content.draft")}
              </p>
            </div>
            <div className="flex gap-2">
              <Link to={`/dashboard/leads/${lead.id}/edit`} className="text-sm text-brand no-underline hover:underline">
                {t("dashboard.content.edit")}
              </Link>
              <button
                type="button"
                onClick={() => {
                  if (confirm(t("dashboard.content.confirmDelete"))) deleteMutation.mutate(lead.id);
                }}
                className="text-sm text-danger hover:underline"
              >
                {t("dashboard.content.delete")}
              </button>
            </div>
          </div>
        ))}
        {data?.length === 0 && <p className="text-ink-muted">{t("leads.none")}</p>}
      </div>
    </div>
  );
}
