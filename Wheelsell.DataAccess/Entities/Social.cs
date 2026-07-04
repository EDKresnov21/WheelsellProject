using Wheelsell.DataAccess.Enums;

namespace Wheelsell.DataAccess.Entities;

public class Conversation : BaseEntity
{
    public int AdvertId { get; set; }
    public Advert Advert { get; set; } = null!;
    public int BuyerId { get; set; }
    public User Buyer { get; set; } = null!;
    public int SellerId { get; set; }
    public User Seller { get; set; } = null!;

    public ICollection<Message> Messages { get; set; } = new List<Message>();
}

public class Message : BaseEntity
{
    public int ConversationId { get; set; }
    public Conversation Conversation { get; set; } = null!;
    public int SenderId { get; set; }
    public User Sender { get; set; } = null!;
    public string Content { get; set; } = string.Empty;
    public bool IsRead { get; set; }
}

public class Review : BaseEntity
{
    public int AdvertId { get; set; }
    public Advert Advert { get; set; } = null!;
    public int ReviewerId { get; set; }
    public User Reviewer { get; set; } = null!;
    public int RevieweeId { get; set; }
    public User Reviewee { get; set; } = null!;
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
}

public class Favorite : BaseEntity
{
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public int AdvertId { get; set; }
    public Advert Advert { get; set; } = null!;
}

public class Notification : BaseEntity
{
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public NotificationType Type { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public int? RelatedAdvertId { get; set; }
    public int? RelatedConversationId { get; set; }
}
