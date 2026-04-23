"use client";

import Link from "next/link";
import { useUser } from "@/contexts/UserContext";
import { DigiLogo } from "@/components/DigiLogo";

export default function HomePage() {
  const { status, user } = useUser();

  return (
    <div className="space-y-8">
      <section className="gls rounded-3xl p-8">
        <div className="flex items-center gap-4">
          <DigiLogo size={56} className="shadow-lg shadow-fuchsia-900/40" />
          <div>
            <p className="text-xs font-semibold uppercase tracking-widest text-white/45">
              Gateway control plane
            </p>
            <h1 className="display mt-1 text-3xl font-bold tracking-tight sm:text-4xl">
              Digi
            </h1>
          </div>
        </div>
        <p className="mt-5 max-w-2xl text-sm text-white/60">
          Frontend that authenticates against the gateway and surfaces user
          identity, roles, and permissions.
        </p>
        <p className="mt-4 text-xs text-white/45">
          Talking to{" "}
          <code className="rounded bg-white/10 px-2 py-0.5 text-white/80">
            {process.env.NEXT_PUBLIC_GATEWAY_URL ?? "http://localhost:5000"}
          </code>
        </p>
      </section>

      {status === "authenticated" ? (
        <section className="gl rounded-2xl p-6">
          <p className="label">Signed in as</p>
          <p className="display text-lg font-semibold">{user?.email}</p>
          <div className="mt-5 flex gap-3">
            <Link href="/dashboard" className="btn-primary">
              Go to dashboard
            </Link>
          </div>
        </section>
      ) : (
        <section className="flex flex-wrap gap-3">
          <Link href="/login" className="btn-ghost">
            Sign in
          </Link>
          <Link href="/register" className="btn-primary">
            Create account
          </Link>
        </section>
      )}
    </div>
  );
}
