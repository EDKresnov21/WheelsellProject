import { useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { Upload, X, Play, Loader2 } from "lucide-react";
import { api, fileUrl } from "../api/client";
import type { AdvertDetail, Brand, CarModel, Currency, FeatureCategory, CreateAdvertRequest } from "../types";
import { FUEL_TYPES, FUEL_LABELS, TRANSMISSION_TYPES, TRANSMISSION_LABELS, BODY_TYPES, BODY_LABELS, DRIVETRAIN_TYPES, DRIVETRAIN_LABELS, CONDITIONS, CONDITION_LABELS } from "../constants/enums";

const emptyForm: CreateAdvertRequest = {
  title: "",
  description: "",
  brandId: 0,
  carModelId: 0,
  trim: "",
  year: new Date().getFullYear(),
  mileage: 0,
  fuelType: "Petrol",
  transmission: "Manual",
  bodyType: "Sedan",
  drivetrain: "FrontWheelDrive",
  enginePowerHp: 0,
  engineSizeLiters: 1.6,
  color: "",
  ownersCount: 1,
  condition: "New",
  damageSeverity: null,
  repairDescription: "",
  price: 0,
  currencyId: 1,
  sellerFullName: "",
  sellerCity: "",
  sellerEmail: "",
  sellerPhone: "",
  featureIds: []
};

export default function AdvertForm() {
  const { id } = useParams();
  const isEdit = !!id;
  const navigate = useNavigate();

  const [form, setForm] = useState<CreateAdvertRequest>(emptyForm);
  const [brands, setBrands] = useState<Brand[]>([]);
  const [models, setModels] = useState<CarModel[]>([]);
  const [currencies, setCurrencies] = useState<Currency[]>([]);
  const [categories, setCategories] = useState<FeatureCategory[]>([]);
  const [existingImages, setExistingImages] = useState<{ path: string; id?: number }[]>([]);
  const [existingVideos, setExistingVideos] = useState<{ path: string; id?: number }[]>([]);
  const [newImages, setNewImages] = useState<File[]>([]);
  const [newVideos, setNewVideos] = useState<File[]>([]);
  const [advertId, setAdvertId] = useState<number | null>(null);
  const [error, setError] = useState("");
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    api.get<Brand[]>("/lookups/brands").then((r) => setBrands(r.data));
    api.get<Currency[]>("/lookups/currencies").then((r) => setCurrencies(r.data));
    api.get<FeatureCategory[]>("/lookups/feature-categories").then((r) => setCategories(r.data));
  }, []);

  useEffect(() => {
    if (form.brandId) {
      api.get<CarModel[]>("/lookups/models", { params: { brandId: form.brandId } }).then((r) => setModels(r.data));
    }
  }, [form.brandId]);

  useEffect(() => {
    if (!isEdit) return;
    api.get<AdvertDetail>(`/adverts/${id}`).then((r) => {
      const a = r.data;
      setAdvertId(a.id);
      setForm({
        title: a.title,
        description: a.description,
        brandId: a.brandId,
        carModelId: a.carModelId,
        trim: a.trim ?? "",
        year: a.year,
        mileage: a.mileage,
        fuelType: a.fuelType,
        transmission: a.transmission,
        bodyType: a.bodyType,
        drivetrain: a.drivetrain,
        enginePowerHp: a.enginePowerHp,
        engineSizeLiters: a.engineSizeLiters,
        color: a.color,
        ownersCount: a.ownersCount,
        condition: a.condition,
        damageSeverity: a.damageSeverity ?? null,
        repairDescription: a.repairDescription ?? "",
        price: a.price,
        currencyId: a.currencyId,
        sellerFullName: a.sellerFullName,
        sellerCity: a.sellerCity,
        sellerEmail: a.sellerEmail,
        sellerPhone: a.sellerPhone,
        featureIds: []
      });
      setExistingImages(a.imagePaths.map((p) => ({ path: p })));
      setExistingVideos(a.videoPaths.map((p) => ({ path: p })));
      sessionStorage.setItem("wheelsell_edit_feature_names", JSON.stringify(a.features));
    });
  }, [id]);

  useEffect(() => {
    if (!isEdit || categories.length === 0) return;
    const names: string[] = JSON.parse(sessionStorage.getItem("wheelsell_edit_feature_names") ?? "[]");
    const ids = categories.flatMap((c) => c.features).filter((f) => names.includes(f.name)).map((f) => f.id);
    setForm((f) => ({ ...f, featureIds: ids }));
  }, [categories, isEdit]);

  const update = <K extends keyof CreateAdvertRequest>(key: K, value: CreateAdvertRequest[K]) => {
    setForm((f) => ({ ...f, [key]: value }));
  };

  const toggleFeature = (featureId: number) => {
    setForm((f) => ({
      ...f,
      featureIds: f.featureIds.includes(featureId) ? f.featureIds.filter((fid) => fid !== featureId) : [...f.featureIds, featureId]
    }));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError("");
    setSaving(true);
    try {
      let resultId = advertId;
      if (isEdit && advertId) {
        await api.put(`/adverts/${advertId}`, form);
      } else {
        const res = await api.post<AdvertDetail>("/adverts", form);
        resultId = res.data.id;
      }

      if (resultId && newImages.length > 0) {
        const data = new FormData();
        newImages.forEach((f) => data.append("files", f));
        await api.post(`/adverts/${resultId}/images`, data, { headers: { "Content-Type": "multipart/form-data" } });
      }

      if (resultId && newVideos.length > 0) {
        const data = new FormData();
        newVideos.forEach((f) => data.append("files", f));
        await api.post(`/adverts/${resultId}/videos`, data, { headers: { "Content-Type": "multipart/form-data" } });
      }

      navigate(`/adverts/${resultId}`);
    } catch (err: any) {
      setError(err.response?.data?.error ?? "Could not save the advert.");
    } finally {
      setSaving(false);
    }
  };

  const removeExistingImage = (img: { path: string; id?: number }) => {
    setExistingImages((prev) => prev.filter((p) => p.path !== img.path));
  };

  return (
    <div className="max-w-3xl mx-auto px-4 sm:px-6 py-8">
      <h1 className="font-display font-bold text-2xl mb-6">{isEdit ? "Edit advert" : "List your car"}</h1>

      {error && <div className="bg-red-500/10 border border-red-500/30 text-red-400 text-sm rounded-lg px-3 py-2 mb-4">{error}</div>}

      <form onSubmit={handleSubmit} className="space-y-6">
        <Section title="Basics">
          <Field label="Title" className="sm:col-span-2">
            <input value={form.title} onChange={(e) => update("title", e.target.value)} required className="input" />
          </Field>
          <Field label="Brand">
            <select value={form.brandId || ""} onChange={(e) => update("brandId", Number(e.target.value))} required className="input">
              <option value="">Select brand</option>
              {brands.map((b) => (
                <option key={b.id} value={b.id}>{b.name}</option>
              ))}
            </select>
          </Field>
          <Field label="Model">
            <select value={form.carModelId || ""} onChange={(e) => update("carModelId", Number(e.target.value))} required disabled={!form.brandId} className="input disabled:opacity-50">
              <option value="">Select model</option>
              {models.map((m) => (
                <option key={m.id} value={m.id}>{m.name}</option>
              ))}
            </select>
          </Field>
          <Field label="Trim (optional)">
            <input value={form.trim ?? ""} onChange={(e) => update("trim", e.target.value)} className="input" />
          </Field>
          <Field label="Year">
            <input type="number" value={form.year} onChange={(e) => update("year", Number(e.target.value))} required className="input" />
          </Field>
        </Section>

        <Section title="Technical details">
          <Field label="Mileage (km)">
            <input type="number" value={form.mileage} onChange={(e) => update("mileage", Number(e.target.value))} required className="input" />
          </Field>
          <Field label="Color">
            <input value={form.color} onChange={(e) => update("color", e.target.value)} required className="input" />
          </Field>
          <Field label="Fuel type">
            <select value={form.fuelType} onChange={(e) => update("fuelType", e.target.value)} className="input">
              {FUEL_TYPES.map((f) => <option key={f} value={f}>{FUEL_LABELS[f]}</option>)}
            </select>
          </Field>
          <Field label="Transmission">
            <select value={form.transmission} onChange={(e) => update("transmission", e.target.value)} className="input">
              {TRANSMISSION_TYPES.map((f) => <option key={f} value={f}>{TRANSMISSION_LABELS[f]}</option>)}
            </select>
          </Field>
          <Field label="Body type">
            <select value={form.bodyType} onChange={(e) => update("bodyType", e.target.value)} className="input">
              {BODY_TYPES.map((f) => <option key={f} value={f}>{BODY_LABELS[f]}</option>)}
            </select>
          </Field>
          <Field label="Drivetrain">
            <select value={form.drivetrain} onChange={(e) => update("drivetrain", e.target.value)} className="input">
              {DRIVETRAIN_TYPES.map((f) => <option key={f} value={f}>{DRIVETRAIN_LABELS[f]}</option>)}
            </select>
          </Field>
          <Field label="Engine power (hp)">
            <input type="number" value={form.enginePowerHp} onChange={(e) => update("enginePowerHp", Number(e.target.value))} required className="input" />
          </Field>
          <Field label="Engine size (L)">
            <input type="number" step="0.1" value={form.engineSizeLiters} onChange={(e) => update("engineSizeLiters", Number(e.target.value))} required className="input" />
          </Field>
          <Field label="Owners count">
            <input type="number" min={1} value={form.ownersCount} onChange={(e) => update("ownersCount", Number(e.target.value))} required className="input" />
          </Field>
        </Section>

        <Section title="Condition">
          <Field label="Condition">
            <select value={form.condition} onChange={(e) => update("condition", e.target.value)} className="input">
              {CONDITIONS.map((f) => <option key={f} value={f}>{CONDITION_LABELS[f]}</option>)}
            </select>
          </Field>
          {form.condition === "Damaged" && (
            <Field label="Damage severity (1-10)">
              <input type="number" min={1} max={10} value={form.damageSeverity ?? 1} onChange={(e) => update("damageSeverity", Number(e.target.value))} className="input" />
            </Field>
          )}
          {(form.condition === "Repaired" || form.condition === "Damaged" || form.condition === "Crashed") && (
            <Field label="Repair description" className="sm:col-span-2">
              <textarea value={form.repairDescription ?? ""} onChange={(e) => update("repairDescription", e.target.value)} className="input" rows={2} />
            </Field>
          )}
        </Section>

        <Section title="Price">
          <Field label="Price">
            <input type="number" value={form.price} onChange={(e) => update("price", Number(e.target.value))} required className="input" />
          </Field>
          <Field label="Currency">
            <select value={form.currencyId} onChange={(e) => update("currencyId", Number(e.target.value))} className="input">
              {currencies.map((c) => <option key={c.id} value={c.id}>{c.code}</option>)}
            </select>
          </Field>
        </Section>

        <Section title="Description">
          <Field label="Description" className="sm:col-span-2">
            <textarea value={form.description} onChange={(e) => update("description", e.target.value)} required className="input" rows={5} />
          </Field>
        </Section>

        <Section title="Contact details (leave blank to use your profile defaults)">
          <Field label="Display name">
            <input value={form.sellerFullName ?? ""} onChange={(e) => update("sellerFullName", e.target.value)} className="input" />
          </Field>
          <Field label="City">
            <input value={form.sellerCity ?? ""} onChange={(e) => update("sellerCity", e.target.value)} className="input" />
          </Field>
          <Field label="Email">
            <input value={form.sellerEmail ?? ""} onChange={(e) => update("sellerEmail", e.target.value)} className="input" />
          </Field>
          <Field label="Phone">
            <input value={form.sellerPhone ?? ""} onChange={(e) => update("sellerPhone", e.target.value)} className="input" />
          </Field>
        </Section>

        <Section title="Features">
          <div className="sm:col-span-2 space-y-4">
            {categories.map((cat) => (
              <div key={cat.id}>
                <p className="text-sm font-semibold text-ink-300 mb-2">{cat.name}</p>
                <div className="flex flex-wrap gap-2">
                  {cat.features.map((f) => (
                    <button
                      key={f.id}
                      type="button"
                      onClick={() => toggleFeature(f.id)}
                      className={`text-xs px-2.5 py-1 rounded-full border transition-colors ${
                        form.featureIds.includes(f.id) ? "bg-amber-500 text-base-950 border-amber-500 font-medium" : "bg-base-800 border-base-700 text-ink-300 hover:border-amber-500/50"
                      }`}
                    >
                      {f.name}
                    </button>
                  ))}
                </div>
              </div>
            ))}
          </div>
        </Section>

        <Section title="Photos & videos">
          <div className="sm:col-span-2 space-y-4">
            <div>
              <p className="text-sm font-medium text-ink-300 mb-2">Photos (up to 15)</p>
              <div className="flex flex-wrap gap-2">
                {existingImages.map((img) => (
                  <div key={img.path} className="relative w-20 h-16 rounded-lg overflow-hidden border border-base-700">
                    <img src={fileUrl(img.path)} className="w-full h-full object-cover" />
                    <button type="button" onClick={() => removeExistingImage(img)} className="absolute top-0.5 right-0.5 bg-base-950/80 rounded-full p-0.5">
                      <X size={12} />
                    </button>
                  </div>
                ))}
                {newImages.map((file, i) => (
                  <div key={i} className="relative w-20 h-16 rounded-lg overflow-hidden border border-base-700">
                    <img src={URL.createObjectURL(file)} className="w-full h-full object-cover" />
                    <button type="button" onClick={() => setNewImages((p) => p.filter((_, idx) => idx !== i))} className="absolute top-0.5 right-0.5 bg-base-950/80 rounded-full p-0.5">
                      <X size={12} />
                    </button>
                  </div>
                ))}
                <label className="w-20 h-16 rounded-lg border border-dashed border-base-600 flex items-center justify-center cursor-pointer hover:border-amber-500/50">
                  <Upload size={16} className="text-ink-500" />
                  <input type="file" accept="image/*" multiple className="hidden" onChange={(e) => setNewImages((p) => [...p, ...Array.from(e.target.files ?? [])])} />
                </label>
              </div>
            </div>

            <div>
              <p className="text-sm font-medium text-ink-300 mb-2">Videos (up to 5, max 300MB each)</p>
              <div className="flex flex-wrap gap-2">
                {existingVideos.map((vid) => (
                  <div key={vid.path} className="relative w-20 h-16 rounded-lg overflow-hidden border border-base-700 bg-base-800 flex items-center justify-center">
                    <Play size={16} />
                  </div>
                ))}
                {newVideos.map((file, i) => (
                  <div key={i} className="relative w-20 h-16 rounded-lg overflow-hidden border border-base-700 bg-base-800 flex items-center justify-center">
                    <Play size={16} />
                    <button type="button" onClick={() => setNewVideos((p) => p.filter((_, idx) => idx !== i))} className="absolute top-0.5 right-0.5 bg-base-950/80 rounded-full p-0.5">
                      <X size={12} />
                    </button>
                  </div>
                ))}
                <label className="w-20 h-16 rounded-lg border border-dashed border-base-600 flex items-center justify-center cursor-pointer hover:border-amber-500/50">
                  <Upload size={16} className="text-ink-500" />
                  <input type="file" accept="video/*" multiple className="hidden" onChange={(e) => setNewVideos((p) => [...p, ...Array.from(e.target.files ?? [])])} />
                </label>
              </div>
            </div>
          </div>
        </Section>

        <button type="submit" disabled={saving} className="w-full bg-amber-500 hover:bg-amber-400 disabled:opacity-60 text-base-950 font-semibold py-3 rounded-lg flex items-center justify-center gap-2 transition-colors">
          {saving && <Loader2 size={16} className="animate-spin" />}
          {isEdit ? "Save changes" : "Publish advert"}
        </button>
      </form>
    </div>
  );
}

function Section({ title, children }: { title: string; children: React.ReactNode }) {
  return (
    <div className="bg-base-900 border border-base-700 rounded-xl p-5">
      <h2 className="font-display font-semibold text-lg mb-4">{title}</h2>
      <div className="grid sm:grid-cols-2 gap-4">{children}</div>
    </div>
  );
}

function Field({ label, className = "", children }: { label: string; className?: string; children: React.ReactNode }) {
  return (
    <div className={className}>
      <label className="text-sm font-medium text-ink-300 block mb-1.5">{label}</label>
      {children}
    </div>
  );
}
