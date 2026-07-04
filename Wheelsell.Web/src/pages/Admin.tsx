import { useEffect, useState } from "react";
import { Ban, ShieldCheck, UserCog } from "lucide-react";
import { api } from "../api/client";
import type { AdminUserDto, PagedResult } from "../types";

export default function Admin() {
  const [users, setUsers] = useState<AdminUserDto[]>([]);
  const [banTarget, setBanTarget] = useState<number | null>(null);
  const [banReason, setBanReason] = useState("");

  const load = () => {
    api.get<PagedResult<AdminUserDto>>("/admin/users", { params: { page: 1, pageSize: 100 } }).then((r) => setUsers(r.data.items));
  };

  useEffect(() => {
    load();
  }, []);

  const ban = async (id: number) => {
    await api.post(`/admin/users/${id}/ban`, { reason: banReason || "Violation of marketplace rules" });
    setBanTarget(null);
    setBanReason("");
    load();
  };

  const unban = async (id: number) => {
    await api.post(`/admin/users/${id}/unban`);
    load();
  };

  const changeRole = async (id: number, role: string) => {
    await api.post(`/admin/users/${id}/role`, { role });
    load();
  };

  return (
    <div className="max-w-6xl mx-auto px-4 sm:px-6 py-8">
      <h1 className="font-display font-bold text-2xl mb-6 flex items-center gap-2">
        <ShieldCheck className="text-amber-400" /> Admin
      </h1>

      <div className="bg-base-900 border border-base-700 rounded-xl overflow-x-auto">
        <table className="w-full text-sm">
          <thead>
            <tr className="text-left text-ink-500 border-b border-base-700">
              <th className="p-3 font-medium">Username</th>
              <th className="p-3 font-medium">Email</th>
              <th className="p-3 font-medium">Role</th>
              <th className="p-3 font-medium">Status</th>
              <th className="p-3 font-medium">Actions</th>
            </tr>
          </thead>
          <tbody>
            {users.map((u) => (
              <tr key={u.id} className="border-b border-base-700 last:border-0">
                <td className="p-3 font-medium">{u.username}</td>
                <td className="p-3 text-ink-500">{u.email}</td>
                <td className="p-3">
                  <select
                    value={u.role}
                    onChange={(e) => changeRole(u.id, e.target.value)}
                    className="bg-base-800 border border-base-700 rounded-lg px-2 py-1 text-xs focus-ring"
                  >
                    <option value="User">User</option>
                    <option value="Moderator">Moderator</option>
                    <option value="Admin">Admin</option>
                  </select>
                </td>
                <td className="p-3">
                  {u.isBanned ? <span className="text-red-400 text-xs font-medium">Banned</span> : <span className="text-ink-500 text-xs">Active</span>}
                </td>
                <td className="p-3">
                  {u.isBanned ? (
                    <button onClick={() => unban(u.id)} className="text-amber-400 hover:text-amber-300 text-xs font-medium flex items-center gap-1">
                      <UserCog size={13} /> Unban
                    </button>
                  ) : banTarget === u.id ? (
                    <div className="flex items-center gap-1.5">
                      <input
                        value={banReason}
                        onChange={(e) => setBanReason(e.target.value)}
                        placeholder="Reason"
                        className="bg-base-800 border border-base-700 rounded-lg px-2 py-1 text-xs w-32 focus-ring"
                      />
                      <button onClick={() => ban(u.id)} className="text-red-400 hover:text-red-300 text-xs font-medium">
                        Confirm
                      </button>
                    </div>
                  ) : (
                    <button onClick={() => setBanTarget(u.id)} className="text-red-400 hover:text-red-300 text-xs font-medium flex items-center gap-1">
                      <Ban size={13} /> Ban
                    </button>
                  )}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}
