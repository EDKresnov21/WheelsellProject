import { Link } from "react-router-dom";
import { Heart, MapPin, Gauge, Calendar, Fuel, Cog } from "lucide-react";
import type { AdvertSummary } from "../types";
import { fileUrl } from "../api/client";

interface Props {
  advert: AdvertSummary;
  onToggleFavorite?: (id: number) => void;
  showFavorite?: boolean;
}

const fuelLabels: Record<string, string> = {
  Petrol: "Petrol",
  Diesel: "Diesel",
  Electric: "Electric",
  Hybrid: "Hybrid",
  PlugInHybrid: "Plug-in Hybrid",
  LPG: "LPG",
  CNG: "CNG",
  Hydrogen: "Hydrogen"
};

export default function AdvertCard({ advert, onToggleFavorite, showFavorite = true }: Props) {
  return (
    <div className="group bg-base-900 border border-base-700 rounded-xl overflow-hidden hover:border-amber-500/50 transition-colors flex flex-col">
      <Link to={`/adverts/${advert.id}`} className="relative aspect-[4/3] bg-base-800 block overflow-hidden">
        {advert.thumbnailPath ? (
          <img
            src={fileUrl(advert.thumbnailPath)}
            alt={advert.title}
            className="w-full h-full object-cover group-hover:scale-105 transition-transform duration-300"
          />
        ) : (
          <div className="w-full h-full flex items-center justify-center text-ink-500 text-sm">No photo</div>
        )}
        {advert.status !== "Active" && (
          <span className="absolute top-2 left-2 bg-base-950/80 text-ink-100 text-xs font-semibold px-2 py-1 rounded-md uppercase tracking-wide">
            {advert.status === "Sold" ? "Sold" : "Off sale"}
          </span>
        )}
        {showFavorite && onToggleFavorite && (
          <button
            onClick={(e) => {
              e.preventDefault();
              onToggleFavorite(advert.id);
            }}
            className="absolute top-2 right-2 bg-base-950/70 hover:bg-base-950 rounded-full p-1.5 transition-colors"
          >
            <Heart size={16} className={advert.isFavorite ? "fill-amber-500 text-amber-500" : "text-ink-100"} />
          </button>
        )}
      </Link>

      <div className="p-4 flex flex-col gap-2 flex-1">
        <Link to={`/adverts/${advert.id}`} className="font-display font-semibold text-base leading-snug hover:text-amber-400 transition-colors line-clamp-2">
          {advert.title}
        </Link>
        <p className="text-sm text-ink-500">
          {advert.brandName} {advert.modelName}
        </p>

        <div className="grid grid-cols-2 gap-1.5 text-xs text-ink-300 mt-1">
          <span className="flex items-center gap-1.5">
            <Calendar size={13} /> {advert.year}
          </span>
          <span className="flex items-center gap-1.5">
            <Gauge size={13} /> {advert.mileage.toLocaleString()} km
          </span>
          <span className="flex items-center gap-1.5">
            <Fuel size={13} /> {fuelLabels[advert.fuelType] ?? advert.fuelType}
          </span>
          <span className="flex items-center gap-1.5">
            <Cog size={13} /> {advert.transmission}
          </span>
        </div>

        <div className="flex items-center justify-between mt-auto pt-3 border-t border-base-700">
          <span className="font-display font-bold text-lg text-amber-400">
            {advert.price.toLocaleString()} {advert.currencySymbol}
          </span>
          <span className="flex items-center gap-1 text-xs text-ink-500">
            <MapPin size={13} /> {advert.city}
          </span>
        </div>
      </div>
    </div>
  );
}
