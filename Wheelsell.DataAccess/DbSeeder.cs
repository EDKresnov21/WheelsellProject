using Microsoft.EntityFrameworkCore;
using Wheelsell.DataAccess.Entities;

namespace Wheelsell.DataAccess;

public static class DbSeeder
{
    public static void Seed(ModelBuilder modelBuilder)
    {
        var currencies = new List<Currency>
        {
            new() { Id = 1, Code = "EUR", Symbol = "€", Name = "Euro" },
            new() { Id = 2, Code = "USD", Symbol = "$", Name = "US Dollar" },
            new() { Id = 3, Code = "GBP", Symbol = "£", Name = "British Pound" },
            new() { Id = 4, Code = "RUB", Symbol = "₽", Name = "Russian Ruble" },
            new() { Id = 5, Code = "UAH", Symbol = "₴", Name = "Ukrainian Hryvnia" },
            new() { Id = 6, Code = "CNY", Symbol = "¥", Name = "Chinese Yuan" }
        };
        modelBuilder.Entity<Currency>().HasData(currencies);

        var brands = new List<Brand>
        {
            new() { Id = 1, Name = "Volkswagen" },
            new() { Id = 2, Name = "BMW" },
            new() { Id = 3, Name = "Mercedes-Benz" },
            new() { Id = 4, Name = "Audi" },
            new() { Id = 5, Name = "Toyota" },
            new() { Id = 6, Name = "Ford" },
            new() { Id = 7, Name = "Opel" },
            new() { Id = 8, Name = "Renault" },
            new() { Id = 9, Name = "Peugeot" },
            new() { Id = 10, Name = "Skoda" }
        };
        modelBuilder.Entity<Brand>().HasData(brands);

        var models = new List<CarModel>
        {
            new() { Id = 1, BrandId = 1, Name = "Golf" },
            new() { Id = 2, BrandId = 1, Name = "Passat" },
            new() { Id = 3, BrandId = 1, Name = "Tiguan" },
            new() { Id = 4, BrandId = 2, Name = "3 Series" },
            new() { Id = 5, BrandId = 2, Name = "5 Series" },
            new() { Id = 6, BrandId = 2, Name = "X5" },
            new() { Id = 7, BrandId = 3, Name = "C-Class" },
            new() { Id = 8, BrandId = 3, Name = "E-Class" },
            new() { Id = 9, BrandId = 3, Name = "GLE" },
            new() { Id = 10, BrandId = 4, Name = "A4" },
            new() { Id = 11, BrandId = 4, Name = "A6" },
            new() { Id = 12, BrandId = 4, Name = "Q5" },
            new() { Id = 13, BrandId = 5, Name = "Corolla" },
            new() { Id = 14, BrandId = 5, Name = "RAV4" },
            new() { Id = 15, BrandId = 6, Name = "Focus" },
            new() { Id = 16, BrandId = 6, Name = "Kuga" },
            new() { Id = 17, BrandId = 7, Name = "Astra" },
            new() { Id = 18, BrandId = 7, Name = "Insignia" },
            new() { Id = 19, BrandId = 8, Name = "Megane" },
            new() { Id = 20, BrandId = 8, Name = "Clio" },
            new() { Id = 21, BrandId = 9, Name = "308" },
            new() { Id = 22, BrandId = 9, Name = "3008" },
            new() { Id = 23, BrandId = 10, Name = "Octavia" },
            new() { Id = 24, BrandId = 10, Name = "Superb" }
        };
        modelBuilder.Entity<CarModel>().HasData(models);

        var categories = new List<FeatureCategory>
        {
            new() { Id = 1, Name = "Safety", Order = 1 },
            new() { Id = 2, Name = "Comfort", Order = 2 },
            new() { Id = 3, Name = "Other", Order = 3 },
            new() { Id = 4, Name = "Protection", Order = 4 },
            new() { Id = 5, Name = "Interior", Order = 5 },
            new() { Id = 6, Name = "Exterior", Order = 6 },
            new() { Id = 7, Name = "Specialized", Order = 7 }
        };
        modelBuilder.Entity<FeatureCategory>().HasData(categories);

        var features = new List<Feature>();
        var id = 1;

        void AddFeatures(int categoryId, params string[] names)
        {
            foreach (var name in names)
            {
                features.Add(new Feature { Id = id++, FeatureCategoryId = categoryId, Name = name });
            }
        }

        AddFeatures(1,
            "GPS Tracking System",
            "Adaptive Headlights",
            "Anti-lock Braking System",
            "Rear Airbags",
            "Front Airbags",
            "Side Airbags",
            "Electronic Brake-force Distribution",
            "Electronic Stability Program",
            "Tire Pressure Monitoring",
            "Parking Sensors",
            "ISOFIX System",
            "Traction Control System",
            "Anti-slip Regulation",
            "Distance Control System",
            "Descent Control System"
        );

        AddFeatures(2,
            "360 Camera / Rear Camera",
            "Apple CarPlay / Android Auto",
            "Auto Start-Stop Function",
            "Bluetooth / Handsfree System",
            "DVD / TV",
            "Head-up Display",
            "Steptronic / Tiptronic Gearbox",
            "USB / Audio / Video / AUX Inputs",
            "Automatic Tailgate",
            "Adaptive Suspension",
            "Keyless Start",
            "Differential Lock",
            "On-board Computer",
            "Fast / Slow Gear Modes",
            "Seat Ventilation",
            "Light Sensor",
            "Electric Mirrors",
            "Electric Windows",
            "Electric Seat Adjustment",
            "Electric Power Steering",
            "Air Conditioning",
            "Climate Control",
            "Multifunction Steering Wheel",
            "Navigation System",
            "Heated Steering Wheel",
            "Heating",
            "Heated Windshield",
            "Heated Seats",
            "Steering Wheel Adjustment",
            "Rain Sensor",
            "Power Steering",
            "Headlight Washer System"
        );

        AddFeatures(3,
            "4x4",
            "7 Seats",
            "Buy Back Option",
            "Trade-in Possible",
            "LPG System",
            "Long Wheelbase",
            "Reserved / Sold",
            "Has Been in an Accident",
            "Short Wheelbase",
            "Leasing Available",
            "CNG System",
            "For Parts",
            "Fully Serviced",
            "Recently Imported",
            "Registered",
            "Service Book Available",
            "Tuned"
        );

        AddFeatures(4,
            "Offroad Package",
            "Alarm System",
            "Armored",
            "Comprehensive Insurance Included",
            "Winch",
            "Central Locking"
        );

        AddFeatures(5,
            "Velour Interior",
            "Right-hand Drive",
            "Leather Interior",
            "Light Interior"
        );

        AddFeatures(6,
            "2/3 Doors",
            "4/5 Doors",
            "LED Headlights",
            "Xenon Headlights",
            "Alloy Wheels",
            "Metallic Paint",
            "Panoramic Sunroof",
            "Roof Rails",
            "Spoilers",
            "Tow Bar",
            "Halogen Headlights",
            "Running Boards"
        );

        AddFeatures(7,
            "Taxi",
            "Accessible for Disabled",
            "Hearse",
            "Ambulance",
            "Driving School Vehicle",
            "Refrigerated Vehicle",
            "N1 Homologation"
        );

        modelBuilder.Entity<Feature>().HasData(features);
    }
}
