using ContosoChatAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Prompty.Core;
using System;
using System.Collections.Generic;
using System.IO;

namespace ContosoChatAPI.Controllers
{
  [ApiController]
  [Route("[controller]")]
  public class ContosoChatController : ControllerBase
  {

    private readonly ILogger<ContosoChatController> _logger;

    public ContosoChatController(ILogger<ContosoChatController> logger)
    {
      _logger = logger;
    }


    [HttpPost(Name = "PostChatRequest")]
    public async Task<string> Post(string customerId, string question, List<string> chatHistory)
    {
      var chatService = new ChatService();
      string result = await chatService.GetResponseAsync(customerId, question, chatHistory.ToList());
      Console.WriteLine(result);
      return result;
    }
  }
}
