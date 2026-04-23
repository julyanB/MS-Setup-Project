"use client";

import { useRouter } from "next/navigation";
import { useEffect, type ReactNode } from "react";
import { useUser } from "@/contexts/UserContext";

export function ProtectedRoute({ children }: { children: ReactNode }) {
  const { status } = useUser();
  const router = useRouter();

  useEffect(() => {
    if (status === "unauthenticated") {
      router.replace("/login");
    }
  }, [status, router]);

  if (status !== "authenticated") {
    return (
      <div className="py-10 text-sm text-slate-500">Checking session…</div>
    );
  }

  return <>{children}</>;
}
