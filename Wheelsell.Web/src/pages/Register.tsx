import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { Car, CheckCircle2 } from "lucide-react";
import { api } from "../api/client";

export default function Register() {
  const navigate = useNavigate();
  const [form, setForm] = useState({
    username: "",
    email: "",
    password: "",
    name: "",
    surname: "",
    phone: "",
    city: "",
    county: ""
  });
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);
  const [done, setDone] = useState(false);

  const update = (key: keyof typeof form) => (e: React.ChangeEvent<HTMLInputElement>) =>
    setForm((f) => ({ ...f, [key]: e.target.value }));

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError("");
    setLoading(true);
    try {
      await api.post("/auth/register", form);
      setDone(true);
    } catch (err: any) {
      setError(err.response?.data?.error ?? "Registration failed.");
    } finally {
      setLoading(false);
    }
  };

  if (done) {
    return (
      <div className="min-h-[80vh] flex items-center justify-center px-4">
        <div className="w-full max-w-sm text-center bg-base-900 border border-base-700 rounded-xl p-8">
          <CheckCircle2 className="text-amber-400 mx-auto mb-4" size={40} />
          <h1 className="font-display font-bold text-xl mb-2">Account created!</h1>
          <p className="text-sm text-ink-500 mb-6">
            Your account is ready. You can log in now.
          </p>
          <button onClick={() => navigate("/login")} className="bg-amber-500 hover:bg-amber-400 text-base-950 font-semibold px-5 py-2.5 rounded-lg transition-colors">
            Go to login
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-[80vh] flex items-center justify-center px-4 py-12">
      <div className="w-full max-w-lg">
        <div className="flex items-center justify-center gap-2 mb-8">
          <Car className="text-amber-500" size={28} />
          <span className="font-display font-extrabold text-2xl">
            Wheel<span className="text-amber-500">Sell</span>
          </span>
        </div>

        <div className="bg-base-900 border border-base-700 rounded-xl p-6">
          <h1 className="font-display font-bold text-xl mb-1">Create your account</h1>
          <p className="text-sm text-ink-500 mb-6">Join WheelSell to buy and sell cars.</p>

          {error && <div className="bg-red-500/10 border border-red-500/30 text-red-400 text-sm rounded-lg px-3 py-2 mb-4">{error}</div>}

          <form onSubmit={handleSubmit} className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <Field label="Username" value={form.username} onChange={update("username")} />
            <Field label="Email" type="email" value={form.email} onChange={update("email")} />
            <Field label="Password" type="password" value={form.password} onChange={update("password")} className="sm:col-span-2" />
            <Field label="First name" value={form.name} onChange={update("name")} />
            <Field label="Last name" value={form.surname} onChange={update("surname")} />
            <Field label="Phone (optional)" value={form.phone} onChange={update("phone")} required={false} />
            <Field label="City" value={form.city} onChange={update("city")} />
            <Field label="County / Region" value={form.county} onChange={update("county")} className="sm:col-span-2" />

            <button
              type="submit"
              disabled={loading}
              className="sm:col-span-2 bg-amber-500 hover:bg-amber-400 disabled:opacity-60 text-base-950 font-semibold py-2.5 rounded-lg transition-colors mt-2"
            >
              {loading ? "Creating account..." : "Create account"}
            </button>
          </form>
        </div>

        <p className="text-center text-sm text-ink-500 mt-5">
          Already have an account?{" "}
          <Link to="/login" className="text-amber-400 hover:text-amber-300 font-medium">
            Log in
          </Link>
        </p>
      </div>
    </div>
  );
}

function Field({
  label, value, onChange, type = "text", required = true, className = ""
}: {
  label: string; value: string; onChange: (e: React.ChangeEvent<HTMLInputElement>) => void;
  type?: string; required?: boolean; className?: string;
}) {
  return (
    <div className={className}>
      <label className="text-sm font-medium text-ink-300 block mb-1.5">{label}</label>
      <input
        type={type}
        value={value}
        onChange={onChange}
        required={required}
        className="w-full bg-base-800 border border-base-700 rounded-lg px-3 py-2.5 text-sm focus-ring"
      />
    </div>
  );
}
