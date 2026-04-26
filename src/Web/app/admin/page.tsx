"use client";

import { useCallback, useEffect, useMemo, useState } from "react";
import { ProtectedRoute } from "@/components/ProtectedRoute";
import { useUser } from "@/contexts/UserContext";
import { ApiError } from "@/lib/api";
import { adminApi, type AdminUser } from "@/lib/admin";

type LoadState = {
  users: AdminUser[];
  roles: string[];
  permissions: string[];
  userPage: number;
  userPageSize: number;
  userTotalCount: number;
  userTotalPages: number;
};

const emptyState: LoadState = {
  users: [],
  roles: [],
  permissions: [],
  userPage: 1,
  userPageSize: 10,
  userTotalCount: 0,
  userTotalPages: 1,
};

type SelectedUserAccess = {
  roles: string[];
  permissions: string[];
};

const USER_PAGE_SIZE = 10;

export default function AdminPage() {
  return (
    <ProtectedRoute>
      <AdminContent />
    </ProtectedRoute>
  );
}

function AdminContent() {
  const { hasPermission } = useUser();
  const [data, setData] = useState<LoadState>(emptyState);
  const [selectedUserId, setSelectedUserId] = useState<string | null>(null);
  const [selectedRole, setSelectedRole] = useState<string | null>(null);
  const [selectedUserAccess, setSelectedUserAccess] =
    useState<SelectedUserAccess>({ roles: [], permissions: [] });
  const [newRole, setNewRole] = useState("");
  const [newPermission, setNewPermission] = useState("");
  const [userFilter, setUserFilter] = useState("");
  const [userPage, setUserPage] = useState(1);
  const [loading, setLoading] = useState(true);
  const [loadingUserAccess, setLoadingUserAccess] = useState(false);
  const [saving, setSaving] = useState(false);
  const [message, setMessage] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);

  const canManage = hasPermission("roles.manage");

  const load = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const [usersResponse, roles, permissions] = await Promise.all([
        adminApi.getUsers({
          page: userPage,
          pageSize: USER_PAGE_SIZE,
          search: userFilter,
        }),
        adminApi.getRoles(),
        adminApi.getPermissions(),
      ]);

      const users = Array.isArray(usersResponse.items) ? usersResponse.items : [];

      setData({
        users,
        roles,
        permissions,
        userPage: usersResponse.page || userPage,
        userPageSize: usersResponse.pageSize || USER_PAGE_SIZE,
        userTotalCount: usersResponse.totalCount || users.length,
        userTotalPages: usersResponse.totalPages || 1,
      });
      setSelectedUserId((current) =>
        users.some((user) => user.id === current)
          ? current
          : users[0]?.id ?? null,
      );
      setSelectedRole((current) => current ?? roles[0] ?? null);
    } catch (caught) {
      setError(toErrorMessage(caught, "Could not load admin data."));
    } finally {
      setLoading(false);
    }
  }, [userFilter, userPage]);

  useEffect(() => {
    void load();
  }, [load]);

  const selectedUser = useMemo(
    () => data.users.find((user) => user.id === selectedUserId) ?? null,
    [data.users, selectedUserId],
  );

  useEffect(() => {
    if (!selectedUserId) {
      setSelectedUserAccess({ roles: [], permissions: [] });
      return;
    }

    let cancelled = false;
    setLoadingUserAccess(true);
    Promise.all([
      adminApi.getUserRoles(selectedUserId),
      adminApi.getUserPermissions(selectedUserId),
    ])
      .then(([roles, permissions]) => {
        if (!cancelled) setSelectedUserAccess({ roles, permissions });
      })
      .catch((caught) => {
        if (!cancelled) {
          setSelectedUserAccess({ roles: [], permissions: [] });
          setError(toErrorMessage(caught, "Could not load user access."));
        }
      })
      .finally(() => {
        if (!cancelled) setLoadingUserAccess(false);
      });

    return () => {
      cancelled = true;
    };
  }, [selectedUserId]);

  function setUserSearch(value: string) {
    setUserFilter(value);
    setUserPage(1);
  }

  async function refreshSelectedUserAccess(userId: string | null) {
    if (!userId) {
      setSelectedUserAccess({ roles: [], permissions: [] });
      return;
    }

    const [roles, permissions] = await Promise.all([
      adminApi.getUserRoles(userId),
      adminApi.getUserPermissions(userId),
    ]);
    setSelectedUserAccess({ roles, permissions });
  }

  async function runMutation(action: () => Promise<void>, success: string) {
    setSaving(true);
    setError(null);
    setMessage(null);
    try {
      await action();
      await load();
      await refreshSelectedUserAccess(selectedUserId);
      setMessage(success);
    } catch (caught) {
      setError(toErrorMessage(caught, "Admin action failed."));
    } finally {
      setSaving(false);
    }
  }

  async function createRole() {
    const name = newRole.trim();
    if (!name) return;
    await runMutation(async () => adminApi.createRole(name), "Role created.");
    setNewRole("");
    setSelectedRole(name);
  }

  async function createPermission() {
    const name = newPermission.trim();
    if (!name) return;
    await runMutation(
      async () => adminApi.createPermission(name),
      "Permission created.",
    );
    setNewPermission("");
  }

  if (!canManage) {
    return (
      <section className="gls rounded-2xl p-6">
        <p className="label">Admin</p>
        <h1 className="display text-2xl font-bold">Access denied</h1>
        <p className="mt-2 text-sm text-white/60">
          Your account needs the roles.manage permission to open this panel.
        </p>
      </section>
    );
  }

  return (
    <div className="space-y-6">
      <header className="flex flex-wrap items-end justify-between gap-4">
        <div>
          <p className="text-xs font-semibold uppercase tracking-widest text-white/45">
            Digi administration
          </p>
          <h1 className="display mt-2 text-3xl font-bold">Admin panel</h1>
          <p className="mt-1 max-w-3xl text-sm text-white/55">
            Manage users, roles, permissions, and the assignments between them.
          </p>
        </div>
        <button
          type="button"
          onClick={() => void load()}
          disabled={loading || saving}
          className="btn-ghost"
        >
          Refresh
        </button>
      </header>

      {(error || message) && (
        <div
          className={`rounded-2xl border p-3 text-sm ${
            error
              ? "border-rose-300/30 bg-rose-400/10 text-rose-100"
              : "border-emerald-300/30 bg-emerald-400/10 text-emerald-100"
          }`}
        >
          {error ?? message}
        </div>
      )}

      <section className="grid gap-5 xl:grid-cols-[minmax(0,1.2fr)_minmax(420px,0.8fr)]">
        <UsersWidget
          users={data.users}
          allRoles={data.roles}
          allPermissions={data.permissions}
          selectedUser={selectedUser}
          selectedUserAccess={selectedUserAccess}
          selectedUserId={selectedUserId}
          filter={userFilter}
          page={data.userPage}
          totalPages={data.userTotalPages}
          totalCount={data.userTotalCount}
          loading={loading}
          loadingUserAccess={loadingUserAccess}
          saving={saving}
          onFilter={setUserSearch}
          onPrevPage={() => setUserPage((current) => Math.max(1, current - 1))}
          onNextPage={() =>
            setUserPage((current) => Math.min(data.userTotalPages, current + 1))
          }
          onSelect={setSelectedUserId}
          onAddRole={(role) =>
            selectedUser
              ? runMutation(
                  async () => adminApi.addUserRole(selectedUser.id, role),
                  "Role attached to user.",
                )
              : Promise.resolve()
          }
          onRemoveRole={(role) =>
            selectedUser
              ? runMutation(
                  async () => adminApi.removeUserRole(selectedUser.id, role),
                  "Role removed from user.",
                )
              : Promise.resolve()
          }
          onAddPermission={(permission) =>
            selectedUser
              ? runMutation(
                  async () =>
                    adminApi.addUserPermission(selectedUser.id, permission),
                  "Permission attached to user.",
                )
              : Promise.resolve()
          }
          onRemovePermission={(permission) =>
            selectedUser
              ? runMutation(
                  async () =>
                    adminApi.removeUserPermission(selectedUser.id, permission),
                  "Permission removed from user.",
                )
              : Promise.resolve()
          }
        />

        <RolesWidget
          roles={data.roles}
          permissions={data.permissions}
          selectedRole={selectedRole}
          newRole={newRole}
          newPermission={newPermission}
          loading={loading}
          saving={saving}
          onSelectRole={setSelectedRole}
          onNewRole={setNewRole}
          onNewPermission={setNewPermission}
          onCreateRole={() => void createRole()}
          onCreatePermission={() => void createPermission()}
          onAddPermission={(permission) =>
            selectedRole
              ? runMutation(
                  async () =>
                    adminApi.addRolePermission(selectedRole, permission),
                  "Permission attached to role.",
                )
              : Promise.resolve()
          }
          onRemovePermission={(permission) =>
            selectedRole
              ? runMutation(
                  async () =>
                    adminApi.removeRolePermission(selectedRole, permission),
                  "Permission removed from role.",
                )
              : Promise.resolve()
          }
        />
      </section>
    </div>
  );
}

