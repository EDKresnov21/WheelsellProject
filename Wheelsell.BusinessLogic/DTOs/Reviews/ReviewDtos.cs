namespace Wheelsell.BusinessLogic.DTOs.Reviews;

public class ReviewDto
{
    public int Id { get; set; }
    public int AdvertId { get; set; }
    public string AdvertTitle { get; set; } = string.Empty;
    public int ReviewerId { get; set; }
    public string ReviewerUsername { get; set; } = string.Empty;
    public string? ReviewerProfilePhotoPath { get; set; }
    public int RevieweeId { get; set; }
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class CreateReviewRequest
{
    public int AdvertId { get; set; }
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
}
