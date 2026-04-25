"use client";

import { useCallback, useEffect, useMemo, useState } from "react";
import { useRouter, useSearchParams } from "next/navigation";
import { ProtectedRoute } from "@/components/ProtectedRoute";
import { useUser } from "@/contexts/UserContext";
import { ApiError } from "@/lib/api";
import {
  RequestMetaDataItem,
  SearchRequestMetaDataResponse,
  requestMetaDataApi,
} from "@/lib/requestMetaData";

type RequestTab = {
  id: "role" | "mine" | "addressed" | "all";
  label: string;
};

const tabs: RequestTab[] = [
  { id: "role", label: "RoleRequest" },
  { id: "mine", label: "My Request" },
  { id: "addressed", label: "Addressed To Me" },
  { id: "all", label: "AllRequest" },
] as const;

type CountableTabId = RequestTab["id"];
type TabCounts = Partial<Record<CountableTabId, number>>;

const PAGE_SIZE = 10;
const REQUEST_DEDUPE_MS = 1500;
const requestMetaDataInFlight = new Map<
  string,
  { startedAt: number; promise: Promise<SearchRequestMetaDataResponse> }
>();

export default function RequestsPage() {
  return (
    <ProtectedRoute>
      <RequestsContent />
    </ProtectedRoute>
  );
}

