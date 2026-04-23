"use client";

import Link from "next/link";
import { useMemo, useState } from "react";
import { useSearchParams } from "next/navigation";
import { ProtectedRoute } from "@/components/ProtectedRoute";
import { useUser } from "@/contexts/UserContext";

type RequestRow = {
  number: string;
  type: string;
  modified: string;
  status: string;
  unseen?: boolean;
};

const rows: RequestRow[] = [
  {
    number: "MB000124",
    type: "Board proposal request",
    modified: "24.04.2026, 10:15",
    status: "SecretaryReview",
    unseen: true,
  },
  {
    number: "MB000123",
    type: "Board proposal request",
    modified: "24.04.2026, 09:40",
    status: "ReturnedForCorrection",
  },
];

type RequestTab = {
  id: "role" | "mine" | "addressed" | "all" | "advanced";
  label: string;
  count?: number;
};

const tabs: RequestTab[] = [
  { id: "role", label: "RoleRequest", count: 6839 },
  { id: "mine", label: "My Request", count: 192 },
  { id: "addressed", label: "Addressed To Me", count: 7 },
  { id: "all", label: "AllRequest", count: 358093 },
  { id: "advanced", label: "Advanced search" },
] as const;

export default function RequestsPage() {
  return (
    <ProtectedRoute>
      <RequestsContent />
    </ProtectedRoute>
  );
}

