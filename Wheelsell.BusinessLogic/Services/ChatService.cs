using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Wheelsell.BusinessLogic.DTOs.Chat;
using Wheelsell.BusinessLogic.DTOs.Common;
using Wheelsell.DataAccess.Entities;
using Wheelsell.DataAccess.Enums;
using Wheelsell.DataAccess.Repositories;

namespace Wheelsell.BusinessLogic.Services;

public interface IChatService
{
    Task<List<ConversationDto>> GetConversationsAsync(int userId);
    Task<ServiceResult<List<MessageDto>>> GetMessagesAsync(int conversationId, int userId);
    Task<ServiceResult<ConversationDto>> StartConversationAsync(int buyerId, StartConversationRequest request);
    Task<ServiceResult<MessageDto>> SendMessageAsync(int senderId, SendMessageRequest request);
    Task<ServiceResult> MarkConversationReadAsync(int conversationId, int userId);
}

public class ChatService : IChatService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly INotificationService _notificationService;
    private readonly IEmailService _emailService;

    public ChatService(IUnitOfWork uow, IMapper mapper, INotificationService notificationService, IEmailService emailService)
    {
        _uow = uow;
        _mapper = mapper;
        _notificationService = notificationService;
        _emailService = emailService;
    }

    public async Task<List<ConversationDto>> GetConversationsAsync(int userId)
    {
        var conversations = await _uow.Conversations.Query()
            .Where(c => c.BuyerId == userId || c.SellerId == userId)
            .Include(c => c.Advert).ThenInclude(a => a.Images)
            .Include(c => c.Buyer)
            .Include(c => c.Seller)
            .Include(c => c.Messages)
            .ToListAsync();

        var result = conversations.Select(c =>
        {
            var otherIsBuyer = c.SellerId == userId;
            var other = otherIsBuyer ? c.Buyer : c.Seller;
            var lastMessage = c.Messages.OrderByDescending(m => m.CreatedAt).FirstOrDefault();

            return new ConversationDto
            {
                Id = c.Id,
                AdvertId = c.AdvertId,
                AdvertTitle = c.Advert.Title,
                AdvertThumbnailPath = c.Advert.Images.OrderBy(i => i.Order).FirstOrDefault()?.Path,
                OtherUserId = other.Id,
                OtherUsername = other.Username,
                OtherUserProfilePhotoPath = other.ProfilePhotoPath,
                LastMessage = lastMessage?.Content,
                LastMessageAt = lastMessage?.CreatedAt,
                UnreadCount = c.Messages.Count(m => m.SenderId != userId && !m.IsRead)
            };
        })
        .OrderByDescending(c => c.LastMessageAt ?? DateTime.MinValue)
        .ToList();

        return result;
    }

    public async Task<ServiceResult<List<MessageDto>>> GetMessagesAsync(int conversationId, int userId)
    {
        var conversation = await _uow.Conversations.GetByIdAsync(conversationId);
        if (conversation is null || (conversation.BuyerId != userId && conversation.SellerId != userId))
        {
            return ServiceResult<List<MessageDto>>.Fail("Conversation not found");
        }

        var messages = await _uow.Messages.Query()
            .Where(m => m.ConversationId == conversationId)
            .Include(m => m.Sender)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync();

        return ServiceResult<List<MessageDto>>.Ok(_mapper.Map<List<MessageDto>>(messages));
    }

    public async Task<ServiceResult<ConversationDto>> StartConversationAsync(int buyerId, StartConversationRequest request)
    {
        var advert = await _uow.Adverts.Query()
            .Include(a => a.Seller)
            .Include(a => a.Images)
            .FirstOrDefaultAsync(a => a.Id == request.AdvertId);

        if (advert is null)
        {
            return ServiceResult<ConversationDto>.Fail("Advert not found");
        }

        if (advert.SellerId == buyerId)
        {
            return ServiceResult<ConversationDto>.Fail("You cannot start a conversation about your own advert");
        }

        var conversation = await _uow.Conversations.Query()
            .Include(c => c.Messages)
            .FirstOrDefaultAsync(c => c.AdvertId == request.AdvertId && c.BuyerId == buyerId);

        var isNewConversation = conversation is null;

        if (conversation is null)
        {
            conversation = new Conversation
            {
                AdvertId = request.AdvertId,
                BuyerId = buyerId,
                SellerId = advert.SellerId
            };
            await _uow.Conversations.AddAsync(conversation);
            await _uow.SaveChangesAsync();
        }

        var buyer = await _uow.Users.GetByIdAsync(buyerId);

        var message = new Message
        {
            ConversationId = conversation.Id,
            SenderId = buyerId,
            Content = request.Content
        };
        await _uow.Messages.AddAsync(message);
        await _uow.SaveChangesAsync();

        await _notificationService.CreateAsync(
            advert.SellerId,
            NotificationType.NewMessage,
            $"{buyer!.Username} sent you a message about \"{advert.Title}\"",
            advert.Id,
            conversation.Id);

        // Email is optional — don't crash if SMTP isn't configured
        if (isNewConversation)
        {
            try
            {
                await _emailService.SendNewMessageNotificationAsync(
                    advert.Seller.Email, advert.Seller.Username, advert.Title, buyer.Username);
            }
            catch
            {
                // Email failure must not block the message send
            }
        }

        return ServiceResult<ConversationDto>.Ok(new ConversationDto
        {
            Id = conversation.Id,
            AdvertId = advert.Id,
            AdvertTitle = advert.Title,
            AdvertThumbnailPath = advert.Images.OrderBy(i => i.Order).FirstOrDefault()?.Path,
            OtherUserId = advert.SellerId,
            OtherUsername = advert.Seller.Username,
            OtherUserProfilePhotoPath = advert.Seller.ProfilePhotoPath,
            LastMessage = message.Content,
            LastMessageAt = message.CreatedAt,
            UnreadCount = 0
        });
    }

    public async Task<ServiceResult<MessageDto>> SendMessageAsync(int senderId, SendMessageRequest request)
    {
        var conversation = await _uow.Conversations.Query()
            .Include(c => c.Advert)
            .Include(c => c.Buyer)
            .Include(c => c.Seller)
            .FirstOrDefaultAsync(c => c.Id == request.ConversationId);

        if (conversation is null || (conversation.BuyerId != senderId && conversation.SellerId != senderId))
        {
            return ServiceResult<MessageDto>.Fail("Conversation not found");
        }

        var message = new Message
        {
            ConversationId = conversation.Id,
            SenderId = senderId,
            Content = request.Content
        };

        await _uow.Messages.AddAsync(message);
        await _uow.SaveChangesAsync();

        var recipientId = conversation.BuyerId == senderId ? conversation.SellerId : conversation.BuyerId;
        var sender = conversation.BuyerId == senderId ? conversation.Buyer : conversation.Seller;

        await _notificationService.CreateAsync(
            recipientId,
            NotificationType.NewMessage,
            $"{sender.Username} sent you a message about \"{conversation.Advert.Title}\"",
            conversation.AdvertId,
            conversation.Id);

        var loaded = await _uow.Messages.Query().Include(m => m.Sender).FirstAsync(m => m.Id == message.Id);
        return ServiceResult<MessageDto>.Ok(_mapper.Map<MessageDto>(loaded));
    }

    public async Task<ServiceResult> MarkConversationReadAsync(int conversationId, int userId)
    {
        var conversation = await _uow.Conversations.GetByIdAsync(conversationId);
        if (conversation is null || (conversation.BuyerId != userId && conversation.SellerId != userId))
        {
            return ServiceResult.Fail("Conversation not found");
        }

        var unread = await _uow.Messages.Query()
            .Where(m => m.ConversationId == conversationId && m.SenderId != userId && !m.IsRead)
            .ToListAsync();

        foreach (var message in unread)
        {
            message.IsRead = true;
            _uow.Messages.Update(message);
        }

        await _uow.SaveChangesAsync();
        return ServiceResult.Ok();
    }
}
