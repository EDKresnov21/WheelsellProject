using Microsoft.Extensions.DependencyInjection;
using Wheelsell.BusinessLogic.Mapping;
using Wheelsell.DataAccess.Repositories;

namespace Wheelsell.BusinessLogic.Services;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBusinessLogic(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(MappingProfile));

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ILookupService, LookupService>();
        services.AddScoped<IAdvertService, AdvertService>();
        services.AddScoped<IChatService, ChatService>();
        services.AddScoped<IReviewService, ReviewService>();
        services.AddScoped<IFavoriteService, FavoriteService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IAdminService, AdminService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IFileStorageService, FileStorageService>();

        return services;
    }
}
