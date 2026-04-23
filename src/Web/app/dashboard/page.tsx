"use client";

import Link from "next/link";
import { ProtectedRoute } from "@/components/ProtectedRoute";
import { useUser } from "@/contexts/UserContext";

const mostUsedRequests = [
  { title: "Board proposal request", code: "MB", href: "/requests?type=board" },
];

const quickActions = [
  "Make a payment",
  "Open a deposit",
  "Create account",
  "Register new customer",
  "Make cash deposit",
  "Exchange currency",
  "Request a debit card",
  "Conflict of interests",
];

const requestQueues = [
  { label: "RoleRequest", count: 6839, href: "/requests?tab=role" },
  { label: "My Request", count: 192, href: "/requests?tab=mine" },
  { label: "AllRequest", count: 358093, href: "/requests?tab=all" },
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

  if (!user) return null;

  return (
    <div className="space-y-8">
      <section className="grid gap-4 lg:grid-cols-[minmax(0,1fr)_360px]">
        <div className="space-y-5">
          <div className="gls rounded-2xl p-4">
            <label className="label">Search</label>
            <div className="flex gap-2">
              <input
                className="field"
                placeholder="UCN, mobile number, customer code, request number"
              />
              <Link href="/requests" className="btn-primary px-4">
                Search
              </Link>
            </div>
          </div>

          <section>
            <div className="mb-3 flex items-center justify-between gap-3">
              <h2 className="display text-lg font-semibold">
                Most used requests
              </h2>
              <Link
                href="/requests"
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
              {quickActions.map((action) => (
                <button
                  key={action}
                  type="button"
                  className="gl flex items-center justify-between rounded-2xl px-4 py-3 text-left text-sm font-semibold transition hover:bg-white/[0.14]"
                >
                  {action}
                  <span className="text-white/35">›</span>
                </button>
              ))}
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
