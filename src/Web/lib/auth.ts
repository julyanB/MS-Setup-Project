const TOKEN_KEY = "ms.auth.token";

export function getToken(): string | null {
  if (typeof window === "undefined") return null;
  return window.localStorage.getItem(TOKEN_KEY);
}

export function setToken(token: string): void {
  window.localStorage.setItem(TOKEN_KEY, token);
}

export function clearToken(): void {
  window.localStorage.removeItem(TOKEN_KEY);
}

export type DecodedJwt = {
  sub?: string;
  email?: string;
  name?: string;
  role?: string | string[];
  permission?: string | string[];
  exp?: number;
  [claim: string]: unknown;
};

export function decodeJwt(token: string): DecodedJwt | null {
  const parts = token.split(".");
  if (parts.length !== 3) return null;
  try {
    const payload = parts[1].replace(/-/g, "+").replace(/_/g, "/");
    const padded = payload + "=".repeat((4 - (payload.length % 4)) % 4);
    const json = atob(padded);
    const decoded = decodeURIComponent(
      json
        .split("")
        .map((c) => "%" + c.charCodeAt(0).toString(16).padStart(2, "0"))
        .join(""),
    );
    return JSON.parse(decoded) as DecodedJwt;
  } catch {
    return null;
  }
}

export function isTokenExpired(decoded: DecodedJwt | null): boolean {
  if (!decoded?.exp) return false;
  return decoded.exp * 1000 < Date.now();
}

export function toArray(claim: string | string[] | undefined): string[] {
  if (!claim) return [];
  return Array.isArray(claim) ? claim : [claim];
}
