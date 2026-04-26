import { api } from "./api";

export type AdminUser = {
  id: string;
  email: string | null;
  userName: string | null;
  roleCount: number;
  permissionCount: number;
};

export type AdminUsersResponse = {
  items: AdminUser[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
};

export type AdminUsersQuery = {
  page: number;
  pageSize: number;
  search?: string;
};

export const adminApi = {
  getUsers: ({ page, pageSize, search }: AdminUsersQuery) => {
    const params = new URLSearchParams({
      page: String(page),
      pageSize: String(pageSize),
    });
    if (search?.trim()) params.set("search", search.trim());
    return api.get<AdminUsersResponse>(`/admin/users?${params.toString()}`);
  },
  getRoles: () => api.get<string[]>("/admin/roles"),
  createRole: (name: string) => api.post<void>("/admin/roles", { name }),
  getPermissions: () => api.get<string[]>("/admin/permissions"),
  createPermission: (name: string) =>
    api.post<void>("/admin/permissions", { name }),
  getUserRoles: (userId: string) =>
    api.get<string[]>(`/admin/users/${encodeURIComponent(userId)}/roles`),
  addUserRole: (userId: string, roleName: string) =>
    api.post<void>(`/admin/users/${encodeURIComponent(userId)}/roles`, {
      roleName,
    }),
  removeUserRole: (userId: string, roleName: string) =>
    api.del<void>(
      `/admin/users/${encodeURIComponent(userId)}/roles/${encodeURIComponent(roleName)}`,
    ),
  getUserPermissions: (userId: string) =>
    api.get<string[]>(
      `/admin/users/${encodeURIComponent(userId)}/permissions`,
    ),
  addUserPermission: (userId: string, permission: string) =>
    api.post<void>(`/admin/users/${encodeURIComponent(userId)}/permissions`, {
      permission,
    }),
  removeUserPermission: (userId: string, permission: string) =>
    api.del<void>(
      `/admin/users/${encodeURIComponent(userId)}/permissions/${encodeURIComponent(permission)}`,
    ),
  getRolePermissions: (roleName: string) =>
    api.get<string[]>(
      `/admin/roles/${encodeURIComponent(roleName)}/permissions`,
    ),
  addRolePermission: (roleName: string, permission: string) =>
    api.post<void>(
      `/admin/roles/${encodeURIComponent(roleName)}/permissions`,
      { permission },
    ),
  removeRolePermission: (roleName: string, permission: string) =>
    api.del<void>(
      `/admin/roles/${encodeURIComponent(roleName)}/permissions/${encodeURIComponent(permission)}`,
    ),
};