function RequestsContent() {
  const { user } = useUser();
  const searchParams = useSearchParams();
  const requestedTab = searchParams.get("tab") as RequestTab["id"] | null;
  const initialTab = tabs.some((tab) => tab.id === requestedTab)
    ? requestedTab!
    : "role";
  const [activeTab, setActiveTab] = useState<RequestTab["id"]>(initialTab);
  const [onlyUnseen, setOnlyUnseen] = useState(false);
  const [typeFilter, setTypeFilter] = useState(
    searchParams.get("type") === "board" ? "Board proposal request" : "All",
  );
  const [statusFilter, setStatusFilter] = useState("All");
  const accountName = user?.email?.split("@")[0] ?? "My account";

  const filteredRows = useMemo(
    () =>
      rows.filter((row) => {
        if (onlyUnseen && !row.unseen) return false;
        if (typeFilter !== "All" && row.type !== typeFilter) return false;
        if (statusFilter !== "All" && row.status !== statusFilter) return false;
        return true;
      }),
    [onlyUnseen, typeFilter, statusFilter],
  );

  return (
    <div className="space-y-6">
      <header className="flex flex-wrap items-end justify-between gap-4">
        <div>
          <p className="text-xs font-semibold uppercase tracking-widest text-white/45">
            Digi requests
          </p>
          <h1 className="display mt-2 text-3xl font-bold">Request workbench</h1>
          <p className="mt-1 max-w-2xl text-sm text-white/55">
            Role queues, personal requests, assigned requests, filters, and
            board proposal intake in one place.
          </p>
        </div>
        <Link href="/requests/board-proposals/new" className="btn-primary">
          New board proposal
        </Link>
      </header>

      <section className="grid gap-3 lg:grid-cols-5">
        {tabs.map((tab) => (
          <button
            key={tab.id}
            type="button"
            onClick={() => setActiveTab(tab.id)}
            className={`relative rounded-2xl border px-4 py-5 text-center transition ${
              activeTab === tab.id
                ? "border-fuchsia-200/45 bg-fuchsia-300/20 text-white"
                : "border-white/10 bg-white/[0.06] text-white/70 hover:bg-white/[0.12]"
            }`}
          >
            {tab.count !== undefined && (
              <span className="absolute -right-2 -top-2 rounded-full bg-rose-400 px-2 py-0.5 text-[11px] font-bold text-white">
                {tab.count}
              </span>
            )}
            <span className="display block text-sm font-semibold">
              {tab.label}
            </span>
          </button>
        ))}
      </section>

      <section className="gls rounded-2xl p-5">
        <div className="grid gap-4 lg:grid-cols-[1fr_auto]">
          <div className="grid gap-4 md:grid-cols-3">
            <div>
              <label className="label">Visible columns</label>
              <select className="field" defaultValue="4">
                <option value="4">4 selected</option>
                <option value="5">5 selected</option>
              </select>
            </div>
            <div>
              <label className="label">Request type</label>
              <select
                className="field"
                value={typeFilter}
                onChange={(event) => setTypeFilter(event.target.value)}
              >
                <option>All</option>
                <option>Board proposal request</option>
              </select>
            </div>
            <div>
              <label className="label">Request status</label>
              <select
                className="field"
                value={statusFilter}
                onChange={(event) => setStatusFilter(event.target.value)}
              >
                <option>All</option>
                <option>SecretaryReview</option>
                <option>ReturnedForCorrection</option>
              </select>
            </div>
          </div>

          <label className="mt-7 flex items-center gap-3 text-sm font-semibold text-white/75 lg:justify-end">
            <input
              type="checkbox"
              checked={onlyUnseen}
              onChange={(event) => setOnlyUnseen(event.target.checked)}
              className="h-5 w-5 rounded border-white/20 bg-white/10"
            />
            Show only unseen
          </label>
        </div>

        <div className="mt-5 overflow-hidden rounded-2xl border border-white/10">
          <table className="w-full border-collapse text-left text-sm">
            <thead className="bg-black/45 text-xs uppercase tracking-wider text-white/55">
              <tr>
                <th className="px-4 py-3">Number</th>
                <th className="px-4 py-3">Request type</th>
                <th className="px-4 py-3">Modified date</th>
                <th className="px-4 py-3">Request status</th>
                <th className="px-4 py-3">Owner</th>
              </tr>
            </thead>
            <tbody>
              {filteredRows.map((row) => (
                <tr
                  key={row.number}
                  className="border-t border-white/10 bg-white/[0.03] transition hover:bg-white/[0.1]"
                >
                  <td className="px-4 py-3 font-mono text-xs text-white/85">
                    <span className="mr-2 inline-block h-1.5 w-1.5 rounded-full bg-orange-300 opacity-0 data-[on=true]:opacity-100" data-on={row.unseen ? "true" : "false"} />
                    {row.number}
                  </td>
                  <td className="px-4 py-3 font-semibold">{row.type}</td>
                  <td className="px-4 py-3 text-white/70">{row.modified}</td>
                  <td className="px-4 py-3">
                    <StatusBadge status={row.status} />
                  </td>
                  <td className="px-4 py-3 text-white/70">{accountName}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>

        <div className="mt-4 flex flex-wrap items-center justify-between gap-4 text-sm text-white/55">
          <span>Showing 1 - {filteredRows.length} of 6839 results</span>
          <div className="flex items-center gap-2">
            {[1, 2, 3, 4, 5, 6].map((page) => (
              <button
                key={page}
                type="button"
                className={`grid h-9 w-9 place-items-center rounded-xl border ${
                  page === 1
                    ? "border-sky-200/40 bg-sky-300/25 text-white"
                    : "border-white/10 bg-white/[0.05] text-white/60"
                }`}
              >
                {page}
              </button>
            ))}
          </div>
        </div>
      </section>
    </div>
  );
}

function StatusBadge({ status }: { status: string }) {
  const palette =
    status === "SecretaryReview"
      ? "border-amber-300/30 bg-amber-400/10 text-amber-100"
      : status === "ReturnedForCorrection"
        ? "border-orange-300/30 bg-orange-400/10 text-orange-100"
        : status === "Expired"
          ? "border-rose-300/30 bg-rose-400/10 text-rose-100"
          : "border-sky-300/30 bg-sky-400/10 text-sky-100";

  return <span className={`chip ${palette}`}>{status}</span>;
}
