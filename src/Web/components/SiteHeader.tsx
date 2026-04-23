"use client";

import Link from "next/link";
import { useState } from "react";
import { useUser } from "@/contexts/UserContext";
import { DigiLogo } from "@/components/DigiLogo";

export function SiteHeader() {
  const { user, status, logout } = useUser();
  const [profileOpen, setProfileOpen] = useState(false);

  const displayName = user?.email?.split("@")[0] ?? "Digi user";
  const initials = displayName.slice(0, 2).toUpperCase();

  return (
    <header className="relative z-[100] border-b border-white/10 bg-black/20 backdrop-blur-xl">
      <div className="mx-auto flex max-w-7xl items-center justify-between px-6 py-3">
        <Link
          href="/"
          className="display flex items-center gap-3 text-base font-semibold tracking-tight"
        >
          <DigiLogo size={32} className="shadow-lg shadow-fuchsia-900/40" />
          <span className="text-white/90">Digi</span>
        </Link>

        <nav className="flex items-center gap-3 text-sm">
          {status === "authenticated" && (
            <>
              <Link
                href="/dashboard"
                className="rounded-lg px-3 py-1.5 text-white/70 transition hover:bg-white/10 hover:text-white"
              >
                Home
              </Link>
              <Link
                href="/requests"
                className="rounded-lg px-3 py-1.5 text-white/70 transition hover:bg-white/10 hover:text-white"
              >
                Requests
              </Link>
            </>
          )}

          {status === "authenticated" ? (
            <div className="flex items-center gap-3">
              <button type="button" onClick={logout} className="btn-ghost">
                Sign out
              </button>
              <button
                type="button"
                onClick={() => setProfileOpen((open) => !open)}
                className="flex items-center gap-2 rounded-xl border border-white/15 bg-white/[0.08] px-2.5 py-1.5 text-left transition hover:bg-white/[0.14]"
              >
                <span className="grid h-8 w-8 place-items-center rounded-lg bg-gradient-to-br from-violet-500 to-sky-500 text-[11px] font-bold">
                  {initials}
                </span>
                <span className="hidden text-sm font-semibold text-white/80 sm:block">
                  {displayName}
                </span>
              </button>
            </div>
          ) : status === "unauthenticated" ? (
            <div className="flex items-center gap-3">
              <Link
                href="/login"
                className="rounded-lg px-3 py-1.5 text-white/75 transition hover:text-white"
              >
                Sign in
              </Link>
              <Link href="/register" className="btn-primary">
                Create account
              </Link>
            </div>
          ) : null}
        </nav>
      </div>

      {status === "authenticated" && profileOpen && (
        <div className="fixed right-6 top-[58px] z-[9999] w-[min(340px,calc(100vw-48px))]">
          <button
            type="button"
            className="fixed inset-0 -z-10 cursor-default bg-transparent"
            aria-label="Close profile"
            onClick={() => setProfileOpen(false)}
          />
          <aside className="gls rounded-3xl p-5 shadow-2xl shadow-black/45">
            <div className="flex items-start justify-between gap-4">
              <div>
                <p className="text-xs font-semibold uppercase tracking-widest text-white/45">
                  Profile
                </p>
                <h2 className="display mt-2 text-xl font-bold">
                  {displayName}
                </h2>
                <p className="text-sm text-white/55">Office staff - BG</p>
              </div>
              <button type="button" onClick={logout} className="btn-ghost px-3">
                Sign out
              </button>
            </div>

            <div className="mt-6 grid place-items-center">
              <div className="grid h-24 w-24 place-items-center rounded-full border border-white/25 bg-gradient-to-br from-violet-500 to-sky-500 text-2xl font-bold shadow-xl shadow-fuchsia-500/30">
                {initials}
              </div>
              <p className="mt-4 text-sm text-white/75">{user?.email}</p>
              <span className="chip mt-3 border-white/20 bg-white/10">HQ</span>
            </div>

            <div className="mt-6">
              <label className="label">Current role</label>
              <select className="field">
                <option>Office staff</option>
                <option>Secretary</option>
                <option>Chairperson</option>
              </select>
            </div>

            <div className="mt-4 grid grid-cols-3 gap-2">
              {["Notifications", "Self Service", "ESG"].map((tab) => (
                <button key={tab} type="button" className="btn-ghost px-3">
                  {tab}
                </button>
              ))}
            </div>

            <p className="mt-6 text-center text-sm font-semibold text-white/55">
              Nothing to see here...
            </p>
          </aside>
        </div>
      )}
    </header>
  );
}
