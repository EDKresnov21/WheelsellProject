using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wheelsell.BusinessLogic.DTOs.Chat;
using Wheelsell.BusinessLogic.Services;

namespace Wheelsell.Control.Controllers;

[Authorize]
[Route("api/chat")]
public class ChatController : ApiControllerBase
{
    private readonly IChatService _chatService;

    public ChatController(IChatService chatService)
    {
        _chatService = chatService;
    }

    [HttpGet("conversations")]
    public async Task<IActionResult> GetConversations()
    {
        return Ok(await _chatService.GetConversationsAsync(CurrentUserId));
    }

    [HttpGet("conversations/{id:int}/messages")]
    public async Task<IActionResult> GetMessages(int id)
    {
        var result = await _chatService.GetMessagesAsync(id, CurrentUserId);
        return HandleResult(result);
    }

    [HttpPost("conversations")]
    public async Task<IActionResult> StartConversation(StartConversationRequest request)
    {
        var result = await _chatService.StartConversationAsync(CurrentUserId, request);
        return HandleResult(result);
    }

    [HttpPost("conversations/{id:int}/messages")]
    public async Task<IActionResult> SendMessage(int id, [FromBody] SendMessageRequest request)
    {
        request.ConversationId = id;
        var result = await _chatService.SendMessageAsync(CurrentUserId, request);
        return HandleResult(result);
    }

    [HttpPost("conversations/{id:int}/read")]
    public async Task<IActionResult> MarkRead(int id)
    {
        var result = await _chatService.MarkConversationReadAsync(id, CurrentUserId);
        return HandleResult(result);
    }
}
