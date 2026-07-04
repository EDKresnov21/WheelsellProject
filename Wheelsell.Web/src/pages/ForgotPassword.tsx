import { useState } from "react";
import { Link } from "react-router-dom";
import { Mail, CheckCircle2 } from "lucide-react";
import { api } from "../api/client";

export default function ForgotPassword() {
  const [email, setEmail] = useState("");
  const [done, setDone] = useState(false);
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    try {
      await api.post("/auth/forgot-password", { email });
    } finally {
      setLoading(false);
      setDone(true);
    }
  };

  return (
    <div className="min-h-[80vh] flex items-center justify-center px-4">
      <div className="w-full max-w-sm bg-base-900 border border-base-700 rounded-xl p-6 text-center">
        {done ? (
          <>
            <CheckCircle2 className="text-amber-400 mx-auto mb-4" size={40} />
            <h1 className="font-display font-bold text-xl mb-2">Check your inbox</h1>
            <p className="text-sm text-ink-500 mb-6">If an account exists for {email}, we've sent a password reset link.</p>
          </>
        ) : (
          <>
            <Mail className="text-amber-400 mx-auto mb-4" size={32} />
            <h1 className="font-display font-bold text-xl mb-1">Reset your password</h1>
            <p className="text-sm text-ink-500 mb-6">Enter your email and we'll send you a reset link.</p>
            <form onSubmit={handleSubmit} className="flex flex-col gap-4 text-left">
              <input
                type="email"
                required
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                placeholder="you@example.com"
                className="w-full bg-base-800 border border-base-700 rounded-lg px-3 py-2.5 text-sm focus-ring"
              />
              <button
                type="submit"
                disabled={loading}
                className="bg-amber-500 hover:bg-amber-400 disabled:opacity-60 text-base-950 font-semibold py-2.5 rounded-lg transition-colors"
              >
                {loading ? "Sending..." : "Send reset link"}
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
