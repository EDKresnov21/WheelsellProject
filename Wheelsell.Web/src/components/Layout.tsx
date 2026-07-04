import { Link, NavLink, Outlet, useNavigate } from "react-router-dom";
import { useEffect, useState } from "react";
import { Car, Heart, MessageSquare, Bell, User, LogOut, Menu, X, ShieldCheck } from "lucide-react";
import { useAuthStore } from "../store/authStore";
import { api, fileUrl } from "../api/client";

function NavItem({ to, children }: { to: string; children: React.ReactNode }) {
  return (
    <NavLink
      to={to}
      className={({ isActive }) =>
        `text-sm font-medium transition-colors hover:text-amber-400 ${
          isActive ? "text-amber-400" : "text-ink-300"
        }`
      }
    >
      {children}
    </NavLink>
  );
}

export default function Layout() {
  const { user, clear, refreshToken } = useAuthStore();
  const navigate = useNavigate();
  const [unread, setUnread] = useState(0);
  const [menuOpen, setMenuOpen] = useState(false);

  useEffect(() => {
    if (!user) return;
    api.get<number>("/notifications/unread-count").then((r) => setUnread(r.data)).catch(() => {});
    const interval = setInterval(() => {
      api.get<number>("/notifications/unread-count").then((r) => setUnread(r.data)).catch(() => {});
    }, 30000);
    return () => clearInterval(interval);
  }, [user]);

  const handleLogout = async () => {
    try {
      await api.post("/auth/logout", { refreshToken });
    } catch {
      // ignore
    }
    clear();
    navigate("/");
  };

  return (
    <div className="min-h-screen flex flex-col bg-base-950 text-ink-100">
      <header className="border-b border-base-700 bg-base-900/80 backdrop-blur sticky top-0 z-30">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 h-16 flex items-center justify-between">
          <Link to="/" className="flex items-center gap-2 font-display font-extrabold text-xl tracking-tight">
            <Car className="text-amber-500" size={24} strokeWidth={2.4} />
            <span>
              Wheel<span className="text-amber-500">Sell</span>
            </span>
          </Link>

          <nav className="hidden md:flex items-center gap-7">
            <NavItem to="/search">Browse</NavItem>
            {user && <NavItem to="/my-adverts">My Adverts</NavItem>}
            {user && <NavItem to="/create-advert">Sell a Car</NavItem>}
            {(user?.role === "Moderator" || user?.role === "Admin") && <NavItem to="/moderator">Moderation</NavItem>}
            {user?.role === "Admin" && <NavItem to="/admin">Admin</NavItem>}
          </nav>

          <div className="hidden md:flex items-center gap-5">
            {user ? (
              <>
                <Link to="/favorites" className="text-ink-300 hover:text-amber-400 transition-colors" title="Favorites">
                  <Heart size={20} />
                </Link>
                <Link to="/chat" className="text-ink-300 hover:text-amber-400 transition-colors" title="Messages">
                  <MessageSquare size={20} />
                </Link>
                <Link to="/notifications" className="relative text-ink-300 hover:text-amber-400 transition-colors" title="Notifications">
                  <Bell size={20} />
                  {unread > 0 && (
                    <span className="absolute -top-1.5 -right-1.5 bg-amber-500 text-base-950 text-[10px] font-bold rounded-full w-4 h-4 flex items-center justify-center">
                      {unread > 9 ? "9+" : unread}
                    </span>
                  )}
                </Link>
                <Link to="/profile" className="flex items-center gap-2 group">
                  {user.profilePhotoPath ? (
                    <img src={fileUrl(user.profilePhotoPath)} alt="" className="w-8 h-8 rounded-full object-cover border border-base-600" />
                  ) : (
                    <div className="w-8 h-8 rounded-full bg-base-700 flex items-center justify-center">
                      <User size={16} className="text-ink-300" />
                    </div>
                  )}
                  <span className="text-sm font-medium text-ink-300 group-hover:text-amber-400 transition-colors">{user.username}</span>
                </Link>
                <button onClick={handleLogout} className="text-ink-500 hover:text-amber-400 transition-colors" title="Log out">
                  <LogOut size={19} />
                </button>
              </>
            ) : (
              <>
                <Link to="/login" className="text-sm font-medium text-ink-300 hover:text-amber-400 transition-colors">
                  Log in
                </Link>
                <Link
                  to="/register"
                  className="text-sm font-semibold bg-amber-500 hover:bg-amber-400 text-base-950 px-4 py-2 rounded-lg transition-colors"
                >
                  Sign up
                </Link>
              </>
            )}
          </div>

          <button className="md:hidden text-ink-300" onClick={() => setMenuOpen((v) => !v)}>
            {menuOpen ? <X size={24} /> : <Menu size={24} />}
          </button>
        </div>

        {menuOpen && (
          <div className="md:hidden border-t border-base-700 px-4 py-4 flex flex-col gap-4">
            <NavItem to="/search">Browse</NavItem>
            {user && <NavItem to="/my-adverts">My Adverts</NavItem>}
            {user && <NavItem to="/create-advert">Sell a Car</NavItem>}
            {user && <NavItem to="/favorites">Favorites</NavItem>}
            {user && <NavItem to="/chat">Messages</NavItem>}
            {user && <NavItem to="/notifications">Notifications</NavItem>}
            {user && <NavItem to="/profile">Profile</NavItem>}
            {(user?.role === "Moderator" || user?.role === "Admin") && <NavItem to="/moderator">Moderation</NavItem>}
            {user?.role === "Admin" && <NavItem to="/admin">Admin</NavItem>}
            {user ? (
              <button onClick={handleLogout} className="text-left text-sm font-medium text-ink-300 flex items-center gap-2">
                <LogOut size={16} /> Log out
              </button>
            ) : (
              <>
                <NavItem to="/login">Log in</NavItem>
                <NavItem to="/register">Sign up</NavItem>
              </>
            )}
          </div>
        )}
      </header>

      <main className="flex-1">
        <Outlet />
      </main>

      <footer className="border-t border-base-700 mt-auto">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 py-8 flex flex-col sm:flex-row items-center justify-between gap-3 text-sm text-ink-500">
          <div className="flex items-center gap-2 font-display font-bold text-ink-300">
            <Car size={16} className="text-amber-500" />
            WheelSell
          </div>
          <p>Buy and sell cars with confidence.</p>
          <div className="flex items-center gap-1.5">
            <ShieldCheck size={14} />
            Moderated marketplace
          </div>
        </div>
      </footer>
    </div>
  );
}
