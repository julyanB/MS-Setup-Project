# Web (Next.js)

Next.js 15 (App Router) frontend that talks to the gateway.

## Setup

```bash
cd src/Web
cp .env.local.example .env.local   # point NEXT_PUBLIC_GATEWAY_URL at your gateway
npm install
npm run dev
```

Open http://localhost:3000.

## How it wires up

- `lib/api.ts` — `fetch` wrapper that prefixes `NEXT_PUBLIC_GATEWAY_URL` and attaches `Authorization: Bearer <token>`.
- `lib/identity.ts` — typed helpers for `POST /identity/Login` and `POST /identity/Register`.
- `lib/auth.ts` — token storage (localStorage) + JWT payload decoding.
- `contexts/UserContext.tsx` — global user context: `useUser()` exposes `user`, `status`, `login`, `register`, `logout`, `hasRole`, `hasPermission`.
- `components/ProtectedRoute.tsx` — client-side route guard that redirects to `/login` if unauthenticated.
- `app/login`, `app/register`, `app/dashboard` — example pages exercising the context.

The gateway must include `http://localhost:3000` in `Cors:AllowedOrigins` (already added in `src/Gateway/appsettings.json`).