function UsersWidget({
  users,
  allRoles,
  allPermissions,
  selectedUser,
  selectedUserAccess,
  selectedUserId,
  filter,
  page,
  totalPages,
  totalCount,
  loading,
  loadingUserAccess,
  saving,
  onFilter,
  onPrevPage,
  onNextPage,
  onSelect,
  onAddRole,
  onRemoveRole,
  onAddPermission,
  onRemovePermission,
}: {
  users: AdminUser[];
  allRoles: string[];
  allPermissions: string[];
  selectedUser: AdminUser | null;
  selectedUserAccess: SelectedUserAccess;
  selectedUserId: string | null;
  filter: string;
  page: number;
  totalPages: number;
  totalCount: number;
  loading: boolean;
  loadingUserAccess: boolean;
  saving: boolean;
  onFilter: (value: string) => void;
  onPrevPage: () => void;
  onNextPage: () => void;
  onSelect: (id: string) => void;
  onAddRole: (role: string) => Promise<void>;
  onRemoveRole: (role: string) => Promise<void>;
  onAddPermission: (permission: string) => Promise<void>;
  onRemovePermission: (permission: string) => Promise<void>;
}) {
  const availableRoles = allRoles.filter(
    (role) => !selectedUserAccess.roles.includes(role),
  );
  const availablePermissions = allPermissions.filter(
    (permission) => !selectedUserAccess.permissions.includes(permission),
  );

  return (
    <section className="gls rounded-2xl p-5">
      <div className="flex flex-wrap items-end justify-between gap-3">
        <div>
          <h2 className="display text-xl font-semibold">Users</h2>
          <p className="mt-1 text-sm text-white/55">
            Select a user and manage direct roles or permissions.
          </p>
        </div>
        <span className="chip">{totalCount} users</span>
      </div>

      <div className="mt-5 grid gap-4 lg:grid-cols-[minmax(260px,0.85fr)_minmax(0,1.15fr)]">
        <div className="space-y-3">
          <input
            className="field"
            value={filter}
            onChange={(event) => onFilter(event.target.value)}
            placeholder="Search users"
          />
          <div className="max-h-[560px] overflow-y-auto rounded-2xl border border-white/10">
            {users.map((user) => {
              const active = user.id === selectedUserId;
              return (
                <button
                  key={user.id}
                  type="button"
                  onClick={() => onSelect(user.id)}
                  className={`block w-full border-b border-white/10 px-4 py-3 text-left transition last:border-b-0 ${
                    active
                      ? "bg-fuchsia-300/20 text-white"
                      : "bg-white/[0.03] text-white/75 hover:bg-white/[0.09]"
                  }`}
                >
                  <span className="display block text-sm font-semibold">
                    {user.email ?? user.userName ?? user.id}
                  </span>
                  <span className="mt-1 block truncate font-mono text-[11px] text-white/40">
                    {user.id}
                  </span>
                  <span className="mt-2 flex gap-2 text-[11px] text-white/45">
                    <span>{user.roleCount} roles</span>
                    <span>{user.permissionCount} direct permissions</span>
                  </span>
                </button>
              );
            })}
            {users.length === 0 && (
              <p className="px-4 py-8 text-center text-sm text-white/50">
                {loading ? "Loading users..." : "No users found."}
              </p>
            )}
          </div>
          <div className="flex items-center justify-between gap-3 text-xs text-white/55">
            <span>
              Page {page} of {totalPages}
            </span>
            <div className="flex gap-2">
              <button
                type="button"
                onClick={onPrevPage}
                disabled={page <= 1 || loading}
                className="btn-ghost px-3 py-1.5 text-xs disabled:opacity-40"
              >
                Prev
              </button>
              <button
                type="button"
                onClick={onNextPage}
                disabled={page >= totalPages || loading}
                className="btn-ghost px-3 py-1.5 text-xs disabled:opacity-40"
              >
                Next
              </button>
            </div>
          </div>
        </div>

        <div className="space-y-4">
          {selectedUser ? (
            <>
              <div className="rounded-2xl border border-white/10 bg-white/[0.05] p-4">
                <p className="label">Selected user</p>
                <h3 className="display text-lg font-bold">
                  {selectedUser.email ?? selectedUser.userName}
                </h3>
                <p className="mt-1 break-all font-mono text-xs text-white/45">
                  {selectedUser.id}
                </p>
              </div>

              <AssignmentBox
                title="User roles"
                assigned={selectedUserAccess.roles}
                available={availableRoles}
                placeholder="Add role"
                saving={saving || loadingUserAccess}
                onAdd={onAddRole}
                onRemove={onRemoveRole}
              />

              <AssignmentBox
                title="Direct permissions"
                assigned={selectedUserAccess.permissions}
                available={availablePermissions}
                placeholder="Add permission"
                saving={saving || loadingUserAccess}
                onAdd={onAddPermission}
                onRemove={onRemovePermission}
              />
            </>
          ) : (
            <div className="rounded-2xl border border-white/10 bg-white/[0.05] p-8 text-center text-sm text-white/50">
              Select a user to manage access.
            </div>
          )}
        </div>
      </div>
    </section>
  );
}

