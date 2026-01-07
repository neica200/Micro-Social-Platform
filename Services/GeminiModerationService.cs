using System.Text;
using System.Text.Json;

namespace Micro_social_app.Services 
{
    public class GeminiModerationService : IAIContentModerationService
    {
        private readonly HttpClient _http;
        private readonly string _apiKey;

        public GeminiModerationService(HttpClient http, IConfiguration config)
        {
            _http = http;
            _apiKey = config["Gemini:ApiKey"] ?? "";
        }

        public async Task<bool> IsContentAllowedAsync(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return true;

            // Prompt-ul tău excelent pentru moderare
            var prompt = $"""
            You are a content moderation classifier.
            Return ONLY one of these exact tokens: ALLOW or BLOCK.

            Text:
            {text}
            """;

            var body = new
            {
                contents = new[]
                {
                    new {
                        parts = new[] { new { text = prompt } }
                    }
                }
            };

            var payload = JsonSerializer.Serialize(body);
            var content = new StringContent(payload, Encoding.UTF8, "application/json");

            var modelName = "gemini-2.5-flash";

            var res = await _http.PostAsync(
                $"https://generativelanguage.googleapis.com/v1beta/models/{modelName}:generateContent?key={_apiKey}",
                content
            );

            if (!res.IsSuccessStatusCode)
            {
                var errorResponse = await res.Content.ReadAsStringAsync();
                Console.WriteLine($"[GEMINI ERROR]: {res.StatusCode} - {errorResponse}");

                if (string.IsNullOrEmpty(_apiKey)) Console.WriteLine("[GEMINI ERROR]: API Key este GOALĂ!");
                return true; // fail-open
            }
            var json = await res.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(json);

            try
            {
                var modelText = doc.RootElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString()?
                    .Trim()
                    .ToUpperInvariant() ?? "";

                if (modelText.StartsWith("BLOCK")) return false;
                if (modelText.StartsWith("ALLOW")) return true;
            }
            catch
            {
            }

            // fallback
            return true;
        }
    }
}

