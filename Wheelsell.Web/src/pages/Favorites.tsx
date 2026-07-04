import { useEffect, useState } from "react";
import { api } from "../api/client";
import AdvertCard from "../components/AdvertCard";
import type { AdvertSummary } from "../types";

export default function Favorites() {
  const [favorites, setFavorites] = useState<AdvertSummary[]>([]);

  const load = () => {
    api.get<AdvertSummary[]>("/favorites").then((r) => setFavorites(r.data));
  };

  useEffect(() => {
    load();
  }, []);

  const toggleFavorite = async (id: number) => {
    await api.post(`/favorites/${id}/toggle`);
    setFavorites((prev) => prev.filter((a) => a.id !== id));
  };

  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 py-8">
      <h1 className="font-display font-bold text-2xl mb-6">Favorites</h1>
      <div className="grid sm:grid-cols-2 lg:grid-cols-3 gap-5">
        {favorites.map((a) => (
          <AdvertCard key={a.id} advert={a} onToggleFavorite={toggleFavorite} />
        ))}
      </div>
      {favorites.length === 0 && <p className="text-ink-500 text-sm">You haven't favorited any adverts yet.</p>}
    </div>
  );
}
