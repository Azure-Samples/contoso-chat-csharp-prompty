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
        private readonly ChatService chatService;

        public ContosoChatController(ILogger<ContosoChatController> logger, ChatService chatService)
    {
      _logger = logger;
            this.chatService = chatService;
        }


    [HttpPost(Name = "PostChatRequest")]
    public async Task<string> Post(string customerId, string question, List<string> chatHistory)
    {
      string result = await chatService.GetResponseAsync(customerId, question, chatHistory.ToList());
      _logger.LogInformation(result);
      return result;
    }
  }
}
