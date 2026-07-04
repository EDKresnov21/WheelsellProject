using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Wheelsell.BusinessLogic.DTOs.Common;
using Wheelsell.BusinessLogic.DTOs.Notifications;
using Wheelsell.DataAccess.Entities;
using Wheelsell.DataAccess.Enums;
using Wheelsell.DataAccess.Repositories;

namespace Wheelsell.BusinessLogic.Services;

public interface INotificationService
{
    Task<List<NotificationDto>> GetForUserAsync(int userId);
    Task<int> GetUnreadCountAsync(int userId);
    Task<ServiceResult> MarkAsReadAsync(int userId, int notificationId);
    Task<ServiceResult> MarkAllAsReadAsync(int userId);
    Task<Notification> CreateAsync(int userId, NotificationType type, string message, int? relatedAdvertId = null, int? relatedConversationId = null);
}

public class NotificationService : INotificationService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public NotificationService(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<List<NotificationDto>> GetForUserAsync(int userId)
    {
        var notifications = await _uow.Notifications.Query()
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(50)
            .ToListAsync();

        return _mapper.Map<List<NotificationDto>>(notifications);
    }

    public async Task<int> GetUnreadCountAsync(int userId)
    {
        return await _uow.Notifications.Query().CountAsync(n => n.UserId == userId && !n.IsRead);
    }

    public async Task<ServiceResult> MarkAsReadAsync(int userId, int notificationId)
    {
        var notification = await _uow.Notifications.GetByIdAsync(notificationId);
        if (notification is null || notification.UserId != userId)
        {
            return ServiceResult.Fail("Notification not found");
        }

        notification.IsRead = true;
        _uow.Notifications.Update(notification);
        await _uow.SaveChangesAsync();
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> MarkAllAsReadAsync(int userId)
    {
        var notifications = await _uow.Notifications.Query()
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync();

        foreach (var notification in notifications)
        {
            notification.IsRead = true;
            _uow.Notifications.Update(notification);
        }

        await _uow.SaveChangesAsync();
        return ServiceResult.Ok();
    }

    public async Task<Notification> CreateAsync(int userId, NotificationType type, string message, int? relatedAdvertId = null, int? relatedConversationId = null)
    {
        var notification = new Notification
        {
            UserId = userId,
            Type = type,
            Message = message,
            RelatedAdvertId = relatedAdvertId,
            RelatedConversationId = relatedConversationId
        };

        await _uow.Notifications.AddAsync(notification);
        await _uow.SaveChangesAsync();
        return notification;
    }
}
