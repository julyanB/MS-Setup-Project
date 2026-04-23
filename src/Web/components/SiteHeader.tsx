"use client";

import Link from "next/link";
import { useUser } from "@/contexts/UserContext";
import { DigiLogo } from "@/components/DigiLogo";

export function SiteHeader() {
  const { user, status, logout } = useUser();

  const initials = (user?.email ?? "?")
    .split(/[@.]/)[0]
    .slice(0, 2)
    .toUpperCase();

  return (
    <header className="border-b border-white/10 bg-black/20 backdrop-blur-xl">
      <div className="mx-auto flex max-w-5xl items-center justify-between px-6 py-3">
        <Link
          href="/"
          className="display flex items-center gap-3 text-base font-semibold tracking-tight"
        >
          <DigiLogo size={32} className="shadow-lg shadow-fuchsia-900/40" />
          <span className="text-white/90">Digi</span>
        </Link>

        <nav className="flex items-center gap-3 text-sm">
          {status === "authenticated" && (
            <Link
              href="/dashboard"
              className="rounded-lg px-3 py-1.5 text-white/70 transition hover:bg-white/10 hover:text-white"
            >
              Dashboard
            </Link>
          )}

          {status === "authenticated" ? (
            <div className="flex items-center gap-3">
              <div className="hidden items-center gap-2 sm:flex">
                <div className="grid h-8 w-8 place-items-center rounded-xl border border-white/25 bg-gradient-to-br from-violet-500 to-sky-500 text-[11px] font-semibold">
                  {initials}
                </div>
                <span className="text-white/75">{user?.email}</span>
              </div>
              <button type="button" onClick={logout} className="btn-ghost">
                Sign out
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
    </header>
  );
}
