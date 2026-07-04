import { useEffect, useState } from "react";
import { Link, useSearchParams } from "react-router-dom";
import { CheckCircle2, XCircle, Loader2 } from "lucide-react";
import { api } from "../api/client";

export default function ConfirmEmail() {
  const [params] = useSearchParams();
  const [status, setStatus] = useState<"loading" | "success" | "error">("loading");

  useEffect(() => {
    const token = params.get("token");
    if (!token) {
      setStatus("error");
      return;
    }
    api
      .post("/auth/confirm-email", { token })
      .then(() => setStatus("success"))
      .catch(() => setStatus("error"));
  }, [params]);

  return (
    <div className="min-h-[80vh] flex items-center justify-center px-4">
      <div className="w-full max-w-sm text-center bg-base-900 border border-base-700 rounded-xl p-8">
        {status === "loading" && <Loader2 className="text-amber-400 mx-auto mb-4 animate-spin" size={40} />}
        {status === "success" && <CheckCircle2 className="text-amber-400 mx-auto mb-4" size={40} />}
        {status === "error" && <XCircle className="text-red-400 mx-auto mb-4" size={40} />}

        <h1 className="font-display font-bold text-xl mb-2">
          {status === "loading" && "Confirming your email..."}
          {status === "success" && "Email confirmed"}
          {status === "error" && "Invalid or expired link"}
        </h1>
        <p className="text-sm text-ink-500 mb-6">
          {status === "success" && "Your account is now active. You can log in."}
          {status === "error" && "This confirmation link is no longer valid. Please request a new one or contact support."}
        </p>
        {status !== "loading" && (
          <Link to="/login" className="bg-amber-500 hover:bg-amber-400 text-base-950 font-semibold px-5 py-2.5 rounded-lg transition-colors inline-block">
            Go to login
          </Link>
        )}
      </div>
    </div>
  );
}