function RolesWidget({
  roles,
  permissions,
  selectedRole,
  newRole,
  newPermission,
  loading,
  saving,
  onSelectRole,
  onNewRole,
  onNewPermission,
  onCreateRole,
  onCreatePermission,
  onAddPermission,
  onRemovePermission,
}: {
  roles: string[];
  permissions: string[];
  selectedRole: string | null;
  newRole: string;
  newPermission: string;
  loading: boolean;
  saving: boolean;
  onSelectRole: (role: string) => void;
  onNewRole: (value: string) => void;
  onNewPermission: (value: string) => void;
  onCreateRole: () => void;
  onCreatePermission: () => void;
  onAddPermission: (permission: string) => Promise<void>;
  onRemovePermission: (permission: string) => Promise<void>;
}) {
  const [rolePermissions, setRolePermissions] = useState<string[]>([]);
  const [loadingRole, setLoadingRole] = useState(false);
  const availablePermissions = permissions.filter(
    (permission) => !rolePermissions.includes(permission),
  );

  useEffect(() => {
    if (!selectedRole) {
      setRolePermissions([]);
      return;
    }

    let cancelled = false;
    setLoadingRole(true);
    adminApi
      .getRolePermissions(selectedRole)
      .then((items) => {
        if (!cancelled) setRolePermissions(items);
      })
      .catch(() => {
        if (!cancelled) setRolePermissions([]);
      })
      .finally(() => {
        if (!cancelled) setLoadingRole(false);
      });

    return () => {
      cancelled = true;
    };
  }, [selectedRole, permissions]);

  async function addPermission(permission: string) {
    await onAddPermission(permission);
    if (selectedRole) {
      setRolePermissions(await adminApi.getRolePermissions(selectedRole));
    }
  }

  async function removePermission(permission: string) {
    await onRemovePermission(permission);
    if (selectedRole) {
      setRolePermissions(await adminApi.getRolePermissions(selectedRole));
    }
  }

  return (
    <aside className="space-y-5">
      <section className="gls rounded-2xl p-5">
        <h2 className="display text-xl font-semibold">Roles</h2>
        <p className="mt-1 text-sm text-white/55">
          Create roles and attach permissions to the selected role.
        </p>

        <div className="mt-5 grid grid-cols-[minmax(0,1fr)_auto] gap-2">
          <input
            className="field"
            value={newRole}
            onChange={(event) => onNewRole(event.target.value)}
            placeholder="New role name"
          />
          <button
            type="button"
            onClick={onCreateRole}
            disabled={!newRole.trim() || saving}
            className="btn-primary"
          >
            Create
          </button>
        </div>

        <div className="mt-4 max-h-72 overflow-y-auto rounded-2xl border border-white/10">
          {roles.map((role) => {
            const active = role === selectedRole;
            return (
              <button
                key={role}
                type="button"
                onClick={() => onSelectRole(role)}
                className={`block w-full border-b border-white/10 px-4 py-3 text-left text-sm font-semibold transition last:border-b-0 ${
                  active
                    ? "bg-fuchsia-300/20 text-white"
                    : "bg-white/[0.03] text-white/75 hover:bg-white/[0.09]"
                }`}
              >
                {role}
              </button>
            );
          })}
          {roles.length === 0 && (
            <p className="px-4 py-8 text-center text-sm text-white/50">
              {loading ? "Loading roles..." : "No roles found."}
            </p>
          )}
        </div>

        <div className="mt-4">
          <AssignmentBox
            title={selectedRole ? `${selectedRole} permissions` : "Role permissions"}
            assigned={rolePermissions}
            available={availablePermissions}
            placeholder="Add permission"
            saving={saving || loadingRole || !selectedRole}
            onAdd={addPermission}
            onRemove={removePermission}
          />
        </div>
      </section>

      <section className="gls rounded-2xl p-5">
        <h2 className="display text-xl font-semibold">Permissions</h2>
        <p className="mt-1 text-sm text-white/55">
          Create reusable permission names for gateway policies and role grants.
        </p>
        <div className="mt-5 grid grid-cols-[minmax(0,1fr)_auto] gap-2">
          <input
            className="field"
            value={newPermission}
            onChange={(event) => onNewPermission(event.target.value)}
            placeholder="permission.name"
          />
          <button
            type="button"
            onClick={onCreatePermission}
            disabled={!newPermission.trim() || saving}
            className="btn-primary"
          >
            Create
          </button>
        </div>
        <div className="mt-4 flex max-h-48 flex-wrap gap-2 overflow-y-auto">
          {permissions.map((permission) => (
            <span key={permission} className="chip">
              {permission}
            </span>
          ))}
        </div>
      </section>
    </aside>
  );
}

