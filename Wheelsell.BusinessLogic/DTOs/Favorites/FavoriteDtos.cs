using Wheelsell.BusinessLogic.DTOs.Adverts;

namespace Wheelsell.BusinessLogic.DTOs.Favorites;

public class FavoriteDto
{
    public int Id { get; set; }
    public AdvertSummaryDto Advert { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}
