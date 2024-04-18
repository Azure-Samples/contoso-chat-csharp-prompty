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
<<<<<<< HEAD
      _logger = logger;
=======

        private readonly ILogger<ContosoChatController> _logger;

        public ContosoChatController(ILogger<ContosoChatController> logger)
        {
            _logger = logger;
        }

        public static async Task<string> RunPrompt(Dictionary<string, dynamic> inputs)
        {
            var prompty = new Prompty.Core.Prompty();
            prompty.Inputs = inputs;
            prompty = await prompty.Execute("chat.prompty", prompty);
            return prompty.ChatResponseMessage.Content;
        }

        [HttpGet(Name = "GetChatRequest")]
        public string Get()
        {
            //var inputs = new Dictionary<string, dynamic>
            //{
            //    { "firstName", firstName },
            //    { "lastName", lastName },
            //    { "question", question }
            //};
            // load chat.json file as new dictionary<string, string>
            var jsonInputs = System.IO.File.ReadAllText("chat.json");
            // convert json to dictionary<string, string>
            var inputs = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(jsonInputs);
            string result = RunPrompt(inputs).Result;
            Console.WriteLine(result);
            return result;
        }
>>>>>>> 675001a (chat updates with static chat.json data)
    }


    [HttpPost(Name = "PostChatRequest")]
    public string Post(string customerId, string question, List<string> chatHistory)
    {
      var chatService = new ChatService();
      string result = chatService.GetResponseAsync(customerId, question, chatHistory.ToList()).Result;
      Console.WriteLine(result);
      return result;
    }
  }
}
