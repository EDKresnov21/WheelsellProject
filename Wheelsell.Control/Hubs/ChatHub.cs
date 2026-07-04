using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using Wheelsell.BusinessLogic.DTOs.Chat;
using Wheelsell.BusinessLogic.Services;

namespace Wheelsell.Control.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly IChatService _chatService;

    public ChatHub(IChatService chatService)
    {
        _chatService = chatService;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();
        await Groups.AddToGroupAsync(Context.ConnectionId, GetUserGroup(userId));
        await base.OnConnectedAsync();
    }

    public async Task SendMessage(SendMessageRequest request)
    {
        var userId = GetUserId();
        var result = await _chatService.SendMessageAsync(userId, request);

        if (!result.Success || result.Data is null)
        {
            return;
        }

        var conversations = await _chatService.GetConversationsAsync(userId);
        var conversation = conversations.FirstOrDefault(c => c.Id == request.ConversationId);
        var recipientId = conversation?.OtherUserId;

        await Clients.Group(GetUserGroup(userId)).SendAsync("MessageReceived", result.Data);

        if (recipientId.HasValue)
        {
            await Clients.Group(GetUserGroup(recipientId.Value)).SendAsync("MessageReceived", result.Data);
        }
    }

    private int GetUserId()
    {
        var claim = Context.User?.FindFirst(ClaimTypes.NameIdentifier);
        return int.Parse(claim!.Value);
    }

    private static string GetUserGroup(int userId) => $"user-{userId}";
}
