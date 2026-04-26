import { api } from "./api";

export type EmployeeLookupItem = {
  id: string;
  email: string | null;
  userName: string | null;
};

export type EmployeeLookupQuery = {
  role?: string;
  search?: string;
  limit?: number;
};

export const employeeApi = {
  lookup: ({ role, search, limit = 100 }: EmployeeLookupQuery = {}) => {
    const params = new URLSearchParams({ limit: String(limit) });
    if (role?.trim()) params.set("role", role.trim());
    if (search?.trim()) params.set("search", search.trim());

    return api.get<EmployeeLookupItem[]>(`/employees?${params.toString()}`);
  },
};

export function employeeLabel(employee: EmployeeLookupItem): string {
  return employee.email ?? employee.userName ?? employee.id;
}
