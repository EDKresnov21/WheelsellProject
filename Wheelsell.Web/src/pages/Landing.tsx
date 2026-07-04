import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { Search, ShieldCheck, MessageSquare, History } from "lucide-react";
import { api } from "../api/client";
import { useAuthStore } from "../store/authStore";
import AdvertCard from "../components/AdvertCard";
import type { AdvertSummary, PagedResult, Brand } from "../types";

export default function Landing() {
  const navigate = useNavigate();
  const user = useAuthStore((s) => s.user);
  const [recent, setRecent] = useState<AdvertSummary[]>([]);
  const [brands, setBrands] = useState<Brand[]>([]);
  const [query, setQuery] = useState("");
  const [brandId, setBrandId] = useState("");

  useEffect(() => {
    api
      .get<PagedResult<AdvertSummary>>("/adverts", { params: { page: 1, pageSize: 8, sortBy: "createdAt", sortDescending: true } })
      .then((r) => setRecent(r.data.items))
      .catch(() => {});
    api.get<Brand[]>("/lookups/brands").then((r) => setBrands(r.data)).catch(() => {});
  }, []);

  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault();
    const params = new URLSearchParams();
    if (query) params.set("query", query);
    if (brandId) params.set("brandId", brandId);
    navigate(`/search?${params.toString()}`);
  };

  const toggleFavorite = async (id: number) => {
    if (!user) return navigate("/login");
    await api.post(`/favorites/${id}/toggle`);
    setRecent((prev) => prev.map((a) => (a.id === id ? { ...a, isFavorite: !a.isFavorite } : a)));
  };

  return (
    <div>
      <section className="relative overflow-hidden border-b border-base-700">
        <div
          className="absolute inset-0 opacity-40"
          style={{
            background:
              "radial-gradient(ellipse 80% 60% at 50% -10%, rgba(232,162,39,0.18), transparent), radial-gradient(ellipse 60% 50% at 90% 100%, rgba(232,162,39,0.08), transparent)"
          }}
        />
        <div className="relative max-w-5xl mx-auto px-4 sm:px-6 py-20 sm:py-28 text-center">
          <p className="text-amber-400 font-semibold text-sm tracking-widest uppercase mb-4">Buy. Sell. Drive.</p>
          <h1 className="font-display font-extrabold text-4xl sm:text-6xl tracking-tight leading-tight">
            Every car has a <span className="text-amber-400">story</span>.<br className="hidden sm:block" /> Find the next chapter of yours.
          </h1>
          <p className="text-ink-300 mt-5 max-w-xl mx-auto">
            WheelSell connects buyers and sellers with full transparency — complete price and mileage history follows every car from owner to owner.
          </p>

          <form onSubmit={handleSearch} className="mt-9 max-w-2xl mx-auto bg-base-900 border border-base-700 rounded-xl p-2 flex flex-col sm:flex-row gap-2">
            <select
              value={brandId}
              onChange={(e) => setBrandId(e.target.value)}
              className="bg-base-800 border border-base-700 rounded-lg px-3 py-2.5 text-sm text-ink-100 focus-ring sm:w-44"
            >
              <option value="">All brands</option>
              {brands.map((b) => (
                <option key={b.id} value={b.id}>
                  {b.name}
                </option>
              ))}
            </select>
            <input
              value={query}
              onChange={(e) => setQuery(e.target.value)}
              placeholder="Search by model, trim, keyword..."
              className="flex-1 bg-base-800 border border-base-700 rounded-lg px-3 py-2.5 text-sm focus-ring"
            />
            <button type="submit" className="bg-amber-500 hover:bg-amber-400 text-base-950 font-semibold px-5 py-2.5 rounded-lg flex items-center justify-center gap-2 transition-colors">
              <Search size={16} /> Search
            </button>
          </form>
        </div>
      </section>

      <section className="max-w-7xl mx-auto px-4 sm:px-6 py-12">
        <div className="flex items-end justify-between mb-6">
          <h2 className="font-display font-bold text-2xl">Recently listed</h2>
          <a href="/search" className="text-sm text-amber-400 hover:text-amber-300 font-medium">
            View all
          </a>
        </div>
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-5">
          {recent.map((a) => (
            <AdvertCard key={a.id} advert={a} onToggleFavorite={toggleFavorite} />
          ))}
        </div>
        {recent.length === 0 && <p className="text-ink-500 text-sm">No adverts yet — be the first to list a car.</p>}
      </section>

      <section className="border-t border-base-700 bg-base-900/40">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 py-16 grid sm:grid-cols-3 gap-8">
          <div>
            <History className="text-amber-400 mb-3" size={26} />
            <h3 className="font-display font-semibold text-lg mb-1">Full ownership history</h3>
            <p className="text-sm text-ink-500">Every relisted car carries its price and mileage history forward, so buyers always see the full picture.</p>
          </div>
          <div>
            <MessageSquare className="text-amber-400 mb-3" size={26} />
            <h3 className="font-display font-semibold text-lg mb-1">Direct messaging</h3>
            <p className="text-sm text-ink-500">Chat with sellers in real time, right from the advert — no phone calls required until you're ready.</p>
          </div>
          <div>
            <ShieldCheck className="text-amber-400 mb-3" size={26} />
            <h3 className="font-display font-semibold text-lg mb-1">Moderated listings</h3>
            <p className="text-sm text-ink-500">Our team keeps the marketplace clean, removing fraudulent or misleading adverts.</p>
          </div>
        </div>
      </section>
    </div>
  );
}
