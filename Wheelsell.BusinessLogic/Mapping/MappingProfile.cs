using AutoMapper;
using Wheelsell.BusinessLogic.DTOs.Admin;
using Wheelsell.BusinessLogic.DTOs.Chat;
using Wheelsell.BusinessLogic.DTOs.Lookups;
using Wheelsell.BusinessLogic.DTOs.Notifications;
using Wheelsell.BusinessLogic.DTOs.Reviews;
using Wheelsell.BusinessLogic.DTOs.Users;
using Wheelsell.DataAccess.Entities;

namespace Wheelsell.BusinessLogic.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<User, UserProfileDto>()
            .ForMember(d => d.Role, o => o.MapFrom(s => s.Role.ToString()))
            .ForMember(d => d.AverageRating, o => o.Ignore())
            .ForMember(d => d.ReviewsCount, o => o.Ignore());

        CreateMap<User, AdminUserDto>()
            .ForMember(d => d.Role, o => o.MapFrom(s => s.Role.ToString()));

        CreateMap<Brand, BrandDto>();
        CreateMap<CarModel, CarModelDto>();
        CreateMap<Currency, CurrencyDto>();
        CreateMap<Feature, FeatureDto>();
        CreateMap<FeatureCategory, FeatureCategoryDto>();

        CreateMap<Message, MessageDto>()
            .ForMember(d => d.SenderUsername, o => o.MapFrom(s => s.Sender.Username));

        CreateMap<Review, ReviewDto>()
            .ForMember(d => d.AdvertTitle, o => o.MapFrom(s => s.Advert.Title))
            .ForMember(d => d.ReviewerUsername, o => o.MapFrom(s => s.Reviewer.Username))
            .ForMember(d => d.ReviewerProfilePhotoPath, o => o.MapFrom(s => s.Reviewer.ProfilePhotoPath));

        CreateMap<Notification, NotificationDto>()
            .ForMember(d => d.Type, o => o.MapFrom(s => s.Type.ToString()));
    }
}
