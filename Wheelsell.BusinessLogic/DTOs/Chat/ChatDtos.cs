namespace Wheelsell.BusinessLogic.DTOs.Chat;

public class ConversationDto
{
    public int Id { get; set; }
    public int AdvertId { get; set; }
    public string AdvertTitle { get; set; } = string.Empty;
    public string? AdvertThumbnailPath { get; set; }
    public int OtherUserId { get; set; }
    public string OtherUsername { get; set; } = string.Empty;
    public string? OtherUserProfilePhotoPath { get; set; }
    public string? LastMessage { get; set; }
    public DateTime? LastMessageAt { get; set; }
    public int UnreadCount { get; set; }
}

public class MessageDto
{
    public int Id { get; set; }
    public int ConversationId { get; set; }
    public int SenderId { get; set; }
    public string SenderUsername { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class StartConversationRequest
{
    public int AdvertId { get; set; }
    public string Content { get; set; } = string.Empty;
}

public class SendMessageRequest
{
    public int ConversationId { get; set; }
    public string Content { get; set; } = string.Empty;
}
