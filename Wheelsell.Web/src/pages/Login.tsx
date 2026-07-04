import { useState } from "react";
import { Link, useLocation, useNavigate } from "react-router-dom";
import { Car } from "lucide-react";
import { api } from "../api/client";
import { useAuthStore } from "../store/authStore";
import type { AuthResponse } from "../types";

export default function Login() {
  const navigate = useNavigate();
  const location = useLocation();
  const setAuth = useAuthStore((s) => s.setAuth);
  const [usernameOrEmail, setUsernameOrEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError("");
    setLoading(true);
    try {
      const res = await api.post<AuthResponse>("/auth/login", { usernameOrEmail, password });
      setAuth(res.data.accessToken, res.data.refreshToken, res.data.user);
      const redirectTo = (location.state as { from?: string } | null)?.from ?? "/";
      navigate(redirectTo);
    } catch (err: any) {
      setError(err.response?.data?.error ?? "Login failed. Please check your credentials.");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-[80vh] flex items-center justify-center px-4">
      <div className="w-full max-w-sm">
        <div className="flex items-center justify-center gap-2 mb-8">
          <Car className="text-amber-500" size={28} />
          <span className="font-display font-extrabold text-2xl">
            Wheel<span className="text-amber-500">Sell</span>
          </span>
        </div>

        <div className="bg-base-900 border border-base-700 rounded-xl p-6">
          <h1 className="font-display font-bold text-xl mb-1">Welcome back</h1>
          <p className="text-sm text-ink-500 mb-6">Log in to manage your adverts and messages.</p>

          {error && <div className="bg-red-500/10 border border-red-500/30 text-red-400 text-sm rounded-lg px-3 py-2 mb-4">{error}</div>}

          <form onSubmit={handleSubmit} className="flex flex-col gap-4">
            <div>
              <label className="text-sm font-medium text-ink-300 block mb-1.5">Username or email</label>
              <input
                value={usernameOrEmail}
                onChange={(e) => setUsernameOrEmail(e.target.value)}
                required
                className="w-full bg-base-800 border border-base-700 rounded-lg px-3 py-2.5 text-sm focus-ring"
              />
            </div>
            <div>
              <div className="flex justify-between items-baseline mb-1.5">
                <label className="text-sm font-medium text-ink-300">Password</label>
                <Link to="/forgot-password" className="text-xs text-amber-400 hover:text-amber-300">
                  Forgot password?
                </Link>
              </div>
              <input
                type="password"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                required
                className="w-full bg-base-800 border border-base-700 rounded-lg px-3 py-2.5 text-sm focus-ring"
              />
            </div>
            <button
              type="submit"
              disabled={loading}
              className="bg-amber-500 hover:bg-amber-400 disabled:opacity-60 text-base-950 font-semibold py-2.5 rounded-lg transition-colors mt-2"
            >
              {loading ? "Logging in..." : "Log in"}
            </button>
          </form>
        </div>

        <p className="text-center text-sm text-ink-500 mt-5">
          Don't have an account?{" "}
          <Link to="/register" className="text-amber-400 hover:text-amber-300 font-medium">
            Sign up
          </Link>
        </p>
      </div>
    </div>
  );
}
