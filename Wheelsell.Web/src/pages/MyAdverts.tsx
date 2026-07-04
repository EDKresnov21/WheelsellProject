import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { Plus } from "lucide-react";
import { api } from "../api/client";
import { useAuthStore } from "../store/authStore";
import AdvertCard from "../components/AdvertCard";
import type { AdvertSummary, PagedResult } from "../types";

type StatusFilter = "all" | "Active" | "Sold" | "OffSale";

export default function MyAdverts() {
  const user = useAuthStore((s) => s.user);
  const [adverts, setAdverts] = useState<AdvertSummary[]>([]);
  const [filter, setFilter] = useState<StatusFilter>("all");

  useEffect(() => {
    if (!user) return;
    const params: Record<string, any> = { page: 1, pageSize: 100 };
    if (filter !== "all") params.status = filter;
    api.get<PagedResult<AdvertSummary>>(`/adverts/user/${user.id}`, { params }).then((r) => setAdverts(r.data.items));
  }, [user?.id, filter]);

  if (!user) return null;

  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 py-8">
      <div className="flex items-center justify-between mb-6">
        <h1 className="font-display font-bold text-2xl">My adverts</h1>
        <Link to="/create-advert" className="flex items-center gap-2 bg-amber-500 hover:bg-amber-400 text-base-950 font-semibold px-4 py-2.5 rounded-lg text-sm transition-colors">
          <Plus size={16} /> Sell a car
        </Link>
      </div>

      <div className="flex gap-2 mb-6">
        {(["all", "Active", "OffSale", "Sold"] as StatusFilter[]).map((f) => (
          <button
            key={f}
            onClick={() => setFilter(f)}
            className={`text-sm px-3 py-1.5 rounded-lg border transition-colors ${
              filter === f ? "bg-amber-500 text-base-950 border-amber-500 font-medium" : "bg-base-800 border-base-700 text-ink-300"
            }`}
          >
            {f === "all" ? "All" : f === "OffSale" ? "Off sale" : f}
          </button>
        ))}
      </div>

      <div className="grid sm:grid-cols-2 lg:grid-cols-3 gap-5">
        {adverts.map((a) => (
          <AdvertCard key={a.id} advert={a} showFavorite={false} />
        ))}
      </div>
      {adverts.length === 0 && <p className="text-ink-500 text-sm">No adverts in this category.</p>}
    </div>
  );
}
