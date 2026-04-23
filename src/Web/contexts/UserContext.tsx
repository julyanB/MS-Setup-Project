"use client";

import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useState,
  type ReactNode,
} from "react";
import {
  clearToken,
  decodeJwt,
  getToken,
  isTokenExpired,
  setToken,
  toArray,
  type DecodedJwt,
} from "@/lib/auth";
import { identityApi, type LoginRequest, type RegisterRequest } from "@/lib/identity";

export type CurrentUser = {
  id: string | null;
  email: string | null;
  roles: string[];
  permissions: string[];
  raw: DecodedJwt;
};

type UserContextValue = {
  user: CurrentUser | null;
  token: string | null;
  status: "loading" | "authenticated" | "unauthenticated";
  login: (credentials: LoginRequest) => Promise<void>;
  register: (credentials: RegisterRequest) => Promise<void>;
  logout: () => void;
  hasRole: (role: string) => boolean;
  hasPermission: (permission: string) => boolean;
};

const UserContext = createContext<UserContextValue | null>(null);

function pickString(
  decoded: DecodedJwt,
  keys: readonly string[],
): string | null {
  for (const key of keys) {
    const value = decoded[key];
    if (typeof value === "string" && value.length > 0) return value;
  }
  return null;
}

function pickArray(
  decoded: DecodedJwt,
  keys: readonly string[],
): string[] {
  for (const key of keys) {
    const value = decoded[key];
    if (value !== undefined) {
      return toArray(value as string | string[]);
    }
  }
  return [];
}

const ID_CLAIMS = [
  "sub",
  "nameid",
  "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier",
] as const;

const EMAIL_CLAIMS = [
  "email",
  "unique_name",
  "name",
  "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name",
  "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress",
] as const;

const ROLE_CLAIMS = [
  "role",
  "roles",
  "http://schemas.microsoft.com/ws/2008/06/identity/claims/role",
] as const;

const PERMISSION_CLAIMS = ["permission", "permissions"] as const;

function toCurrentUser(token: string): CurrentUser | null {
  const decoded = decodeJwt(token);
  if (!decoded || isTokenExpired(decoded)) return null;

  return {
    id: pickString(decoded, ID_CLAIMS),
    email: pickString(decoded, EMAIL_CLAIMS),
    roles: pickArray(decoded, ROLE_CLAIMS),
    permissions: pickArray(decoded, PERMISSION_CLAIMS),
    raw: decoded,
  };
}

export function UserProvider({ children }: { children: ReactNode }) {
  const [token, setTokenState] = useState<string | null>(null);
  const [user, setUser] = useState<CurrentUser | null>(null);
  const [status, setStatus] = useState<UserContextValue["status"]>("loading");

  useEffect(() => {
    const existing = getToken();
    if (!existing) {
      setStatus("unauthenticated");
      return;
    }
    const current = toCurrentUser(existing);
    if (!current) {
      clearToken();
      setStatus("unauthenticated");
      return;
    }
    setTokenState(existing);
    setUser(current);
    setStatus("authenticated");
  }, []);

  const applyToken = useCallback((nextToken: string) => {
    const current = toCurrentUser(nextToken);
    if (!current) {
      clearToken();
      setTokenState(null);
      setUser(null);
      setStatus("unauthenticated");
      throw new Error("Received token is invalid or expired");
    }
    setToken(nextToken);
    setTokenState(nextToken);
    setUser(current);
    setStatus("authenticated");
  }, []);

  const login = useCallback<UserContextValue["login"]>(
    async (credentials) => {
      const { token: received } = await identityApi.login(credentials);
      applyToken(received);
    },
    [applyToken],
  );

  const register = useCallback<UserContextValue["register"]>(
    async (credentials) => {
      await identityApi.register(credentials);
      await login(credentials);
    },
    [login],
  );

  const logout = useCallback(() => {
    clearToken();
    setTokenState(null);
    setUser(null);
    setStatus("unauthenticated");
  }, []);

  const value = useMemo<UserContextValue>(
    () => ({
      user,
      token,
      status,
      login,
      register,
      logout,
      hasRole: (role) => user?.roles.includes(role) ?? false,
      hasPermission: (permission) =>
        user?.permissions.includes(permission) ?? false,
    }),
    [user, token, status, login, register, logout],
  );

  return <UserContext.Provider value={value}>{children}</UserContext.Provider>;
}

export function useUser(): UserContextValue {
  const ctx = useContext(UserContext);
  if (!ctx) {
    throw new Error("useUser must be used inside <UserProvider>");
  }
  return ctx;
}
