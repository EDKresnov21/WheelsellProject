using Wheelsell.DataAccess.Entities;

namespace Wheelsell.DataAccess.Repositories;

public interface IUnitOfWork
{
    IRepository<User> Users { get; }
    IRepository<RefreshToken> RefreshTokens { get; }
    IRepository<EmailConfirmationToken> EmailConfirmationTokens { get; }
    IRepository<PasswordResetToken> PasswordResetTokens { get; }
    IRepository<Brand> Brands { get; }
    IRepository<CarModel> CarModels { get; }
    IRepository<Currency> Currencies { get; }
    IRepository<FeatureCategory> FeatureCategories { get; }
    IRepository<Feature> Features { get; }
    IRepository<Advert> Adverts { get; }
    IRepository<AdvertImage> AdvertImages { get; }
    IRepository<AdvertVideo> AdvertVideos { get; }
    IRepository<Conversation> Conversations { get; }
    IRepository<Message> Messages { get; }
    IRepository<Review> Reviews { get; }
    IRepository<Favorite> Favorites { get; }
    IRepository<Notification> Notifications { get; }
    WheelsellDbContext Context { get; }
    Task SaveChangesAsync();
}

public class UnitOfWork : IUnitOfWork
{
    public WheelsellDbContext Context { get; }

    public IRepository<User> Users { get; }
    public IRepository<RefreshToken> RefreshTokens { get; }
    public IRepository<EmailConfirmationToken> EmailConfirmationTokens { get; }
    public IRepository<PasswordResetToken> PasswordResetTokens { get; }
    public IRepository<Brand> Brands { get; }
    public IRepository<CarModel> CarModels { get; }
    public IRepository<Currency> Currencies { get; }
    public IRepository<FeatureCategory> FeatureCategories { get; }
    public IRepository<Feature> Features { get; }
    public IRepository<Advert> Adverts { get; }
    public IRepository<AdvertImage> AdvertImages { get; }
    public IRepository<AdvertVideo> AdvertVideos { get; }
    public IRepository<Conversation> Conversations { get; }
    public IRepository<Message> Messages { get; }
    public IRepository<Review> Reviews { get; }
    public IRepository<Favorite> Favorites { get; }
    public IRepository<Notification> Notifications { get; }

    public UnitOfWork(WheelsellDbContext context)
    {
        Context = context;
        Users = new Repository<User>(context);
        RefreshTokens = new Repository<RefreshToken>(context);
        EmailConfirmationTokens = new Repository<EmailConfirmationToken>(context);
        PasswordResetTokens = new Repository<PasswordResetToken>(context);
        Brands = new Repository<Brand>(context);
        CarModels = new Repository<CarModel>(context);
        Currencies = new Repository<Currency>(context);
        FeatureCategories = new Repository<FeatureCategory>(context);
        Features = new Repository<Feature>(context);
        Adverts = new Repository<Advert>(context);
        AdvertImages = new Repository<AdvertImage>(context);
        AdvertVideos = new Repository<AdvertVideo>(context);
        Conversations = new Repository<Conversation>(context);
        Messages = new Repository<Message>(context);
        Reviews = new Repository<Review>(context);
        Favorites = new Repository<Favorite>(context);
        Notifications = new Repository<Notification>(context);
    }

    public async Task SaveChangesAsync() => await Context.SaveChangesAsync();
}
