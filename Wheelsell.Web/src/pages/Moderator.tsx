import { useEffect, useState } from "react";
import { Plus, Trash2, ShieldAlert } from "lucide-react";
import { api } from "../api/client";
import type { Brand, CarModel, Currency, FeatureCategory, BannedUserDto, BannedAdvertDto } from "../types";

type Tab = "brands" | "models" | "currencies" | "features" | "banned";

export default function Moderator() {
  const [tab, setTab] = useState<Tab>("brands");
  const [brands, setBrands] = useState<Brand[]>([]);
  const [models, setModels] = useState<CarModel[]>([]);
  const [currencies, setCurrencies] = useState<Currency[]>([]);
  const [categories, setCategories] = useState<FeatureCategory[]>([]);
  const [bannedUsers, setBannedUsers] = useState<BannedUserDto[]>([]);
  const [bannedAdverts, setBannedAdverts] = useState<BannedAdvertDto[]>([]);

  const [newBrand, setNewBrand] = useState("");
  const [newModelBrandId, setNewModelBrandId] = useState("");
  const [newModelName, setNewModelName] = useState("");
  const [newCurrency, setNewCurrency] = useState({ code: "", symbol: "", name: "" });
  const [newCategoryName, setNewCategoryName] = useState("");
  const [newFeature, setNewFeature] = useState({ categoryId: "", name: "" });
  const [error, setError] = useState("");

  const loadAll = () => {
    api.get<Brand[]>("/lookups/brands").then((r) => setBrands(r.data));
    api.get<CarModel[]>("/lookups/models").then((r) => setModels(r.data));
    api.get<Currency[]>("/lookups/currencies").then((r) => setCurrencies(r.data));
    api.get<FeatureCategory[]>("/lookups/feature-categories").then((r) => setCategories(r.data));
    api.get<BannedUserDto[]>("/admin/banned-users").then((r) => setBannedUsers(r.data));
    api.get<BannedAdvertDto[]>("/admin/banned-adverts").then((r) => setBannedAdverts(r.data));
  };

  useEffect(() => {
    loadAll();
  }, []);

  const runAction = async (action: () => Promise<void>) => {
    setError("");
    try {
      await action();
    } catch (err: any) {
      setError(err.response?.data?.error ?? "Something went wrong. Please try again.");
    }
  };

  const addBrand = () => runAction(async () => {
    if (!newBrand.trim()) return;
    await api.post("/lookups/brands", { name: newBrand });
    setNewBrand("");
    loadAll();
  });
  const deleteBrand = (id: number) => runAction(async () => {
    await api.delete(`/lookups/brands/${id}`);
    loadAll();
  });

  const addModel = () => runAction(async () => {
    if (!newModelBrandId || !newModelName.trim()) return;
    await api.post("/lookups/models", { brandId: Number(newModelBrandId), name: newModelName });
    setNewModelName("");
    loadAll();
  });
  const deleteModel = (id: number) => runAction(async () => {
    await api.delete(`/lookups/models/${id}`);
    loadAll();
  });

  const addCurrency = () => runAction(async () => {
    if (!newCurrency.code.trim()) return;
    await api.post("/lookups/currencies", newCurrency);
    setNewCurrency({ code: "", symbol: "", name: "" });
    loadAll();
  });
  const deleteCurrency = (id: number) => runAction(async () => {
    await api.delete(`/lookups/currencies/${id}`);
    loadAll();
  });

  const addCategory = () => runAction(async () => {
    if (!newCategoryName.trim()) return;
    await api.post("/lookups/feature-categories", { name: newCategoryName, order: categories.length });
    setNewCategoryName("");
    loadAll();
  });

  const addFeature = () => runAction(async () => {
    if (!newFeature.categoryId || !newFeature.name.trim()) return;
    await api.post("/lookups/features", { featureCategoryId: Number(newFeature.categoryId), name: newFeature.name });
    setNewFeature({ ...newFeature, name: "" });
    loadAll();
  });
  const deleteFeature = (id: number) => runAction(async () => {
    await api.delete(`/lookups/features/${id}`);
    loadAll();
  });

  const unbanUser = (id: number) => runAction(async () => {
    await api.post(`/admin/users/${id}/unban`);
    loadAll();
  });
  const unbanAdvert = (id: number) => runAction(async () => {
    await api.post(`/admin/adverts/${id}/unban`);
    loadAll();
  });

  return (
    <div className="max-w-5xl mx-auto px-4 sm:px-6 py-8">
      <h1 className="font-display font-bold text-2xl mb-6 flex items-center gap-2">
        <ShieldAlert className="text-amber-400" /> Moderation
      </h1>

      {error && <div className="bg-red-500/10 border border-red-500/30 text-red-400 text-sm rounded-lg px-3 py-2 mb-4">{error}</div>}

      <div className="flex gap-1 border-b border-base-700 mb-6 overflow-x-auto">
        {(["brands", "models", "currencies", "features", "banned"] as Tab[]).map((t) => (
          <button
            key={t}
            onClick={() => setTab(t)}
            className={`px-4 py-2.5 text-sm font-medium border-b-2 whitespace-nowrap transition-colors ${
              tab === t ? "border-amber-500 text-amber-400" : "border-transparent text-ink-500 hover:text-ink-300"
            }`}
          >
            {t === "banned" ? "Banned items" : t.charAt(0).toUpperCase() + t.slice(1)}
          </button>
        ))}
      </div>

      {tab === "brands" && (
        <div className="bg-base-900 border border-base-700 rounded-xl p-5">
          <div className="flex gap-2 mb-4">
            <input value={newBrand} onChange={(e) => setNewBrand(e.target.value)} placeholder="New brand name" className="input flex-1" />
            <button onClick={addBrand} className="bg-amber-500 hover:bg-amber-400 text-base-950 px-3 py-2 rounded-lg"><Plus size={16} /></button>
          </div>
          <div className="space-y-1">
            {brands.map((b) => (
              <div key={b.id} className="flex items-center justify-between py-2 border-b border-base-700 last:border-0">
                <span className="text-sm">{b.name}</span>
                <button onClick={() => deleteBrand(b.id)} className="text-red-400 hover:text-red-300"><Trash2 size={14} /></button>
              </div>
            ))}
          </div>
        </div>
      )}

      {tab === "models" && (
        <div className="bg-base-900 border border-base-700 rounded-xl p-5">
          <div className="flex gap-2 mb-4">
            <select value={newModelBrandId} onChange={(e) => setNewModelBrandId(e.target.value)} className="input flex-1">
              <option value="">Select brand</option>
              {brands.map((b) => <option key={b.id} value={b.id}>{b.name}</option>)}
            </select>
            <input value={newModelName} onChange={(e) => setNewModelName(e.target.value)} placeholder="New model name" className="input flex-1" />
            <button onClick={addModel} className="bg-amber-500 hover:bg-amber-400 text-base-950 px-3 py-2 rounded-lg"><Plus size={16} /></button>
          </div>
          <div className="space-y-1">
            {models.map((m) => (
              <div key={m.id} className="flex items-center justify-between py-2 border-b border-base-700 last:border-0">
                <span className="text-sm">{brands.find((b) => b.id === m.brandId)?.name} — {m.name}</span>
                <button onClick={() => deleteModel(m.id)} className="text-red-400 hover:text-red-300"><Trash2 size={14} /></button>
              </div>
            ))}
          </div>
        </div>
      )}

      {tab === "currencies" && (
        <div className="bg-base-900 border border-base-700 rounded-xl p-5">
          <div className="flex gap-2 mb-4">
            <input value={newCurrency.code} onChange={(e) => setNewCurrency({ ...newCurrency, code: e.target.value })} placeholder="Code (USD)" className="input w-24" />
            <input value={newCurrency.symbol} onChange={(e) => setNewCurrency({ ...newCurrency, symbol: e.target.value })} placeholder="Symbol ($)" className="input w-24" />
            <input value={newCurrency.name} onChange={(e) => setNewCurrency({ ...newCurrency, name: e.target.value })} placeholder="Name" className="input flex-1" />
            <button onClick={addCurrency} className="bg-amber-500 hover:bg-amber-400 text-base-950 px-3 py-2 rounded-lg"><Plus size={16} /></button>
          </div>
          <div className="space-y-1">
            {currencies.map((c) => (
              <div key={c.id} className="flex items-center justify-between py-2 border-b border-base-700 last:border-0">
                <span className="text-sm">{c.code} ({c.symbol}) — {c.name}</span>
                <button onClick={() => deleteCurrency(c.id)} className="text-red-400 hover:text-red-300"><Trash2 size={14} /></button>
              </div>
            ))}
          </div>
        </div>
      )}

      {tab === "features" && (
        <div className="space-y-5">
          <div className="bg-base-900 border border-base-700 rounded-xl p-5">
            <p className="text-sm font-medium mb-2">Add category</p>
            <div className="flex gap-2">
              <input value={newCategoryName} onChange={(e) => setNewCategoryName(e.target.value)} placeholder="Category name" className="input flex-1" />
              <button onClick={addCategory} className="bg-amber-500 hover:bg-amber-400 text-base-950 px-3 py-2 rounded-lg"><Plus size={16} /></button>
            </div>
          </div>

          <div className="bg-base-900 border border-base-700 rounded-xl p-5">
            <p className="text-sm font-medium mb-2">Add feature</p>
            <div className="flex gap-2 mb-4">
              <select value={newFeature.categoryId} onChange={(e) => setNewFeature({ ...newFeature, categoryId: e.target.value })} className="input flex-1">
                <option value="">Select category</option>
                {categories.map((c) => <option key={c.id} value={c.id}>{c.name}</option>)}
              </select>
              <input value={newFeature.name} onChange={(e) => setNewFeature({ ...newFeature, name: e.target.value })} placeholder="Feature name" className="input flex-1" />
              <button onClick={addFeature} className="bg-amber-500 hover:bg-amber-400 text-base-950 px-3 py-2 rounded-lg"><Plus size={16} /></button>
            </div>
            {categories.map((cat) => (
              <div key={cat.id} className="mb-3">
                <p className="text-xs font-semibold text-ink-500 uppercase mb-1.5">{cat.name}</p>
                <div className="flex flex-wrap gap-1.5">
                  {cat.features.map((f) => (
                    <span key={f.id} className="flex items-center gap-1.5 text-xs bg-base-800 border border-base-700 px-2 py-1 rounded-full">
                      {f.name}
                      <button onClick={() => deleteFeature(f.id)} className="text-red-400 hover:text-red-300"><Trash2 size={11} /></button>
                    </span>
                  ))}
                </div>
              </div>
            ))}
          </div>
        </div>
      )}

      {tab === "banned" && (
        <div className="space-y-6">
          <div className="bg-base-900 border border-base-700 rounded-xl p-5">
            <h3 className="font-semibold mb-3">Banned users</h3>
            {bannedUsers.map((u) => (
              <div key={u.id} className="flex items-center justify-between py-2 border-b border-base-700 last:border-0">
                <div>
                  <p className="text-sm font-medium">{u.username}</p>
                  <p className="text-xs text-ink-500">{u.banReason}</p>
                </div>
                <button onClick={() => unbanUser(u.id)} className="text-amber-400 hover:text-amber-300 text-xs font-medium">Unban</button>
              </div>
            ))}
            {bannedUsers.length === 0 && <p className="text-sm text-ink-500">No banned users.</p>}
          </div>

          <div className="bg-base-900 border border-base-700 rounded-xl p-5">
            <h3 className="font-semibold mb-3">Banned adverts</h3>
            {bannedAdverts.map((a) => (
              <div key={a.id} className="flex items-center justify-between py-2 border-b border-base-700 last:border-0">
                <div>
                  <p className="text-sm font-medium">{a.title}</p>
                  <p className="text-xs text-ink-500">by {a.sellerUsername} — {a.banReason}</p>
                </div>
                <button onClick={() => unbanAdvert(a.id)} className="text-amber-400 hover:text-amber-300 text-xs font-medium">Unban</button>
              </div>
            ))}
            {bannedAdverts.length === 0 && <p className="text-sm text-ink-500">No banned adverts.</p>}
          </div>
        </div>
      )}
    </div>
  );
}