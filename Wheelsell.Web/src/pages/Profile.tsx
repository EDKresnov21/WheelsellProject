import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { Camera, Star } from "lucide-react";
import { api, fileUrl } from "../api/client";
import { useAuthStore } from "../store/authStore";
import { RatingStars } from "../components/RatingStars";
import AdvertCard from "../components/AdvertCard";
import type { AdvertSummary, PagedResult, PurchaseHistoryItem, ReviewDto, UserProfile } from "../types";

type Tab = "adverts" | "purchases" | "reviews" | "settings";

export default function Profile() {
  const { user, setUser } = useAuthStore();
  const [tab, setTab] = useState<Tab>("adverts");
  const [adverts, setAdverts] = useState<AdvertSummary[]>([]);
  const [purchases, setPurchases] = useState<PurchaseHistoryItem[]>([]);
  const [reviews, setReviews] = useState<ReviewDto[]>([]);
  const [form, setForm] = useState({ name: "", surname: "", phone: "", city: "", county: "" });
  const [saving, setSaving] = useState(false);
  const [saved, setSaved] = useState(false);

  useEffect(() => {
    if (!user) return;
    setForm({ name: user.name, surname: user.surname, phone: user.phone ?? "", city: user.city, county: user.county });
  }, [user?.id]);

  useEffect(() => {
    if (!user) return;
    if (tab === "adverts") {
      api.get<PagedResult<AdvertSummary>>(`/adverts/user/${user.id}`, { params: { page: 1, pageSize: 50 } }).then((r) => setAdverts(r.data.items));
    } else if (tab === "purchases") {
      api.get<PurchaseHistoryItem[]>("/users/me/purchase-history").then((r) => setPurchases(r.data));
    } else if (tab === "reviews") {
      api.get<ReviewDto[]>(`/users/${user.id}/reviews`).then((r) => setReviews(r.data));
    }
  }, [tab, user?.id]);

  if (!user) return null;

  const handlePhotoChange = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;
    const data = new FormData();
    data.append("file", file);
    const res = await api.post<UserProfile>("/users/me/profile-photo", data, { headers: { "Content-Type": "multipart/form-data" } });
    setUser(res.data);
  };

  const saveProfile = async (e: React.FormEvent) => {
    e.preventDefault();
    setSaving(true);
    try {
      const res = await api.put<UserProfile>("/users/me", form);
      setUser(res.data);
      setSaved(true);
      setTimeout(() => setSaved(false), 2500);
    } finally {
      setSaving(false);
    }
  };

  return (
    <div className="max-w-5xl mx-auto px-4 sm:px-6 py-8">
      <div className="flex items-center gap-5 mb-8">
        <div className="relative">
          {user.profilePhotoPath ? (
            <img src={fileUrl(user.profilePhotoPath)} className="w-20 h-20 rounded-full object-cover border border-base-700" />
          ) : (
            <div className="w-20 h-20 rounded-full bg-base-700" />
          )}
          <label className="absolute -bottom-1 -right-1 bg-amber-500 hover:bg-amber-400 text-base-950 rounded-full p-1.5 cursor-pointer transition-colors">
            <Camera size={14} />
            <input type="file" accept="image/*" className="hidden" onChange={handlePhotoChange} />
          </label>
        </div>
        <div>
          <h1 className="font-display font-bold text-2xl">{user.username}</h1>
          <div className="flex items-center gap-2 mt-1">
            <RatingStars rating={user.averageRating} />
            <span className="text-sm text-ink-500">({user.reviewsCount} reviews)</span>
          </div>
        </div>
      </div>

      <div className="flex gap-1 border-b border-base-700 mb-6">
        {[
          ["adverts", "My adverts"],
          ["purchases", "Purchase history"],
          ["reviews", "Reviews"],
          ["settings", "Settings"]
        ].map(([key, label]) => (
          <button
            key={key}
            onClick={() => setTab(key as Tab)}
            className={`px-4 py-2.5 text-sm font-medium border-b-2 transition-colors ${
              tab === key ? "border-amber-500 text-amber-400" : "border-transparent text-ink-500 hover:text-ink-300"
            }`}
          >
            {label}
          </button>
        ))}
      </div>

      {tab === "adverts" && (
        <div className="grid sm:grid-cols-2 lg:grid-cols-3 gap-5">
          {adverts.map((a) => (
            <AdvertCard key={a.id} advert={a} showFavorite={false} />
          ))}
          {adverts.length === 0 && <p className="text-ink-500 text-sm">You haven't listed any cars yet.</p>}
        </div>
      )}

      {tab === "purchases" && (
        <div className="space-y-3">
          {purchases.map((p) => (
            <Link key={p.advertId} to={`/adverts/${p.advertId}`} className="flex items-center gap-4 bg-base-900 border border-base-700 rounded-xl p-4 hover:border-amber-500/50 transition-colors">
              <div className="w-20 h-16 rounded-lg bg-base-800 overflow-hidden shrink-0">
                {p.thumbnailPath && <img src={fileUrl(p.thumbnailPath)} className="w-full h-full object-cover" />}
              </div>
              <div className="flex-1">
                <p className="font-medium">{p.title}</p>
                <p className="text-sm text-ink-500">
                  {p.brandName} {p.modelName} · {p.year} · from {p.sellerUsername}
                </p>
              </div>
              <div className="text-right">
                <p className="font-display font-semibold text-amber-400">{p.price.toLocaleString()} {p.currencyCode}</p>
                <p className="text-xs text-ink-500">{p.soldAt ? new Date(p.soldAt).toLocaleDateString() : ""}</p>
              </div>
            </Link>
          ))}
          {purchases.length === 0 && <p className="text-ink-500 text-sm">No purchases yet.</p>}
        </div>
      )}

      {tab === "reviews" && (
        <div className="space-y-4">
          {reviews.map((r) => (
            <div key={r.id} className="bg-base-900 border border-base-700 rounded-xl p-4">
              <div className="flex items-center justify-between mb-1">
                <span className="font-medium text-sm">{r.reviewerUsername}</span>
                <RatingStars rating={r.rating} />
              </div>
              <p className="text-sm text-ink-500">{r.comment}</p>
            </div>
          ))}
          {reviews.length === 0 && <p className="text-ink-500 text-sm">No reviews yet.</p>}
        </div>
      )}

      {tab === "settings" && (
        <form onSubmit={saveProfile} className="bg-base-900 border border-base-700 rounded-xl p-5 max-w-md space-y-4">
          {saved && <p className="text-sm text-amber-400">Profile updated.</p>}
          <FormField label="First name" value={form.name} onChange={(v) => setForm((f) => ({ ...f, name: v }))} />
          <FormField label="Last name" value={form.surname} onChange={(v) => setForm((f) => ({ ...f, surname: v }))} />
          <FormField label="Phone" value={form.phone} onChange={(v) => setForm((f) => ({ ...f, phone: v }))} />
          <FormField label="City" value={form.city} onChange={(v) => setForm((f) => ({ ...f, city: v }))} />
          <FormField label="County / Region" value={form.county} onChange={(v) => setForm((f) => ({ ...f, county: v }))} />
          <button disabled={saving} className="bg-amber-500 hover:bg-amber-400 disabled:opacity-60 text-base-950 font-semibold px-4 py-2.5 rounded-lg transition-colors">
            {saving ? "Saving..." : "Save changes"}
          </button>
        </form>
      )}
    </div>
  );
}

function FormField({ label, value, onChange }: { label: string; value: string; onChange: (v: string) => void }) {
  return (
    <div>
      <label className="text-sm font-medium text-ink-300 block mb-1.5">{label}</label>
      <input value={value} onChange={(e) => onChange(e.target.value)} className="input" />
    </div>
  );
}
