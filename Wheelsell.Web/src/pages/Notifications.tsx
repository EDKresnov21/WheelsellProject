import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { Bell, CheckCheck } from "lucide-react";
import { api } from "../api/client";
import type { NotificationDto } from "../types";

const typeIcons: Record<string, string> = {
  NewMessage: "💬",
  NewReview: "⭐",
  AdvertBanned: "🚫",
  AccountBanned: "🚫"
};

export default function Notifications() {
  const [notifications, setNotifications] = useState<NotificationDto[]>([]);

  const load = () => {
    api.get<NotificationDto[]>("/notifications").then((r) => setNotifications(r.data));
  };

  useEffect(() => {
    load();
  }, []);

  const markRead = async (id: number) => {
    await api.post(`/notifications/${id}/read`);
    setNotifications((prev) => prev.map((n) => (n.id === id ? { ...n, isRead: true } : n)));
  };

  const markAllRead = async () => {
    await api.post("/notifications/read-all");
    setNotifications((prev) => prev.map((n) => ({ ...n, isRead: true })));
  };

  const linkFor = (n: NotificationDto) => {
    if (n.relatedConversationId) return "/chat";
    if (n.relatedAdvertId) return `/adverts/${n.relatedAdvertId}`;
    return "#";
  };

  return (
    <div className="max-w-3xl mx-auto px-4 sm:px-6 py-8">
      <div className="flex items-center justify-between mb-6">
        <h1 className="font-display font-bold text-2xl">Notifications</h1>
        {notifications.some((n) => !n.isRead) && (
          <button onClick={markAllRead} className="flex items-center gap-1.5 text-sm text-amber-400 hover:text-amber-300">
            <CheckCheck size={15} /> Mark all read
          </button>
        )}
      </div>

      <div className="space-y-2">
        {notifications.map((n) => (
          <Link
            key={n.id}
            to={linkFor(n)}
            onClick={() => !n.isRead && markRead(n.id)}
            className={`flex items-start gap-3 p-4 rounded-xl border transition-colors ${
              n.isRead ? "bg-base-900 border-base-700" : "bg-base-800 border-amber-500/30"
            }`}
          >
            <span className="text-lg">{typeIcons[n.type] ?? <Bell size={18} />}</span>
            <div className="flex-1">
              <p className="text-sm">{n.message}</p>
              <p className="text-xs text-ink-500 mt-1">{new Date(n.createdAt).toLocaleString()}</p>
            </div>
            {!n.isRead && <span className="w-2 h-2 rounded-full bg-amber-500 mt-1.5 shrink-0" />}
          </Link>
        ))}
        {notifications.length === 0 && <p className="text-ink-500 text-sm">You have no notifications.</p>}
      </div>
    </div>
  );
}
