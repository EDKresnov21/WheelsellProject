using Microsoft.EntityFrameworkCore;
using Wheelsell.DataAccess.Entities;

namespace Wheelsell.DataAccess;

public class WheelsellDbContext : DbContext
{
    public WheelsellDbContext(DbContextOptions<WheelsellDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<EmailConfirmationToken> EmailConfirmationTokens => Set<EmailConfirmationToken>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();

    public DbSet<Brand> Brands => Set<Brand>();
    public DbSet<CarModel> CarModels => Set<CarModel>();
    public DbSet<Currency> Currencies => Set<Currency>();
    public DbSet<FeatureCategory> FeatureCategories => Set<FeatureCategory>();
    public DbSet<Feature> Features => Set<Feature>();

    public DbSet<Advert> Adverts => Set<Advert>();
    public DbSet<AdvertImage> AdvertImages => Set<AdvertImage>();
    public DbSet<AdvertVideo> AdvertVideos => Set<AdvertVideo>();
    public DbSet<AdvertFeature> AdvertFeatures => Set<AdvertFeature>();

    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<Favorite> Favorites => Set<Favorite>();
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.Username).IsUnique();
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.Username).HasMaxLength(50);
            entity.Property(u => u.Email).HasMaxLength(255);
            entity.Property(u => u.Name).HasMaxLength(100);
            entity.Property(u => u.Surname).HasMaxLength(100);
            entity.Property(u => u.City).HasMaxLength(100);
            entity.Property(u => u.County).HasMaxLength(100);
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasOne(rt => rt.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<EmailConfirmationToken>(entity =>
        {
            entity.HasOne(t => t.User)
                .WithMany(u => u.EmailConfirmationTokens)
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PasswordResetToken>(entity =>
        {
            entity.HasOne(t => t.User)
                .WithMany(u => u.PasswordResetTokens)
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CarModel>(entity =>
        {
            entity.HasOne(m => m.Brand)
                .WithMany(b => b.Models)
                .HasForeignKey(m => m.BrandId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Feature>(entity =>
        {
            entity.HasOne(f => f.FeatureCategory)
                .WithMany(c => c.Features)
                .HasForeignKey(f => f.FeatureCategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Currency>(entity =>
        {
            entity.HasIndex(c => c.Code).IsUnique();
        });

        modelBuilder.Entity<Advert>(entity =>
        {
            entity.Property(a => a.Price).HasColumnType("decimal(18,2)");
            entity.Property(a => a.EngineSizeLiters).HasColumnType("decimal(4,1)");

            entity.HasOne(a => a.Seller)
                .WithMany(u => u.Adverts)
                .HasForeignKey(a => a.SellerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(a => a.Buyer)
                .WithMany()
                .HasForeignKey(a => a.BuyerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(a => a.Brand)
                .WithMany()
                .HasForeignKey(a => a.BrandId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(a => a.CarModel)
                .WithMany()
                .HasForeignKey(a => a.CarModelId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(a => a.Currency)
                .WithMany()
                .HasForeignKey(a => a.CurrencyId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(a => a.PreviousAdvert)
                .WithMany()
                .HasForeignKey(a => a.PreviousAdvertId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AdvertImage>(entity =>
        {
            entity.HasOne(i => i.Advert)
                .WithMany(a => a.Images)
                .HasForeignKey(i => i.AdvertId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AdvertVideo>(entity =>
        {
            entity.HasOne(v => v.Advert)
                .WithMany(a => a.Videos)
                .HasForeignKey(v => v.AdvertId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AdvertFeature>(entity =>
        {
            entity.HasKey(af => new { af.AdvertId, af.FeatureId });

            entity.HasOne(af => af.Advert)
                .WithMany(a => a.Features)
                .HasForeignKey(af => af.AdvertId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(af => af.Feature)
                .WithMany(f => f.AdvertFeatures)
                .HasForeignKey(af => af.FeatureId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Conversation>(entity =>
        {
            entity.HasOne(c => c.Advert)
                .WithMany(a => a.Conversations)
                .HasForeignKey(c => c.AdvertId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(c => c.Buyer)
                .WithMany()
                .HasForeignKey(c => c.BuyerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(c => c.Seller)
                .WithMany()
                .HasForeignKey(c => c.SellerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasOne(m => m.Conversation)
                .WithMany(c => c.Messages)
                .HasForeignKey(m => m.ConversationId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(m => m.Sender)
                .WithMany()
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasOne(r => r.Advert)
                .WithMany(a => a.Reviews)
                .HasForeignKey(r => r.AdvertId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(r => r.Reviewer)
                .WithMany()
                .HasForeignKey(r => r.ReviewerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(r => r.Reviewee)
                .WithMany()
                .HasForeignKey(r => r.RevieweeId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Favorite>(entity =>
        {
            entity.HasIndex(f => new { f.UserId, f.AdvertId }).IsUnique();

            entity.HasOne(f => f.User)
                .WithMany(u => u.Favorites)
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(f => f.Advert)
                .WithMany(a => a.Favorites)
                .HasForeignKey(f => f.AdvertId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasOne(n => n.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        DbSeeder.Seed(modelBuilder);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .HasQueryFilter(GetSoftDeleteFilter(entityType.ClrType));
            }
        }
    }

    private static System.Linq.Expressions.LambdaExpression GetSoftDeleteFilter(Type type)
    {
        var parameter = System.Linq.Expressions.Expression.Parameter(type, "e");
        var property = System.Linq.Expressions.Expression.Property(parameter, nameof(BaseEntity.IsDeleted));
        var condition = System.Linq.Expressions.Expression.Equal(property, System.Linq.Expressions.Expression.Constant(false));
        return System.Linq.Expressions.Expression.Lambda(condition, parameter);
    }
}
