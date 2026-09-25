using HealthPlus.Application.Common;
using HealthPlus.Application.DTOs.AiChat;
using HealthPlus.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthPlus.Controllers;

[Authorize]
public class AiChatController : BaseApiController
{
    private readonly IAiChatService _service;
    public AiChatController(IAiChatService service) => _service = service;

    [HttpPost("messages")]
    public async Task<IActionResult> SendMessage([FromBody] SendAiMessageRequest request, CancellationToken ct)
    {
        try
        {
            var result = await _service.SendMessageAsync(CurrentUserId, request, ct);
            return Ok(ApiResponse<AiChatResponse>.Ok(result));
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse.Fail(ex.Message)); }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, ApiResponse.Fail(ex.Message)); }
        catch (InvalidOperationException ex) { return BadRequest(ApiResponse.Fail(ex.Message)); }
    }

    [HttpGet("conversations")]
    public async Task<IActionResult> GetConversations(CancellationToken ct)
    {
        var result = await _service.GetMyConversationsAsync(CurrentUserId, ct);
        return Ok(ApiResponse<IEnumerable<AiConversationSummaryResponse>>.Ok(result));
    }

    [HttpGet("conversations/{id:guid}/messages")]
    public async Task<IActionResult> GetMessages(Guid id, CancellationToken ct)
    {
        try
        {
            var result = await _service.GetMessagesAsync(CurrentUserId, id, ct);
            return Ok(ApiResponse<IEnumerable<AiMessageResponse>>.Ok(result));
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse.Fail(ex.Message)); }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, ApiResponse.Fail(ex.Message)); }
    }
}
