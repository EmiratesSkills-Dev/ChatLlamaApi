using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;

namespace ChatLlamaApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatAiController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public ChatAiController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClient = httpClientFactory.CreateClient();
            _configuration = configuration;
        }
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] QuestionRequest request)
        {
            var openRouterApiKey = _configuration["OpenRouter:ApiKey"];
            var apiUrl = "https://openrouter.ai/api/v1/chat/completions";

            var body = new
            {
                model = "meta-llama/llama-4-maverick:free", // ou outro modelo free disponível
                messages = new[]
                {
                    new { role = "user", content = request.Question }
                }
            };

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, apiUrl);
            httpRequest.Headers.Add("Authorization", $"Bearer {openRouterApiKey}");
            httpRequest.Headers.Add("HTTP-Referer", "http://localhost"); // seu domínio ou localhost (OBRIGATÓRIO)
            httpRequest.Headers.Add("X-Title", "LlamaApiTest"); // nome do seu projeto (OBRIGATÓRIO)

            httpRequest.Content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(httpRequest);

            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());

            var resultString = await response.Content.ReadAsStringAsync();
            var openRouterResponse = JsonConvert.DeserializeObject<OpenAiResponse>(resultString);

            var answer = openRouterResponse.choices[0].message.content;

            return Ok(new { answer });
        }
    }

    public class QuestionRequest
    {
        public string Question { get; set; }
    }

    public class OpenAiResponse
    {
        public Choice[] choices { get; set; }
    }

    public class Choice
    {
        public Message message { get; set; }
    }

    public class Message
    {
        public string role { get; set; }
        public string content { get; set; }
    }

}
