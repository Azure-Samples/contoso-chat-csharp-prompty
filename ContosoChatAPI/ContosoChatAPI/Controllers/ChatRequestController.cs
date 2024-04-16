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


        [HttpGet(Name = "GetChatRequest")]
        public string Get(string customerId, string question, string chatHistory)
        {
            var chatService = new ChatService();
            string result = chatService.GetResponseAsync(customerId, question, chatHistory.Split(',').ToList()).Result;
            Console.WriteLine(result);
            return result;
        }
    }
}
