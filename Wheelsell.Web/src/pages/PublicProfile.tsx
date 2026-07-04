import { useEffect, useState } from "react";
import { useParams } from "react-router-dom";
import { MapPin } from "lucide-react";
import { api, fileUrl } from "../api/client";
import { RatingStars } from "../components/RatingStars";
import AdvertCard from "../components/AdvertCard";
import type { AdvertSummary, PagedResult, ReviewDto, UserProfile } from "../types";

export default function PublicProfile() {
  const { id } = useParams();
  const [profile, setProfile] = useState<UserProfile | null>(null);
  const [adverts, setAdverts] = useState<AdvertSummary[]>([]);
  const [reviews, setReviews] = useState<ReviewDto[]>([]);

  useEffect(() => {
    api.get<UserProfile>(`/users/${id}`).then((r) => setProfile(r.data));
    api.get<PagedResult<AdvertSummary>>(`/adverts/user/${id}`, { params: { page: 1, pageSize: 50 } }).then((r) => setAdverts(r.data.items));
    api.get<ReviewDto[]>(`/users/${id}/reviews`).then((r) => setReviews(r.data));
  }, [id]);

  if (!profile) return <div className="max-w-3xl mx-auto px-4 py-16 text-center text-ink-500">Loading...</div>;

  return (
    <div className="max-w-5xl mx-auto px-4 sm:px-6 py-8">
      <div className="flex items-center gap-5 mb-8">
        {profile.profilePhotoPath ? (
          <img src={fileUrl(profile.profilePhotoPath)} className="w-20 h-20 rounded-full object-cover border border-base-700" />
        ) : (
          <div className="w-20 h-20 rounded-full bg-base-700" />
        )}
        <div>
          <h1 className="font-display font-bold text-2xl">{profile.username}</h1>
          <p className="text-sm text-ink-500 flex items-center gap-1.5 mt-1">
            <MapPin size={14} /> {profile.city}
          </p>
          <div className="flex items-center gap-2 mt-1">
            <RatingStars rating={profile.averageRating} />
            <span className="text-sm text-ink-500">({profile.reviewsCount} reviews)</span>
          </div>
        </div>
      </div>

      <h2 className="font-display font-semibold text-lg mb-4">Active adverts</h2>
      <div className="grid sm:grid-cols-2 lg:grid-cols-3 gap-5 mb-10">
        {adverts.filter((a) => a.status === "Active").map((a) => (
          <AdvertCard key={a.id} advert={a} showFavorite={false} />
        ))}
        {adverts.filter((a) => a.status === "Active").length === 0 && <p className="text-ink-500 text-sm">No active adverts.</p>}
      </div>

      <h2 className="font-display font-semibold text-lg mb-4">Reviews</h2>
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
    </div>
  );
}