function AssignmentBox({
  title,
  assigned,
  available,
  placeholder,
  saving,
  onAdd,
  onRemove,
}: {
  title: string;
  assigned: string[];
  available: string[];
  placeholder: string;
  saving: boolean;
  onAdd: (value: string) => Promise<void>;
  onRemove: (value: string) => Promise<void>;
}) {
  const [selected, setSelected] = useState("");

  useEffect(() => {
    setSelected("");
  }, [available.join("|")]);

  return (
    <div className="rounded-2xl border border-white/10 bg-white/[0.05] p-4">
      <div className="flex items-center justify-between gap-3">
        <h3 className="display text-sm font-semibold">{title}</h3>
        <span className="chip">{assigned.length}</span>
      </div>
      <div className="mt-3 flex max-h-32 flex-wrap gap-2 overflow-y-auto pr-1">
        {assigned.map((item) => (
          <span key={item} className="chip max-w-full pr-1">
            <span className="truncate">{item}</span>
            <button
              type="button"
              onClick={() => void onRemove(item)}
              disabled={saving}
              className="rounded-full px-1.5 text-white/55 transition hover:bg-white/10 hover:text-white disabled:opacity-40"
              aria-label={`Remove ${item}`}
            >
              x
            </button>
          </span>
        ))}
        {assigned.length === 0 && (
          <p className="text-sm text-white/45">Nothing assigned.</p>
        )}
      </div>
      <div className="mt-4 grid grid-cols-[minmax(0,1fr)_auto] gap-2">
        <select
          className="field"
          value={selected}
          onChange={(event) => setSelected(event.target.value)}
          disabled={saving || available.length === 0}
        >
          <option value="">{placeholder}</option>
          {available.map((item) => (
            <option key={item} value={item}>
              {item}
            </option>
          ))}
        </select>
        <button
          type="button"
          onClick={() => selected && void onAdd(selected)}
          disabled={!selected || saving}
          className="btn-ghost"
        >
          Add
        </button>
      </div>
    </div>
  );
}

function toErrorMessage(caught: unknown, fallback: string): string {
  if (caught instanceof ApiError) return caught.message;
  if (caught instanceof Error) return caught.message;
  return fallback;
}
