import { useEffect, useRef, useState } from "react";
import * as signalR from "@microsoft/signalr";
import { Send, MessageSquare } from "lucide-react";
import { api, fileUrl, API_BASE_URL } from "../api/client";
import { useAuthStore } from "../store/authStore";
import type { ConversationDto, MessageDto } from "../types";

export default function Chat() {
  const { user, accessToken } = useAuthStore();
  const [conversations, setConversations] = useState<ConversationDto[]>([]);
  const [activeId, setActiveId] = useState<number | null>(null);
  const [messages, setMessages] = useState<MessageDto[]>([]);
  const [text, setText] = useState("");
  const [sending, setSending] = useState(false);
  const connectionRef = useRef<signalR.HubConnection | null>(null);
  const bottomRef = useRef<HTMLDivElement>(null);

  const loadConversations = () => {
    api.get<ConversationDto[]>("/chat/conversations").then((r) => {
      setConversations(r.data);
    });
  };

  useEffect(() => {
    loadConversations();
  }, []);

  useEffect(() => {
    if (!accessToken) return;

    const connection = new signalR.HubConnectionBuilder()
      .withUrl(`${API_BASE_URL}/hubs/chat?access_token=${accessToken}`)
      .withAutomaticReconnect()
      .build();

    connection.on("MessageReceived", (message: MessageDto) => {
      setMessages((prev) => (prev.some((m) => m.id === message.id) ? prev : [...prev, message]));
      loadConversations();
    });

    connection.start().catch(() => {
      // SignalR unavailable — fallback to REST polling
    });
    connectionRef.current = connection;

    return () => {
      connection.stop();
    };
  }, [accessToken]);

  useEffect(() => {
    if (!activeId) return;
    api.get<MessageDto[]>(`/chat/conversations/${activeId}/messages`).then((r) => setMessages(r.data));
    api.post(`/chat/conversations/${activeId}/read`).then(() => {
      setConversations((prev) => prev.map((c) => (c.id === activeId ? { ...c, unreadCount: 0 } : c)));
    }).catch(() => {});
  }, [activeId]);

  useEffect(() => {
    bottomRef.current?.scrollIntoView({ behavior: "smooth" });
  }, [messages]);

  const sendMessage = async () => {
    if (!text.trim() || !activeId || sending) return;
    const content = text.trim();
    setText("");
    setSending(true);

    try {
      if (connectionRef.current?.state === signalR.HubConnectionState.Connected) {
        await connectionRef.current.invoke("SendMessage", { conversationId: activeId, content });
      } else {
        // REST fallback
        const res = await api.post<MessageDto>(`/chat/conversations/${activeId}/messages`, { content });
        setMessages((prev) => [...prev, res.data]);
        loadConversations();
      }
    } catch {
      // restore text if send failed
      setText(content);
    } finally {
      setSending(false);
    }
  };

  const active = conversations.find((c) => c.id === activeId);

  if (!user) return null;

  return (
    <div className="max-w-6xl mx-auto px-4 sm:px-6 py-8">
      <h1 className="font-display font-bold text-2xl mb-6">Messages</h1>

      <div className="grid sm:grid-cols-[280px_1fr] bg-base-900 border border-base-700 rounded-xl overflow-hidden" style={{ height: "70vh" }}>

        {/* Conversation list */}
        <div className="border-r border-base-700 overflow-y-auto">
          {conversations.length === 0 && (
            <div className="flex flex-col items-center justify-center h-full gap-3 text-ink-500 p-6 text-center">
              <MessageSquare size={32} className="opacity-40" />
              <p className="text-sm">No conversations yet. Contact a seller from an advert page to start one.</p>
            </div>
          )}
          {conversations.map((c) => (
            <button
              key={c.id}
              onClick={() => setActiveId(c.id)}
              className={`w-full flex items-center gap-3 p-3 text-left border-b border-base-700 hover:bg-base-800 transition-colors ${activeId === c.id ? "bg-base-800" : ""}`}
            >
              <div className="w-10 h-10 rounded-full bg-base-700 overflow-hidden shrink-0">
                {c.otherUserProfilePhotoPath && <img src={fileUrl(c.otherUserProfilePhotoPath)} className="w-full h-full object-cover" alt="" />}
              </div>
              <div className="flex-1 min-w-0">
                <div className="flex items-center justify-between">
                  <span className="font-medium text-sm truncate">{c.otherUsername}</span>
                  {c.unreadCount > 0 && (
                    <span className="bg-amber-500 text-base-950 text-[10px] font-bold rounded-full w-4 h-4 flex items-center justify-center shrink-0 ml-1">
                      {c.unreadCount > 9 ? "9+" : c.unreadCount}
                    </span>
                  )}
                </div>
                <p className="text-xs text-ink-500 truncate">{c.advertTitle}</p>
                {c.lastMessage && <p className="text-xs text-ink-500/70 truncate">{c.lastMessage}</p>}
              </div>
            </button>
          ))}
        </div>

        {/* Thread */}
        <div className="flex flex-col">
          {active ? (
            <>
              <div className="border-b border-base-700 p-3 flex items-center gap-3 shrink-0">
                <div className="w-9 h-9 rounded-full bg-base-700 overflow-hidden shrink-0">
                  {active.otherUserProfilePhotoPath && <img src={fileUrl(active.otherUserProfilePhotoPath)} className="w-full h-full object-cover" alt="" />}
                </div>
                <div>
                  <p className="font-medium text-sm">{active.otherUsername}</p>
                  <p className="text-xs text-ink-500 truncate">{active.advertTitle}</p>
                </div>
              </div>

              <div className="flex-1 overflow-y-auto p-4 space-y-3">
                {messages.map((m) => (
                  <div key={m.id} className={`flex ${m.senderId === user.id ? "justify-end" : "justify-start"}`}>
                    <div className={`max-w-xs lg:max-w-sm px-3 py-2 rounded-xl text-sm break-words ${m.senderId === user.id ? "bg-amber-500 text-base-950" : "bg-base-800 text-ink-100"}`}>
                      {m.content}
                    </div>
                  </div>
                ))}
                <div ref={bottomRef} />
              </div>

              <div className="border-t border-base-700 p-3 flex items-center gap-2 shrink-0">
                <input
                  value={text}
                  onChange={(e) => setText(e.target.value)}
                  onKeyDown={(e) => { if (e.key === "Enter" && !e.shiftKey) { e.preventDefault(); sendMessage(); } }}
                  placeholder="Type a message..."
                  disabled={sending}
                  className="flex-1 bg-base-800 border border-base-700 rounded-lg px-3 py-2 text-sm focus-ring disabled:opacity-60"
                />
                <button onClick={sendMessage} disabled={sending || !text.trim()} className="bg-amber-500 hover:bg-amber-400 disabled:opacity-40 text-base-950 rounded-lg p-2.5 transition-colors">
                  <Send size={16} />
                </button>
              </div>
            </>
          ) : (
            <div className="flex-1 flex flex-col items-center justify-center gap-3 text-ink-500">
              <MessageSquare size={36} className="opacity-30" />
              <p className="text-sm">Select a conversation</p>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
