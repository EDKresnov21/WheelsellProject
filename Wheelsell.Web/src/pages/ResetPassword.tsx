import { useState } from "react";
import { Link, useNavigate, useSearchParams } from "react-router-dom";
import { KeyRound, CheckCircle2 } from "lucide-react";
import { api } from "../api/client";

export default function ResetPassword() {
  const [params] = useSearchParams();
  const navigate = useNavigate();
  const token = params.get("token") ?? "";
  const [newPassword, setNewPassword] = useState("");
  const [error, setError] = useState("");
  const [done, setDone] = useState(false);
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError("");
    setLoading(true);
    try {
      await api.post("/auth/reset-password", { token, newPassword });
      setDone(true);
    } catch (err: any) {
      setError(err.response?.data?.error ?? "Could not reset password. The link may have expired.");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-[80vh] flex items-center justify-center px-4">
      <div className="w-full max-w-sm bg-base-900 border border-base-700 rounded-xl p-6 text-center">
        {done ? (
          <>
            <CheckCircle2 className="text-amber-400 mx-auto mb-4" size={40} />
            <h1 className="font-display font-bold text-xl mb-2">Password updated</h1>
            <p className="text-sm text-ink-500 mb-6">You can now log in with your new password.</p>
            <button onClick={() => navigate("/login")} className="bg-amber-500 hover:bg-amber-400 text-base-950 font-semibold px-5 py-2.5 rounded-lg transition-colors">
              Go to login
            </button>
          </>
        ) : (
          <>
            <KeyRound className="text-amber-400 mx-auto mb-4" size={32} />
            <h1 className="font-display font-bold text-xl mb-1">Choose a new password</h1>
            {error && <div className="bg-red-500/10 border border-red-500/30 text-red-400 text-sm rounded-lg px-3 py-2 my-4">{error}</div>}
            <form onSubmit={handleSubmit} className="flex flex-col gap-4 text-left mt-4">
              <input
                type="password"
                required
                value={newPassword}
                onChange={(e) => setNewPassword(e.target.value)}
                placeholder="New password"
                className="w-full bg-base-800 border border-base-700 rounded-lg px-3 py-2.5 text-sm focus-ring"
              />
              <button
                type="submit"
                disabled={loading}
                className="bg-amber-500 hover:bg-amber-400 disabled:opacity-60 text-base-950 font-semibold py-2.5 rounded-lg transition-colors"
              >
                {loading ? "Updating..." : "Update password"}
              </button>
            </form>
          </>
        )}
        <Link to="/login" className="text-amber-400 hover:text-amber-300 text-sm font-medium mt-5 inline-block">
          Back to login
        </Link>
      </div>
    </div>
  );
}
