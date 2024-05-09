using ContosoChatAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace ContosoChatAPI.Controllers;

[ApiController]
[Route("[controller]")]
public sealed class ContosoChatController(ILogger<ContosoChatController> logger, ChatService chatService) : ControllerBase
{
    [HttpPost(Name = "PostChatRequest")]
    public async Task<string> Post(string customerId, string question)
    {
        string result = await chatService.GetResponseAsync(customerId, question);
        logger.LogInformation("Result: {Result}", result);
        return result;
    }
}
