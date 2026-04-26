"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { useEffect, useMemo, useRef, useState } from "react";
import { ProtectedRoute } from "@/components/ProtectedRoute";
import { useUser } from "@/contexts/UserContext";
import {
  requestMetaDataApi,
  type RequestMetaDataItem,
} from "@/lib/requestMetaData";

const mostUsedRequests = [
  { title: "Board proposal request", code: "MB", href: "/requests?tab=all" },
];

type QuickAction = {
  label: string;
  href?: string;
};

const quickActions: QuickAction[] = [
  { label: "Create proposal", href: "/requests/board-proposals/new" },
];

export default function DashboardPage() {
  return (
    <ProtectedRoute>
      <DashboardContent />
    </ProtectedRoute>
  );
}

function DashboardContent() {
  const { user } = useUser();
  const [queueCounts, setQueueCounts] = useState({
    role: 0,
    mine: 0,
    all: 0,
  });

  useEffect(() => {
    if (!user) return;

    let cancelled = false;

    async function loadCounts() {
      try {
        const [role, mine, all] = await Promise.all([
          requestMetaDataApi.search({
            page: 1,
            pageSize: 1,
            requestType: "BoardProposalRequest",
            assignedToMyRole: true,
          }),
          requestMetaDataApi.search({
            page: 1,
            pageSize: 1,
            requestType: "BoardProposalRequest",
            createdBy: user?.email ?? user?.id ?? undefined,
          }),
          requestMetaDataApi.search({
            page: 1,
            pageSize: 1,
            requestType: "BoardProposalRequest",
          }),
        ]);

        if (!cancelled) {
          setQueueCounts({
            role: role.totalCount,
            mine: mine.totalCount,
            all: all.totalCount,
          });
        }
      } catch {
        if (!cancelled) {
          setQueueCounts({ role: 0, mine: 0, all: 0 });
        }
      }
    }

    void loadCounts();

    return () => {
      cancelled = true;
    };
  }, [user]);

  if (!user) return null;

  const requestQueues = [
    { label: "RoleRequest", count: queueCounts.role, href: "/requests?tab=role" },
    { label: "My Request", count: queueCounts.mine, href: "/requests?tab=mine" },
    { label: "AllRequest", count: queueCounts.all, href: "/requests?tab=all" },
  ];

  return (
    <div className="space-y-8">
      <section className="grid gap-4 lg:grid-cols-[minmax(0,1fr)_360px]">
        <div className="space-y-5">
          <RequestSearchBox />


          <section>
            <div className="mb-3 flex items-center justify-between gap-3">
              <h2 className="display text-lg font-semibold">
                Most used requests
              </h2>
              <Link
                href="/requests?tab=all"
                className="text-sm font-semibold text-white/65 underline decoration-white/25 underline-offset-4 hover:text-white"
              >
                View all
              </Link>
            </div>
            <div className="grid gap-3 sm:grid-cols-2 xl:grid-cols-3">
              {mostUsedRequests.map((request) => (
                <Link
                  key={request.title}
                  href={request.href}
                  className="gl rounded-2xl p-4 transition hover:-translate-y-0.5 hover:bg-white/[0.14]"
                >
                  <span className="chip mb-4 border-white/15 text-white/70">
                    {request.code}
                  </span>
                  <span className="display block text-sm font-semibold">
                    {request.title}
                  </span>
                </Link>
              ))}
            </div>
          </section>

          <section>
            <h2 className="display mb-3 text-lg font-semibold">
              Quick actions
            </h2>
            <div className="grid gap-3 sm:grid-cols-2">
              {quickActions.map((action) =>
                action.href ? (
                  <Link
                    key={action.label}
                    href={action.href}
                    className="gl flex items-center justify-between rounded-2xl px-4 py-3 text-left text-sm font-semibold transition hover:bg-white/[0.14]"
                  >
                    {action.label}
                    <span className="text-white/35">&gt;</span>
                  </Link>
                ) : (
                  <button
                    key={action.label}
                    type="button"
                    className="gl flex items-center justify-between rounded-2xl px-4 py-3 text-left text-sm font-semibold transition hover:bg-white/[0.14]"
                  >
                    {action.label}
                    <span className="text-white/35">&gt;</span>
                  </button>
                ),
              )}
            </div>
          </section>
        </div>

        <aside className="space-y-5">
          <section className="gls rounded-2xl p-5">
            <h2 className="display text-lg font-semibold">Office targets</h2>
            <div className="mt-5 grid grid-cols-2 gap-4">
              <TargetDial label="Daily flow" value={65} />
              <TargetDial label="SLA" value={91} />
            </div>
            <p className="mt-4 text-right text-xs text-white/45">
              Current to: 24.04.2026
            </p>
          </section>

          <section className="gls rounded-2xl p-5">
            <h2 className="display text-lg font-semibold">Request queues</h2>
            <div className="mt-4 space-y-3">
              {requestQueues.map((queue) => (
                <Link
                  key={queue.label}
                  href={queue.href}
                  className="gl flex items-center justify-between rounded-2xl p-4 transition hover:bg-white/[0.14]"
                >
                  <span>
                    <span className="text-xs font-semibold uppercase tracking-wider text-white/45">
                      {queue.label}
                    </span>
                    <span className="mt-1 block text-sm text-white/50">
                      Open paginated list
                    </span>
                  </span>
                  <span className="display text-2xl font-bold">
                    {queue.count.toLocaleString()}
                  </span>
                </Link>
              ))}
            </div>
          </section>
        </aside>
      </section>

    </div>
  );
}

