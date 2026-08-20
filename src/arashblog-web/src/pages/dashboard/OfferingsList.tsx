import { Link } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { dashboardApi } from "../../api/dashboard";
import { useSeo } from "../../hooks/useSeo";

export default function OfferingsList() {
  const { t } = useTranslation();
  useSeo({ title: t("dashboard.offerings.title") });
  const queryClient = useQueryClient();
  const { data, isLoading } = useQuery({ queryKey: ["dashboard-offerings"], queryFn: dashboardApi.offerings });

  const deleteMutation = useMutation({
    mutationFn: (id: number) => dashboardApi.deleteOffering(id),
    onSuccess: () => void queryClient.invalidateQueries({ queryKey: ["dashboard-offerings"] }),
  });

  return (
    <div>
      <div className="mb-6 flex items-center justify-between">
        <h1 className="text-2xl font-bold">{t("dashboard.offerings.title")}</h1>
        <Link to="/dashboard/offerings/new" className="rounded-lg bg-brand px-4 py-2 font-bold text-white no-underline">
          {t("dashboard.offerings.newOffering")}
        </Link>
      </div>

      {isLoading && <p>{t("common.loading")}</p>}

      <div className="flex flex-col gap-2">
        {data?.items.map((offering) => (
          <div key={offering.id} className="flex items-center justify-between rounded-lg border border-line bg-card p-3">
            <div>
              <p className="font-bold">{offering.titleFa}</p>
              <p className="text-xs text-ink-faint">
                {offering.status === "Published" ? t("dashboard.content.published") : t("dashboard.content.draft")}
                {offering.price !== null && ` · ${offering.price.toLocaleString()} ${t("offerings.currency")}`}
              </p>
            </div>
            <div className="flex gap-2">
              <Link to={`/dashboard/offerings/${offering.id}/edit`} className="text-sm text-brand no-underline hover:underline">
                {t("dashboard.content.edit")}
              </Link>
              <button
                type="button"
                onClick={() => {
                  if (confirm(t("dashboard.content.confirmDelete"))) deleteMutation.mutate(offering.id);
                }}
                className="text-sm text-danger hover:underline"
              >
                {t("dashboard.content.delete")}
              </button>
            </div>
          </div>
        ))}
        {data?.items.length === 0 && <p className="text-ink-muted">{t("offerings.none")}</p>}
      </div>
    </div>
  );
}
