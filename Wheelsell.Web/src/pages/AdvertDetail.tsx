import { useEffect, useState } from "react";
import { Link, useNavigate, useParams } from "react-router-dom";
import {
  Heart, MapPin, Gauge, Calendar, Fuel, Cog, Settings2, Users, Palette, Zap, History,
  MessageSquare, Pencil, Trash2, CheckCircle2, EyeOff, Play, ChevronLeft, ChevronRight, Star, AlertCircle
} from "lucide-react";
import { api, fileUrl } from "../api/client";
import { useAuthStore } from "../store/authStore";
import { RatingStars } from "../components/RatingStars";
import type { AdvertDetail, ConversationDto, ReviewDto } from "../types";
import { FUEL_LABELS, TRANSMISSION_LABELS, BODY_LABELS, DRIVETRAIN_LABELS, CONDITION_LABELS } from "../constants/enums";

export default function AdvertDetailPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const user = useAuthStore((s) => s.user);

  const [advert, setAdvert] = useState<AdvertDetail | null>(null);
  const [activeMedia, setActiveMedia] = useState(0);
  const [activeTab, setActiveTab] = useState<"images" | "videos">("images");
  const [reviews, setReviews] = useState<ReviewDto[]>([]);
  const [pageError, setPageError] = useState("");

  // Contact seller
  const [contactMessage, setContactMessage] = useState("");
  const [sending, setSending] = useState(false);
  const [messageSent, setMessageSent] = useState(false);

  // Owner management
  const [showMarkSold, setShowMarkSold] = useState(false);
  const [conversations, setConversations] = useState<ConversationDto[]>([]);
  const [selectedBuyerId, setSelectedBuyerId] = useState<number | null>(null);
  const [buyerSearch, setBuyerSearch] = useState("");
  const [actionError, setActionError] = useState("");

  // Review
  const [reviewRating, setReviewRating] = useState(5);
  const [reviewComment, setReviewComment] = useState("");
  const [reviewSubmitted, setReviewSubmitted] = useState(false);
  const [reviewError, setReviewError] = useState("");

  const load = () => {
    api
      .get<AdvertDetail>(`/adverts/${id}`)
      .then((r) => setAdvert(r.data))
      .catch(() => setPageError("This advert could not be found."));
  };

  useEffect(() => {
    load();
  }, [id]);

  useEffect(() => {
    if (advert) {
      api.get<ReviewDto[]>(`/users/${advert.sellerId}/reviews`).then((r) => setReviews(r.data));
    }
  }, [advert?.sellerId]);

  // When the owner opens "mark as sold", load the conversations for this advert
  // so we can show a dropdown of people who enquired
  useEffect(() => {
    if (!showMarkSold || !advert?.isOwner) return;
    api.get<ConversationDto[]>("/chat/conversations").then((r) => {
      const forThisAdvert = r.data.filter((c) => c.advertId === advert.id);
      setConversations(forThisAdvert);
    });
  }, [showMarkSold, advert?.id, advert?.isOwner]);

  if (pageError) {
    return <div className="max-w-3xl mx-auto px-4 py-16 text-center text-ink-500">{pageError}</div>;
  }
  if (!advert) {
    return <div className="max-w-3xl mx-auto px-4 py-16 text-center text-ink-500">Loading...</div>;
  }

  const media = activeTab === "images" ? advert.imagePaths : advert.videoPaths;

  const toggleFavorite = async () => {
    if (!user) return navigate("/login");
    await api.post(`/favorites/${advert.id}/toggle`);
    setAdvert({ ...advert, isFavorite: !advert.isFavorite });
  };

  const sendMessage = async () => {
    if (!user) return navigate("/login");
    if (!contactMessage.trim()) return;
    setSending(true);
    try {
      await api.post("/chat/conversations", { advertId: advert.id, content: contactMessage });
      setContactMessage("");
      setMessageSent(true);
    } catch (err: any) {
      // if conversation already exists, navigate to chat directly
      navigate("/chat");
    } finally {
      setSending(false);
    }
  };

  const handleDelete = async () => {
    if (!confirm("Delete this advert? This cannot be undone.")) return;
    try {
      await api.delete(`/adverts/${advert.id}`);
      navigate("/my-adverts");
    } catch (err: any) {
      setActionError(err.response?.data?.error ?? "Could not delete advert.");
    }
  };

  const handleStatusChange = async (action: "off-sale" | "activate") => {
    setActionError("");
    try {
      await api.post(`/adverts/${advert.id}/${action}`);
      load();
    } catch (err: any) {
      setActionError(err.response?.data?.error ?? "Could not update status.");
    }
  };

  const handleMarkSold = async () => {
    if (!selectedBuyerId) {
      setActionError("Please select the buyer.");
      return;
    }
    setActionError("");
    try {
      await api.post(`/adverts/${advert.id}/mark-sold`, { buyerId: selectedBuyerId });
      setShowMarkSold(false);
      load();
    } catch (err: any) {
      setActionError(err.response?.data?.error ?? "Could not mark as sold.");
    }
  };

  const filteredConversations = conversations.filter((c) =>
    buyerSearch === "" || c.otherUsername.toLowerCase().includes(buyerSearch.toLowerCase())
  );

  const submitReview = async () => {
    setReviewError("");
    try {
      await api.post("/reviews", { advertId: advert.id, rating: reviewRating, comment: reviewComment });
      setReviewSubmitted(true);
      api.get<ReviewDto[]>(`/users/${advert.sellerId}/reviews`).then((r) => setReviews(r.data));
    } catch (err: any) {
      setReviewError(err.response?.data?.error ?? "Could not submit review.");
    }
  };

  const canReview = user && advert.status === "Sold" && advert.buyerId === user.id && !reviews.some((r) => r.reviewerId === user.id);

  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 py-8">
      <div className="grid lg:grid-cols-3 gap-8">
        <div className="lg:col-span-2 space-y-6">

          {/* Gallery */}
          <div className="bg-base-900 border border-base-700 rounded-xl overflow-hidden">
            <div className="relative aspect-video bg-base-800 flex items-center justify-center">
              {media.length === 0 && <span className="text-ink-500 text-sm">No media available</span>}
              {activeTab === "images" && media.length > 0 && (
                <img src={fileUrl(media[activeMedia])} alt={advert.title} className="w-full h-full object-contain" />
              )}
              {activeTab === "videos" && media.length > 0 && (
                <video key={media[activeMedia]} src={fileUrl(media[activeMedia])} controls className="w-full h-full object-contain" />
              )}
              {media.length > 1 && (
                <>
                  <button onClick={() => setActiveMedia((i) => (i - 1 + media.length) % media.length)} className="absolute left-2 top-1/2 -translate-y-1/2 bg-base-950/70 hover:bg-base-950 rounded-full p-1.5">
                    <ChevronLeft size={18} />
                  </button>
                  <button onClick={() => setActiveMedia((i) => (i + 1) % media.length)} className="absolute right-2 top-1/2 -translate-y-1/2 bg-base-950/70 hover:bg-base-950 rounded-full p-1.5">
                    <ChevronRight size={18} />
                  </button>
                </>
              )}
              {advert.status !== "Active" && (
                <span className="absolute top-3 left-3 bg-base-950/85 text-ink-100 text-xs font-semibold px-2.5 py-1 rounded-md uppercase tracking-wide">
                  {advert.status === "Sold" ? "Sold" : "Off sale"}
                </span>
              )}
            </div>

            {(advert.imagePaths.length > 0 || advert.videoPaths.length > 0) && (
              <div className="p-3 flex items-center gap-3">
                {advert.imagePaths.length > 0 && (
                  <button onClick={() => { setActiveTab("images"); setActiveMedia(0); }} className={`text-xs font-medium px-3 py-1.5 rounded-lg border ${activeTab === "images" ? "bg-amber-500 text-base-950 border-amber-500" : "border-base-700 text-ink-300"}`}>
                    Photos ({advert.imagePaths.length})
                  </button>
                )}
                {advert.videoPaths.length > 0 && (
                  <button onClick={() => { setActiveTab("videos"); setActiveMedia(0); }} className={`text-xs font-medium px-3 py-1.5 rounded-lg border flex items-center gap-1.5 ${activeTab === "videos" ? "bg-amber-500 text-base-950 border-amber-500" : "border-base-700 text-ink-300"}`}>
                    <Play size={12} /> Videos ({advert.videoPaths.length})
                  </button>
                )}
              </div>
            )}

            {media.length > 1 && (
              <div className="px-3 pb-3 flex gap-2 overflow-x-auto">
                {media.map((m, i) => (
                  <button key={m + i} onClick={() => setActiveMedia(i)} className={`w-16 h-12 shrink-0 rounded-md overflow-hidden border-2 ${i === activeMedia ? "border-amber-500" : "border-transparent"}`}>
                    {activeTab === "images" ? (
                      <img src={fileUrl(m)} className="w-full h-full object-cover" alt="" />
                    ) : (
                      <div className="w-full h-full bg-base-800 flex items-center justify-center"><Play size={14} /></div>
                    )}
                  </button>
                ))}
              </div>
            )}
          </div>

          {/* Title & price */}
          <div>
            <div className="flex items-start justify-between gap-4">
              <h1 className="font-display font-bold text-2xl sm:text-3xl">{advert.title}</h1>
              <button onClick={toggleFavorite} className="shrink-0 bg-base-900 border border-base-700 rounded-full p-2.5 hover:border-amber-500/50">
                <Heart size={20} className={advert.isFavorite ? "fill-amber-500 text-amber-500" : "text-ink-300"} />
              </button>
            </div>
            <p className="text-ink-500 mt-1">{advert.brandName} {advert.modelName} {advert.trim ?? ""}</p>
            <p className="font-display font-extrabold text-3xl text-amber-400 mt-3">
              {advert.price.toLocaleString()} {advert.currencySymbol}
            </p>
          </div>

          {/* Specs */}
          <div className="bg-base-900 border border-base-700 rounded-xl p-5">
            <h2 className="font-display font-semibold text-lg mb-4">Specifications</h2>
            <div className="grid grid-cols-2 sm:grid-cols-3 gap-4 text-sm">
              <Spec icon={Calendar} label="Year" value={String(advert.year)} />
              <Spec icon={Gauge} label="Mileage" value={`${advert.mileage.toLocaleString()} km`} />
              <Spec icon={Fuel} label="Fuel" value={FUEL_LABELS[advert.fuelType] ?? advert.fuelType} />
              <Spec icon={Cog} label="Transmission" value={TRANSMISSION_LABELS[advert.transmission] ?? advert.transmission} />
              <Spec icon={Settings2} label="Body type" value={BODY_LABELS[advert.bodyType] ?? advert.bodyType} />
              <Spec icon={Zap} label="Drivetrain" value={DRIVETRAIN_LABELS[advert.drivetrain] ?? advert.drivetrain} />
              <Spec icon={Zap} label="Power" value={`${advert.enginePowerHp} hp`} />
              <Spec icon={Settings2} label="Engine" value={`${advert.engineSizeLiters} L`} />
              <Spec icon={Palette} label="Color" value={advert.color} />
              <Spec icon={Users} label="Owners" value={String(advert.ownersCount)} />
              <Spec icon={CheckCircle2} label="Condition" value={CONDITION_LABELS[advert.condition] ?? advert.condition} />
            </div>

            {advert.condition === "Damaged" && advert.damageSeverity != null && (
              <div className="mt-4 pt-4 border-t border-base-700">
                <p className="text-sm text-ink-300 mb-1.5">Damage severity</p>
                <div className="w-full bg-base-800 rounded-full h-2">
                  <div className="bg-amber-500 h-2 rounded-full" style={{ width: `${(advert.damageSeverity / 10) * 100}%` }} />
                </div>
                <p className="text-xs text-ink-500 mt-1">{advert.damageSeverity} / 10</p>
              </div>
            )}

            {advert.repairDescription && (
              <div className="mt-4 pt-4 border-t border-base-700">
                <p className="text-sm font-medium text-ink-300 mb-1">Repair description</p>
                <p className="text-sm text-ink-500">{advert.repairDescription}</p>
              </div>
            )}
          </div>

          {/* Description */}
          <div className="bg-base-900 border border-base-700 rounded-xl p-5">
            <h2 className="font-display font-semibold text-lg mb-3">Description</h2>
            <p className="text-sm text-ink-300 whitespace-pre-wrap">{advert.description}</p>
          </div>

          {/* Features */}
          {advert.features.length > 0 && (
            <div className="bg-base-900 border border-base-700 rounded-xl p-5">
              <h2 className="font-display font-semibold text-lg mb-3">Features</h2>
              <div className="flex flex-wrap gap-2">
                {advert.features.map((f) => (
                  <span key={f} className="text-xs bg-base-800 border border-base-700 text-ink-300 px-2.5 py-1 rounded-full">{f}</span>
                ))}
              </div>
            </div>
          )}

          {/* Sale history */}
          {advert.saleHistory.length > 0 && (
            <div className="bg-base-900 border border-base-700 rounded-xl p-5">
              <h2 className="font-display font-semibold text-lg mb-4 flex items-center gap-2">
                <History size={18} className="text-amber-400" /> Sale history
              </h2>
              <div className="space-y-3">
                {advert.saleHistory.map((h) => (
                  <div key={h.advertId} className="flex items-center justify-between border-b border-base-700 last:border-0 pb-3 last:pb-0 text-sm">
                    <div>
                      <Link to={`/adverts/${h.advertId}`} className="text-ink-100 hover:text-amber-400 font-medium">
                        Sold by {h.sellerUsername}
                      </Link>
                      <p className="text-ink-500 text-xs mt-0.5">
                        {h.soldAt ? new Date(h.soldAt).toLocaleDateString() : "Unknown date"} · {h.mileage.toLocaleString()} km
                        {h.buyerUsername ? ` · to ${h.buyerUsername}` : ""}
                      </p>
                    </div>
                    <span className="font-display font-semibold text-amber-400">{h.price.toLocaleString()} {h.currencyCode}</span>
                  </div>
                ))}
              </div>
            </div>
          )}

          {/* Reviews */}
          <div className="bg-base-900 border border-base-700 rounded-xl p-5">
            <h2 className="font-display font-semibold text-lg mb-4">Seller reviews ({reviews.length})</h2>
            {reviews.length === 0 && <p className="text-sm text-ink-500">No reviews yet.</p>}
            <div className="space-y-4">
              {reviews.map((r) => (
                <div key={r.id} className="border-b border-base-700 last:border-0 pb-4 last:pb-0">
                  <div className="flex items-center justify-between mb-1">
                    <Link to={`/users/${r.reviewerId}`} className="font-medium text-sm hover:text-amber-400">{r.reviewerUsername}</Link>
                    <RatingStars rating={r.rating} />
                  </div>
                  <p className="text-sm text-ink-500">{r.comment}</p>
                  <p className="text-xs text-ink-500/60 mt-1">{new Date(r.createdAt).toLocaleDateString()}</p>
                </div>
              ))}
            </div>

            {canReview && !reviewSubmitted && (
              <div className="mt-5 pt-5 border-t border-base-700">
                <p className="text-sm font-medium mb-3">Leave a review for this seller</p>
                <div className="flex items-center gap-1 mb-3">
                  {[1, 2, 3, 4, 5].map((i) => (
                    <button key={i} onClick={() => setReviewRating(i)}>
                      <Star size={22} className={i <= reviewRating ? "fill-amber-400 text-amber-400" : "text-base-600"} />
                    </button>
                  ))}
                </div>
                {reviewError && <p className="text-red-400 text-sm mb-2">{reviewError}</p>}
                <textarea value={reviewComment} onChange={(e) => setReviewComment(e.target.value)} placeholder="Share your experience with this seller..." className="w-full bg-base-800 border border-base-700 rounded-lg px-3 py-2 text-sm focus-ring mb-3" rows={3} />
                <button onClick={submitReview} className="bg-amber-500 hover:bg-amber-400 text-base-950 font-semibold px-4 py-2 rounded-lg text-sm transition-colors">
                  Submit review
                </button>
              </div>
            )}
            {reviewSubmitted && <p className="text-sm text-amber-400 mt-4 pt-4 border-t border-base-700">Thanks for your review!</p>}
          </div>
        </div>

        {/* Sidebar */}
        <div className="space-y-5">

          {/* Seller card */}
          <div className="bg-base-900 border border-base-700 rounded-xl p-5">
            <div className="flex items-center gap-3 mb-4">
              {advert.sellerProfilePhotoPath ? (
                <img src={fileUrl(advert.sellerProfilePhotoPath)} className="w-12 h-12 rounded-full object-cover border border-base-600" alt="" />
              ) : (
                <div className="w-12 h-12 rounded-full bg-base-700" />
              )}
              <div>
                <Link to={`/users/${advert.sellerId}`} className="font-semibold hover:text-amber-400">{advert.sellerUsername}</Link>
                <div className="flex items-center gap-1.5 mt-0.5">
                  <RatingStars rating={advert.sellerAverageRating} />
                  <span className="text-xs text-ink-500">({advert.sellerAverageRating})</span>
                </div>
              </div>
            </div>

            <div className="space-y-1.5 text-sm text-ink-300">
              <p className="font-medium text-ink-100">{advert.sellerFullName}</p>
              <p className="flex items-center gap-2 text-ink-500"><MapPin size={14} /> {advert.sellerCity}</p>
              {advert.sellerPhone && <p className="text-ink-500">{advert.sellerPhone}</p>}
              <p className="text-ink-500">{advert.sellerEmail}</p>
            </div>

            {!advert.isOwner && advert.status === "Active" && (
              <div className="mt-4 pt-4 border-t border-base-700">
                {messageSent ? (
                  <div className="bg-amber-500/10 border border-amber-500/30 text-amber-400 text-sm rounded-lg px-3 py-2 flex items-center gap-2">
                    <CheckCircle2 size={15} /> Message sent! <Link to="/chat" className="underline ml-auto">Open chat</Link>
                  </div>
                ) : (
                  <>
                    <textarea
                      value={contactMessage}
                      onChange={(e) => setContactMessage(e.target.value)}
                      placeholder={`Hi, is the ${advert.modelName} still available?`}
                      className="w-full bg-base-800 border border-base-700 rounded-lg px-3 py-2 text-sm focus-ring mb-2"
                      rows={3}
                    />
                    <button onClick={sendMessage} disabled={sending} className="w-full bg-amber-500 hover:bg-amber-400 disabled:opacity-60 text-base-950 font-semibold py-2.5 rounded-lg flex items-center justify-center gap-2 transition-colors">
                      <MessageSquare size={16} /> Message seller
                    </button>
                  </>
                )}
              </div>
            )}
          </div>

          {/* Owner management */}
          {advert.isOwner && (
            <div className="bg-base-900 border border-base-700 rounded-xl p-5 space-y-2">
              <h3 className="font-display font-semibold text-sm uppercase tracking-wide text-ink-500 mb-2">Manage advert</h3>

              {actionError && (
                <div className="bg-red-500/10 border border-red-500/30 text-red-400 text-xs rounded-lg px-3 py-2 flex items-center gap-2">
                  <AlertCircle size={13} /> {actionError}
                </div>
              )}

              <Link to={`/adverts/${advert.id}/edit`} className="w-full flex items-center justify-center gap-2 bg-base-800 hover:bg-base-700 border border-base-700 rounded-lg py-2.5 text-sm font-medium transition-colors">
                <Pencil size={15} /> Edit advert
              </Link>

              {advert.status === "Active" && (
                <button onClick={() => handleStatusChange("off-sale")} className="w-full flex items-center justify-center gap-2 bg-base-800 hover:bg-base-700 border border-base-700 rounded-lg py-2.5 text-sm font-medium transition-colors">
                  <EyeOff size={15} /> Take off sale
                </button>
              )}
              {advert.status === "OffSale" && (
                <button onClick={() => handleStatusChange("activate")} className="w-full flex items-center justify-center gap-2 bg-base-800 hover:bg-base-700 border border-base-700 rounded-lg py-2.5 text-sm font-medium transition-colors">
                  <CheckCircle2 size={15} /> Make active again
                </button>
              )}

              {advert.status !== "Sold" && (
                <button
                  onClick={() => { setShowMarkSold((v) => !v); setActionError(""); setSelectedBuyerId(null); setBuyerSearch(""); }}
                  className="w-full flex items-center justify-center gap-2 bg-base-800 hover:bg-base-700 border border-base-700 rounded-lg py-2.5 text-sm font-medium transition-colors"
                >
                  <CheckCircle2 size={15} /> Mark as sold
                </button>
              )}

              {showMarkSold && (
                <div className="border border-base-700 rounded-lg p-3 space-y-2 bg-base-800/50">
                  <p className="text-xs text-ink-500 font-medium">Who bought it?</p>

                  {conversations.length === 0 ? (
                    <p className="text-xs text-ink-500">No one has messaged about this advert yet. You can still enter a buyer — register their account first and select them here after they register.</p>
                  ) : (
                    <>
                      <input
                        value={buyerSearch}
                        onChange={(e) => setBuyerSearch(e.target.value)}
                        placeholder="Search by username..."
                        className="w-full bg-base-800 border border-base-700 rounded-lg px-2.5 py-1.5 text-xs focus-ring"
                      />
                      <div className="space-y-1 max-h-40 overflow-y-auto">
                        {filteredConversations.map((c) => (
                          <button
                            key={c.id}
                            onClick={() => setSelectedBuyerId(c.otherUserId)}
                            className={`w-full flex items-center gap-2 px-2.5 py-2 rounded-lg text-sm text-left transition-colors ${
                              selectedBuyerId === c.otherUserId ? "bg-amber-500/20 border border-amber-500/40 text-amber-400" : "hover:bg-base-700 border border-transparent"
                            }`}
                          >
                            {c.otherUserProfilePhotoPath ? (
                              <img src={fileUrl(c.otherUserProfilePhotoPath)} className="w-6 h-6 rounded-full object-cover shrink-0" alt="" />
                            ) : (
                              <div className="w-6 h-6 rounded-full bg-base-600 shrink-0" />
                            )}
                            <span className="font-medium">{c.otherUsername}</span>
                            {selectedBuyerId === c.otherUserId && <CheckCircle2 size={14} className="ml-auto text-amber-400" />}
                          </button>
                        ))}
                        {filteredConversations.length === 0 && buyerSearch && (
                          <p className="text-xs text-ink-500 px-2">No match found.</p>
                        )}
                      </div>
                    </>
                  )}

                  <button
                    onClick={handleMarkSold}
                    disabled={!selectedBuyerId}
                    className="w-full bg-amber-500 hover:bg-amber-400 disabled:opacity-40 text-base-950 font-semibold py-2 rounded-lg text-sm transition-colors"
                  >
                    Confirm sale
                  </button>
                </div>
              )}

              <button onClick={handleDelete} className="w-full flex items-center justify-center gap-2 bg-red-500/10 hover:bg-red-500/20 border border-red-500/30 text-red-400 rounded-lg py-2.5 text-sm font-medium transition-colors">
                <Trash2 size={15} /> Delete advert
              </button>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}

function Spec({ icon: Icon, label, value }: { icon: any; label: string; value: string }) {
  return (
    <div className="flex items-start gap-2.5">
      <Icon size={16} className="text-amber-400 mt-0.5" />
      <div>
        <p className="text-ink-500 text-xs">{label}</p>
        <p className="text-ink-100 font-medium">{value}</p>
      </div>
    </div>
  );
}