function requestHref(item: RequestMetaDataItem): string {
  if (item.requestType === "BoardProposalRequest") {
    return `/requests/board-proposals/${item.id}`;
  }
  return `/requests?type=${encodeURIComponent(item.requestType)}`;
}

function RequestSearchBox() {
  const router = useRouter();
  const [query, setQuery] = useState("");
  const [results, setResults] = useState<RequestMetaDataItem[]>([]);
  const [loading, setLoading] = useState(false);
  const [open, setOpen] = useState(false);
  const [activeIndex, setActiveIndex] = useState(-1);
  const containerRef = useRef<HTMLDivElement | null>(null);

  const trimmed = useMemo(() => query.trim(), [query]);

  useEffect(() => {
    if (trimmed.length === 0) {
      setResults([]);
      setLoading(false);
      return;
    }

    let cancelled = false;
    setLoading(true);

    const handle = setTimeout(async () => {
      try {
        const response = await requestMetaDataApi.search({
          page: 1,
          pageSize: 50,
        });
        if (cancelled) return;

        const lower = trimmed.toLowerCase();
        const filtered = response.items.filter((item) => {
          const idMatch = String(item.id).includes(trimmed);
          const vIdMatch = (item.vId ?? "").toLowerCase().includes(lower);
          const createdByMatch = (item.createdBy ?? "")
            .toLowerCase()
            .includes(lower);
          const typeMatch = item.requestType.toLowerCase().includes(lower);
          const statusMatch = (item.status ?? "")
            .toLowerCase()
            .includes(lower);
          return (
            idMatch || vIdMatch || createdByMatch || typeMatch || statusMatch
          );
        });

        setResults(filtered.slice(0, 8));
        setActiveIndex(filtered.length > 0 ? 0 : -1);
      } catch {
        if (!cancelled) {
          setResults([]);
          setActiveIndex(-1);
        }
      } finally {
        if (!cancelled) setLoading(false);
      }
    }, 250);

    return () => {
      cancelled = true;
      clearTimeout(handle);
    };
  }, [trimmed]);

  useEffect(() => {
    function onClickOutside(event: MouseEvent) {
      if (!containerRef.current) return;
      if (!containerRef.current.contains(event.target as Node)) {
        setOpen(false);
      }
    }
    document.addEventListener("mousedown", onClickOutside);
    return () => document.removeEventListener("mousedown", onClickOutside);
  }, []);

  const showDropdown = open && trimmed.length > 0;

  function selectItem(item: RequestMetaDataItem) {
    setOpen(false);
    setQuery("");
    setResults([]);
    router.push(requestHref(item));
  }

  function onKeyDown(event: React.KeyboardEvent<HTMLInputElement>) {
    if (!showDropdown || results.length === 0) {
      if (event.key === "Enter") {
        event.preventDefault();
        router.push(`/requests?q=${encodeURIComponent(trimmed)}`);
      }
      return;
    }

    if (event.key === "ArrowDown") {
      event.preventDefault();
      setActiveIndex((idx) => (idx + 1) % results.length);
    } else if (event.key === "ArrowUp") {
      event.preventDefault();
      setActiveIndex((idx) => (idx - 1 + results.length) % results.length);
    } else if (event.key === "Enter") {
      event.preventDefault();
      const target = results[activeIndex] ?? results[0];
      if (target) selectItem(target);
    } else if (event.key === "Escape") {
      setOpen(false);
    }
  }

  return (
    <div
      ref={containerRef}
      className={`gls relative rounded-2xl p-4 ${
        showDropdown ? "z-50" : ""
      }`}
    >
      <label className="label">Search</label>
      <div className="flex gap-2">
        <input
          className="field"
          placeholder="UCN, mobile number, customer code, request number"
          value={query}
          onChange={(event) => {
            setQuery(event.target.value);
            setOpen(true);
          }}
          onFocus={() => setOpen(true)}
          onKeyDown={onKeyDown}
          autoComplete="off"
        />
        <Link href="/requests" className="btn-primary px-4">
          Search
        </Link>
      </div>

      {showDropdown && (
        <div
          className="absolute left-4 right-4 top-full z-[60] mt-2 overflow-hidden rounded-2xl border border-white/15 shadow-2xl shadow-black/60"
          style={{ backgroundColor: "#171336" }}
        >
          {loading && results.length === 0 ? (
            <p className="px-4 py-3 text-sm text-white/55">Searching...</p>
          ) : results.length === 0 ? (
            <p className="px-4 py-3 text-sm text-white/55">
              No matching requests.
            </p>
          ) : (
            <ul className="max-h-80 overflow-y-auto py-1">
              {results.map((item, index) => {
                const isActive = index === activeIndex;
                return (
                  <li key={`${item.requestType}-${item.id}`}>
                    <button
                      type="button"
                      onMouseEnter={() => setActiveIndex(index)}
                      onClick={() => selectItem(item)}
                      className={`flex w-full flex-col items-start gap-0.5 px-4 py-2.5 text-left text-sm transition ${
                        isActive
                          ? "bg-white/[0.12] text-white"
                          : "text-white/80 hover:bg-white/[0.08]"
                      }`}
                    >
                      <span className="flex w-full items-center justify-between gap-3">
                        <span className="font-semibold">
                          <span className="font-mono text-white/55">
                            {item.vId ?? `#${item.id}`}
                          </span>{" "}
                          · {item.requestType}
                        </span>
                        <span className="chip border-white/15 bg-white/10 text-[10px]">
                          {item.status}
                        </span>
                      </span>
                      <span className="text-xs text-white/55">
                        {item.createdBy}
                      </span>
                    </button>
                  </li>
                );
              })}
            </ul>
          )}
        </div>
      )}
    </div>
  );
}

function TargetDial({ label, value }: { label: string; value: number }) {
  return (
    <div className="rounded-2xl border border-white/10 bg-white/[0.05] p-4">
      <div className="mx-auto grid h-24 w-24 place-items-center rounded-full border-[10px] border-white/15 border-r-fuchsia-300 border-t-orange-300">
        <span className="display text-lg font-bold">{value}%</span>
      </div>
      <p className="mt-3 text-center text-xs font-semibold text-white/55">
        {label}
      </p>
    </div>
  );
}