function RequestsContent() {
  const { user } = useUser();
  const router = useRouter();
  const searchParams = useSearchParams();
  const requestedTab = searchParams.get("tab") as RequestTab["id"] | null;
  const initialTab = tabs.some((tab) => tab.id === requestedTab)
    ? requestedTab!
    : "role";
  const [activeTab, setActiveTab] = useState<RequestTab["id"]>(initialTab);
  const [onlyUnseen, setOnlyUnseen] = useState(false);
  const [statusFilter, setStatusFilter] = useState("All");
  const [metaItems, setMetaItems] = useState<RequestMetaDataItem[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [page, setPage] = useState(1);
  const [metaLoading, setMetaLoading] = useState(false);
  const [loadError, setLoadError] = useState<string | null>(null);
  const [tabCounts, setTabCounts] = useState<TabCounts>({});

  const loadTabCounts = useCallback(async () => {
    const userIdentifier = user?.email ?? user?.id ?? undefined;
    const tabQueries: { id: CountableTabId; query: Parameters<typeof requestMetaDataApi.search>[0] }[] = [
      {
        id: "role",
        query: {
          page: 1,
          pageSize: 1,
          requestType: "BoardProposalRequest",
          assignedToMyRole: true,
        },
      },
      {
        id: "mine",
        query: {
          page: 1,
          pageSize: 1,
          requestType: "BoardProposalRequest",
          createdBy: userIdentifier,
        },
      },
      {
        id: "addressed",
        query: {
          page: 1,
          pageSize: 1,
          requestType: "BoardProposalRequest",
          assignedToMe: true,
        },
      },
      {
        id: "all",
        query: {
          page: 1,
          pageSize: 1,
          requestType: "BoardProposalRequest",
        },
      },
    ];

    const results = await Promise.all(
      tabQueries.map(async ({ id, query }) => {
        try {
          const response = await requestMetaDataApi.search(query);
          return [id, response.totalCount] as const;
        } catch {
          return [id, undefined] as const;
        }
      }),
    );

    const next: TabCounts = {};
    for (const [id, count] of results) {
      if (count !== undefined) next[id] = count;
    }
    setTabCounts(next);
  }, [user?.email, user?.id]);
  const loadMetaData = useCallback(async () => {
    const serverStatus = statusFilter === "All" ? undefined : statusFilter;
    const createdBy =
      activeTab === "mine" ? (user?.email ?? user?.id ?? undefined) : undefined;
    const assignedToMe = activeTab === "addressed";
    const assignedToMyRole = activeTab === "role";
    const requestKey = JSON.stringify({
      page,
      pageSize: PAGE_SIZE,
      requestType: "BoardProposalRequest",
      status: serverStatus,
      onlyUnseen,
      createdBy,
      assignedToMe,
      assignedToMyRole,
    });
    const now = Date.now();
    const existing = requestMetaDataInFlight.get(requestKey);
    const promise =
      existing && now - existing.startedAt < REQUEST_DEDUPE_MS
        ? existing.promise
        : requestMetaDataApi.search({
            page,
            pageSize: PAGE_SIZE,
            requestType: "BoardProposalRequest",
            status: serverStatus,
            onlyUnseen,
            createdBy,
            assignedToMe,
            assignedToMyRole,
          });

    if (!existing || now - existing.startedAt >= REQUEST_DEDUPE_MS) {
      requestMetaDataInFlight.set(requestKey, {
        startedAt: now,
        promise,
      });
    }

    setMetaLoading(true);
    setLoadError(null);
    try {
      const response = await promise;
      setMetaItems(response.items);
      setTotalCount(response.totalCount);
    } catch (caught) {
      setLoadError(
        caught instanceof ApiError
          ? caught.message
          : "Could not load requests from core service",
      );
      setMetaItems([]);
      setTotalCount(0);
    } finally {
      setMetaLoading(false);
      window.setTimeout(() => {
        if (requestMetaDataInFlight.get(requestKey)?.promise === promise) {
          requestMetaDataInFlight.delete(requestKey);
        }
      }, REQUEST_DEDUPE_MS);
    }
  }, [activeTab, onlyUnseen, page, statusFilter, user?.email, user?.id]);

  useEffect(() => {
    void loadMetaData();
  }, [loadMetaData]);

  useEffect(() => {
    void loadTabCounts();
  }, [loadTabCounts]);

  useEffect(() => {
    // Reset to first page whenever filters change.
    setPage(1);
  }, [activeTab, onlyUnseen, statusFilter]);

  const filteredMetaItems = useMemo(
    () => metaItems,
    [metaItems],
  );

  const totalPages = Math.max(1, Math.ceil(totalCount / PAGE_SIZE));

  async function openRequest(item: RequestMetaDataItem) {
    try {
      if (!item.seen) {
        await requestMetaDataApi.markSeen(item.requestType, item.id);
      }
      router.push(`/requests/board-proposals/${item.id}`);
    } catch (caught) {
      setLoadError(
        caught instanceof ApiError
          ? caught.message
          : "Could not open request",
      );
    }
  }

  return (
    <div className="space-y-6">
      <header className="flex flex-wrap items-end justify-between gap-4">
        <div>
          <p className="text-xs font-semibold uppercase tracking-widest text-white/45">
            Digi requests
          </p>
          <h1 className="display mt-2 text-3xl font-bold">Request workbench</h1>
          <p className="mt-1 max-w-2xl text-sm text-white/55">
            Requests are projected from the core service request-metadata table via Kafka.
          </p>
        </div>
      </header>

      <section className="grid gap-3 sm:grid-cols-2 lg:grid-cols-4">
        {tabs.map((tab) => {
          const count = tabCounts[tab.id as CountableTabId];
          return (
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
              {count !== undefined && count > 0 && (
                <span className="absolute -right-2 -top-2 rounded-full bg-rose-400 px-2 py-0.5 text-[11px] font-bold text-white">
                  {count}
                </span>
              )}
              <span className="display block text-sm font-semibold">
                {tab.label}
              </span>
            </button>
          );
        })}
      </section>

      <section className="gls rounded-2xl p-5">
        {loadError && (
          <div className="mb-4 rounded-2xl border border-rose-300/30 bg-rose-400/10 p-3 text-sm text-rose-100">
            {loadError}
          </div>
        )}

        <div className="grid gap-4 lg:grid-cols-[1fr_auto]">
          <div className="grid gap-4 md:grid-cols-3">
            <div>
              <label className="label">Visible columns</label>
              <select className="field" defaultValue="4">
                <option value="4">4 selected</option>
              </select>
            </div>
            <div>
              <label className="label">Request type</label>
              <select className="field" value="Board proposal request" disabled>
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
                <option>Draft</option>
                <option>AgendaPreparation</option>
                <option>SecretaryReview</option>
                <option>ChairpersonReview</option>
                <option>ReadyForSending</option>
                <option>Sent</option>
                <option>Held</option>
                <option>DecisionsAndTasks</option>
                <option>DeadlineMonitoring</option>
                <option>Closed</option>
                <option>Cancelled</option>
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

        <MetaTable
          items={filteredMetaItems}
          loading={metaLoading}
          page={page}
          totalPages={totalPages}
          totalCount={totalCount}
          onPrev={() => setPage((p) => Math.max(1, p - 1))}
          onNext={() => setPage((p) => Math.min(totalPages, p + 1))}
          onOpen={openRequest}
        />
      </section>
    </div>
  );
}

function MetaTable({
  items,
  loading,
  page,
  totalPages,
  totalCount,
  onPrev,
  onNext,
  onOpen,
}: {
  items: RequestMetaDataItem[];
  loading: boolean;
  page: number;
  totalPages: number;
  totalCount: number;
  onPrev: () => void;
  onNext: () => void;
  onOpen: (item: RequestMetaDataItem) => void;
}) {
  return (
    <>
      <div className="mt-5 overflow-x-auto rounded-2xl border border-white/10">
        <table className="w-full border-collapse text-left text-sm">
          <thead className="bg-black/45 text-xs uppercase tracking-wider text-white/55">
            <tr>
              <th className="px-4 py-3">Request ID</th>
              <th className="px-4 py-3">Request type</th>
              <th className="px-4 py-3">Status</th>
              <th className="px-4 py-3">Created by</th>
              <th className="px-4 py-3">Modified by</th>
              <th className="px-4 py-3">Created at</th>
              <th className="px-4 py-3">Updated at</th>
            </tr>
          </thead>
          <tbody>
            {items.map((item) => (
              <tr
                key={`${item.requestType}-${item.id}`}
                onClick={() => onOpen(item)}
                className={`cursor-pointer border-t border-white/10 transition hover:bg-white/[0.1] ${
                  item.seen ? "bg-white/[0.03]" : "bg-orange-400/10"
                }`}
              >
                <td className="px-4 py-3 font-mono text-xs text-white/75">
                  <span className="inline-flex items-center gap-2">
                    {!item.seen && (
                      <span className="h-2 w-2 rounded-full bg-orange-500" />
                    )}
                    {item.vId ?? `#${item.id}`}
                  </span>
                </td>
                <td className="px-4 py-3 font-semibold">
                  {item.requestType}
                </td>
                <td className="px-4 py-3">
                  <StatusBadge status={item.status} />
                </td>
                <td className="px-4 py-3 text-white/75">{item.createdBy}</td>
                <td className="px-4 py-3 text-white/75">{item.modifiedBy}</td>
                <td className="px-4 py-3 text-xs text-white/55">
                  {new Date(item.createdAt).toLocaleString()}
                </td>
                <td className="px-4 py-3 text-xs text-white/55">
                  {new Date(item.updatedAt).toLocaleString()}
                </td>
              </tr>
            ))}
            {items.length === 0 && (
              <tr>
                <td className="px-4 py-8 text-center text-white/50" colSpan={7}>
                  {loading ? "Loading..." : "No requests match these filters."}
                </td>
              </tr>
            )}
          </tbody>
        </table>
      </div>
      <div className="mt-4 flex items-center justify-between text-xs text-white/60">
        <span>
          {totalCount === 0
            ? "0 results"
            : `Page ${page} of ${totalPages} - ${totalCount} total`}
        </span>
        <div className="flex gap-2">
          <button
            type="button"
            onClick={onPrev}
            disabled={page <= 1 || loading}
            className="btn-ghost text-xs disabled:opacity-40"
          >
            Prev
          </button>
          <button
            type="button"
            onClick={onNext}
            disabled={page >= totalPages || loading}
            className="btn-ghost text-xs disabled:opacity-40"
          >
            Next
          </button>
        </div>
      </div>
    </>
  );
}

function StatusBadge({ status }: { status: string }) {
  const palette =
    status === "Closed"
      ? "border-emerald-300/30 bg-emerald-400/10 text-emerald-100"
      : status === "Cancelled"
        ? "border-rose-300/30 bg-rose-400/10 text-rose-100"
        : "border-sky-300/30 bg-sky-400/10 text-sky-100";

  return <span className={`chip ${palette}`}>{status}</span>;
}
