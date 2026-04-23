import { api } from "./api";

export type LoginRequest = { email: string; password: string };
export type RegisterRequest = { email: string; password: string };
export type LoginResponse = { token: string };

export const identityApi = {
  login: (body: LoginRequest) =>
    api.post<LoginResponse>("/identity/Login", body, { auth: false }),
  register: (body: RegisterRequest) =>
    api.post<void>("/identity/Register", body, { auth: false }),
};
