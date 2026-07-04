import { useEffect, useState } from "react";
import { useNavigate, useSearchParams } from "react-router-dom";
import { Filter, X, ChevronLeft, ChevronRight } from "lucide-react";
import { api } from "../api/client";
import { useAuthStore } from "../store/authStore";
import AdvertCard from "../components/AdvertCard";
import type { AdvertSummary, Brand, CarModel, Currency, PagedResult } from "../types";
import { FUEL_TYPES, FUEL_LABELS, TRANSMISSION_TYPES, TRANSMISSION_LABELS, BODY_TYPES, BODY_LABELS, DRIVETRAIN_TYPES, DRIVETRAIN_LABELS, CONDITIONS, CONDITION_LABELS } from "../constants/enums";

function useQueryParam(params: URLSearchParams, key: string) {
  return params.get(key) ?? "";
}

export default function Search() {
  const navigate = useNavigate();
  const [params, setParams] = useSearchParams();
  const user = useAuthStore((s) => s.user);

  const [results, setResults] = useState<PagedResult<AdvertSummary> | null>(null);
  const [brands, setBrands] = useState<Brand[]>([]);
  const [models, setModels] = useState<CarModel[]>([]);
  const [currencies, setCurrencies] = useState<Currency[]>([]);
  const [showFilters, setShowFilters] = useState(false);

  const [query, setQuery] = useState(useQueryParam(params, "query"));
  const [brandId, setBrandId] = useState(useQueryParam(params, "brandId"));
  const [carModelId, setCarModelId] = useState(useQueryParam(params, "carModelId"));
  const [yearFrom, setYearFrom] = useState(useQueryParam(params, "yearFrom"));
  const [yearTo, setYearTo] = useState(useQueryParam(params, "yearTo"));
  const [priceFrom, setPriceFrom] = useState(useQueryParam(params, "priceFrom"));
  const [priceTo, setPriceTo] = useState(useQueryParam(params, "priceTo"));
  const [currencyId, setCurrencyId] = useState(useQueryParam(params, "currencyId"));
  const [mileageFrom, setMileageFrom] = useState(useQueryParam(params, "mileageFrom"));
  const [mileageTo, setMileageTo] = useState(useQueryParam(params, "mileageTo"));
  const [fuelTypes, setFuelTypes] = useState<string[]>(params.getAll("fuelTypes"));
  const [transmissions, setTransmissions] = useState<string[]>(params.getAll("transmissions"));
  const [bodyTypes, setBodyTypes] = useState<string[]>(params.getAll("bodyTypes"));
  const [drivetrains, setDrivetrains] = useState<string[]>(params.getAll("drivetrains"));
  const [conditions, setConditions] = useState<string[]>(params.getAll("conditions"));
  const [sortBy, setSortBy] = useState(useQueryParam(params, "sortBy") || "createdAt");
  const [sortDescending, setSortDescending] = useState(params.get("sortDescending") !== "false");
  const page = parseInt(useQueryParam(params, "page") || "1", 10);

  useEffect(() => {
    api.get<Brand[]>("/lookups/brands").then((r) => setBrands(r.data));
    api.get<Currency[]>("/lookups/currencies").then((r) => setCurrencies(r.data));
  }, []);

  useEffect(() => {
    if (brandId) {
      api.get<CarModel[]>("/lookups/models", { params: { brandId } }).then((r) => setModels(r.data));
    } else {
      setModels([]);
    }
  }, [brandId]);

  useEffect(() => {
    const search: Record<string, any> = { page, pageSize: 12, sortBy: params.get("sortBy") || "createdAt", sortDescending: params.get("sortDescending") !== "false" };
    if (params.get("query")) search.query = params.get("query");
    if (params.get("brandId")) search.brandId = params.get("brandId");
    if (params.get("carModelId")) search.carModelId = params.get("carModelId");
    if (params.get("yearFrom")) search.yearFrom = params.get("yearFrom");
    if (params.get("yearTo")) search.yearTo = params.get("yearTo");
    if (params.get("priceFrom")) search.priceFrom = params.get("priceFrom");
    if (params.get("priceTo")) search.priceTo = params.get("priceTo");
    if (params.get("currencyId")) search.currencyId = params.get("currencyId");
    if (params.get("mileageFrom")) search.mileageFrom = params.get("mileageFrom");
    if (params.get("mileageTo")) search.mileageTo = params.get("mileageTo");
    const fts = params.getAll("fuelTypes");
    if (fts.length) search.fuelTypes = fts;
    const trs = params.getAll("transmissions");
    if (trs.length) search.transmissions = trs;
    const bts = params.getAll("bodyTypes");
    if (bts.length) search.bodyTypes = bts;
    const dts = params.getAll("drivetrains");
    if (dts.length) search.drivetrains = dts;
    const cds = params.getAll("conditions");
    if (cds.length) search.conditions = cds;

    api
      .get<PagedResult<AdvertSummary>>("/adverts", { params: search, paramsSerializer: { indexes: null } })
      .then((r) => setResults(r.data));
  }, [params]);

  const applyFilters = (overrides: { sortBy?: string; sortDescending?: boolean; page?: number } = {}) => {
    const sb = overrides.sortBy ?? sortBy;
    const sd = overrides.sortDescending ?? sortDescending;
    const p = overrides.page ?? 1;

    const next = new URLSearchParams();
    if (query) next.set("query", query);
    if (brandId) next.set("brandId", brandId);
    if (carModelId) next.set("carModelId", carModelId);
    if (yearFrom) next.set("yearFrom", yearFrom);
    if (yearTo) next.set("yearTo", yearTo);
    if (priceFrom) next.set("priceFrom", priceFrom);
    if (priceTo) next.set("priceTo", priceTo);
    if (currencyId) next.set("currencyId", currencyId);
    if (mileageFrom) next.set("mileageFrom", mileageFrom);
    if (mileageTo) next.set("mileageTo", mileageTo);
    fuelTypes.forEach((v) => next.append("fuelTypes", v));
    transmissions.forEach((v) => next.append("transmissions", v));
    bodyTypes.forEach((v) => next.append("bodyTypes", v));
    drivetrains.forEach((v) => next.append("drivetrains", v));
    conditions.forEach((v) => next.append("conditions", v));
    next.set("sortBy", sb);
    next.set("sortDescending", String(sd));
    next.set("page", String(p));
    setParams(next);
  };

  // Sort changes: pass new values directly into applyFilters to avoid stale closure
  const handleSortChange = (value: string) => {
    const [sb, sd] = value.split(":");
    setSortBy(sb);
    setSortDescending(sd === "true");
    applyFilters({ sortBy: sb, sortDescending: sd === "true" });
  };

  const toggleArrayValue = (arr: string[], setArr: (v: string[]) => void, value: string) => {
    setArr(arr.includes(value) ? arr.filter((v) => v !== value) : [...arr, value]);
  };

  const toggleFavorite = async (id: number) => {
    if (!user) return navigate("/login");
    await api.post(`/favorites/${id}/toggle`);
    setResults((prev) => prev && { ...prev, items: prev.items.map((a) => (a.id === id ? { ...a, isFavorite: !a.isFavorite } : a)) });
  };

  const goToPage = (p: number) => {
    const next = new URLSearchParams(params);
    next.set("page", String(p));
    setParams(next);
  };

  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 py-8">
      <div className="flex items-center justify-between mb-6">
        <h1 className="font-display font-bold text-2xl">Browse cars</h1>
        <button onClick={() => setShowFilters((v) => !v)} className="sm:hidden flex items-center gap-2 text-sm bg-base-800 border border-base-700 rounded-lg px-3 py-2">
          <Filter size={15} /> Filters
        </button>
      </div>

      <div className="flex gap-6">
        <aside className={`${showFilters ? "block" : "hidden"} sm:block w-full sm:w-72 shrink-0`}>
          <div className="bg-base-900 border border-base-700 rounded-xl p-4 space-y-5 sticky top-20">
            <div className="flex items-center justify-between sm:hidden">
              <h2 className="font-semibold">Filters</h2>
              <button onClick={() => setShowFilters(false)}>
                <X size={18} />
              </button>
            </div>

            <div>
              <label className="text-xs font-semibold text-ink-500 uppercase tracking-wide block mb-1.5">Keyword</label>
              <input value={query} onChange={(e) => setQuery(e.target.value)} className="w-full bg-base-800 border border-base-700 rounded-lg px-3 py-2 text-sm focus-ring" />
            </div>

            <div className="grid grid-cols-2 gap-2">
              <div>
                <label className="text-xs font-semibold text-ink-500 uppercase tracking-wide block mb-1.5">Brand</label>
                <select value={brandId} onChange={(e) => { setBrandId(e.target.value); setCarModelId(""); }} className="w-full bg-base-800 border border-base-700 rounded-lg px-2 py-2 text-sm focus-ring">
                  <option value="">Any</option>
                  {brands.map((b) => (<option key={b.id} value={b.id}>{b.name}</option>))}
                </select>
              </div>
              <div>
                <label className="text-xs font-semibold text-ink-500 uppercase tracking-wide block mb-1.5">Model</label>
                <select value={carModelId} onChange={(e) => setCarModelId(e.target.value)} disabled={!brandId} className="w-full bg-base-800 border border-base-700 rounded-lg px-2 py-2 text-sm focus-ring disabled:opacity-50">
                  <option value="">Any</option>
                  {models.map((m) => (<option key={m.id} value={m.id}>{m.name}</option>))}
                </select>
              </div>
            </div>

            <div className="grid grid-cols-2 gap-2">
              <RangeField label="Year from" value={yearFrom} onChange={setYearFrom} />
              <RangeField label="Year to" value={yearTo} onChange={setYearTo} />
            </div>
            <div className="grid grid-cols-2 gap-2">
              <RangeField label="Price from" value={priceFrom} onChange={setPriceFrom} />
              <RangeField label="Price to" value={priceTo} onChange={setPriceTo} />
            </div>

            <div>
              <label className="text-xs font-semibold text-ink-500 uppercase tracking-wide block mb-1.5">Currency</label>
              <select value={currencyId} onChange={(e) => setCurrencyId(e.target.value)} className="w-full bg-base-800 border border-base-700 rounded-lg px-2 py-2 text-sm focus-ring">
                <option value="">Any</option>
                {currencies.map((c) => (<option key={c.id} value={c.id}>{c.code}</option>))}
              </select>
            </div>

            <div className="grid grid-cols-2 gap-2">
              <RangeField label="Mileage from" value={mileageFrom} onChange={setMileageFrom} />
              <RangeField label="Mileage to" value={mileageTo} onChange={setMileageTo} />
            </div>

            <FilterGroup label="Fuel type" options={FUEL_TYPES} labels={FUEL_LABELS} selected={fuelTypes} onToggle={(v) => toggleArrayValue(fuelTypes, setFuelTypes, v)} />
            <FilterGroup label="Transmission" options={TRANSMISSION_TYPES} labels={TRANSMISSION_LABELS} selected={transmissions} onToggle={(v) => toggleArrayValue(transmissions, setTransmissions, v)} />
            <FilterGroup label="Body type" options={BODY_TYPES} labels={BODY_LABELS} selected={bodyTypes} onToggle={(v) => toggleArrayValue(bodyTypes, setBodyTypes, v)} />
            <FilterGroup label="Drivetrain" options={DRIVETRAIN_TYPES} labels={DRIVETRAIN_LABELS} selected={drivetrains} onToggle={(v) => toggleArrayValue(drivetrains, setDrivetrains, v)} />
            <FilterGroup label="Condition" options={CONDITIONS} labels={CONDITION_LABELS} selected={conditions} onToggle={(v) => toggleArrayValue(conditions, setConditions, v)} />

            <button onClick={() => applyFilters()} className="w-full bg-amber-500 hover:bg-amber-400 text-base-950 font-semibold py-2.5 rounded-lg transition-colors">
              Apply filters
            </button>
          </div>
        </aside>

        <div className="flex-1">
          <div className="flex items-center justify-between mb-4">
            <p className="text-sm text-ink-500">{results ? `${results.totalCount} results` : "Loading..."}</p>
            <select
              value={`${sortBy}:${sortDescending}`}
              onChange={(e) => handleSortChange(e.target.value)}
              className="bg-base-800 border border-base-700 rounded-lg px-3 py-2 text-sm focus-ring"
            >
              <option value="createdAt:true">Newest first</option>
              <option value="price:false">Price: low to high</option>
              <option value="price:true">Price: high to low</option>
              <option value="year:true">Year: newest</option>
              <option value="mileage:false">Mileage: lowest</option>
            </select>
          </div>

          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-5">
            {results?.items.map((a) => (
              <AdvertCard key={a.id} advert={a} onToggleFavorite={toggleFavorite} />
            ))}
          </div>

          {results && results.items.length === 0 && (
            <p className="text-center text-ink-500 py-16">No adverts match your filters. Try adjusting your search.</p>
          )}

          {results && results.totalPages > 1 && (
            <div className="flex items-center justify-center gap-3 mt-8">
              <button disabled={page <= 1} onClick={() => goToPage(page - 1)} className="p-2 rounded-lg bg-base-800 border border-base-700 disabled:opacity-40">
                <ChevronLeft size={16} />
              </button>
              <span className="text-sm text-ink-300">
                Page {results.page} of {results.totalPages}
              </span>
              <button disabled={page >= results.totalPages} onClick={() => goToPage(page + 1)} className="p-2 rounded-lg bg-base-800 border border-base-700 disabled:opacity-40">
                <ChevronRight size={16} />
              </button>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}

function RangeField({ label, value, onChange }: { label: string; value: string; onChange: (v: string) => void }) {
  return (
    <div>
      <label className="text-xs font-semibold text-ink-500 uppercase tracking-wide block mb-1.5">{label}</label>
      <input type="number" value={value} onChange={(e) => onChange(e.target.value)} className="w-full bg-base-800 border border-base-700 rounded-lg px-2 py-2 text-sm focus-ring" />
    </div>
  );
}

function FilterGroup({ label, options, labels, selected, onToggle }: {
  label: string; options: string[]; labels: Record<string, string>; selected: string[]; onToggle: (v: string) => void;
}) {
  return (
    <div>
      <label className="text-xs font-semibold text-ink-500 uppercase tracking-wide block mb-1.5">{label}</label>
      <div className="flex flex-wrap gap-1.5">
        {options.map((o) => (
          <button
            key={o}
            onClick={() => onToggle(o)}
            className={`text-xs px-2.5 py-1 rounded-full border transition-colors ${
              selected.includes(o) ? "bg-amber-500 text-base-950 border-amber-500 font-medium" : "bg-base-800 border-base-700 text-ink-300 hover:border-amber-500/50"
            }`}
          >
            {labels[o] ?? o}
          </button>
        ))}
      </div>
    </div>
  );
}
