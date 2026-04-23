"use client";

import { ProtectedRoute } from "@/components/ProtectedRoute";
import { useUser } from "@/contexts/UserContext";

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
    <div className="space-y-6">
      <header className="flex flex-wrap items-end justify-between gap-4">
        <div>
          <p className="text-xs font-semibold uppercase tracking-widest text-white/45">
            Account
          </p>
          <h1 className="display mt-2 text-3xl font-bold">Dashboard</h1>
          <p className="mt-1 text-sm text-white/55">
            Decoded directly from your JWT.
          </p>
        </div>
        <span className="chip border-emerald-300/30 bg-emerald-400/10 text-emerald-200">
          <span className="h-1.5 w-1.5 rounded-full bg-emerald-300" />
          Authenticated
        </span>
      </header>

      <section className="gl rounded-2xl p-6">
        <h2 className="label">Identity</h2>
        <dl className="mt-3 grid gap-y-2 text-sm sm:grid-cols-[140px_1fr]">
          <dt className="text-white/45">User id</dt>
          <dd className="font-mono text-xs text-white/90 sm:text-sm">
            {user.id ?? "—"}
          </dd>
          <dt className="text-white/45">Email</dt>
          <dd className="text-white/90">{user.email ?? "—"}</dd>
        </dl>
      </section>

      <div className="grid gap-6 md:grid-cols-2">
        <section className="gl rounded-2xl p-6">
          <h2 className="label">Roles</h2>
          {user.roles.length === 0 ? (
            <p className="mt-2 text-sm text-white/45">No roles assigned.</p>
          ) : (
            <ul className="mt-3 flex flex-wrap gap-2">
              {user.roles.map((r) => (
                <li
                  key={r}
                  className="chip border-sky-300/30 bg-sky-400/10 text-sky-100"
                >
                  {r}
                </li>
              ))}
            </ul>
          )}
        </section>

        <section className="gl rounded-2xl p-6">
          <h2 className="label">Permissions</h2>
          {user.permissions.length === 0 ? (
            <p className="mt-2 text-sm text-white/45">
              No permissions on this token.
            </p>
          ) : (
            <ul className="mt-3 flex flex-wrap gap-2">
              {user.permissions.map((p) => (
                <li
                  key={p}
                  className="chip border-fuchsia-300/30 bg-fuchsia-400/10 text-fuchsia-100"
                >
                  {p}
                </li>
              ))}
            </ul>
          )}
        </section>
      </div>
    </div>
  );
}
