namespace AIOrchestrator.Support.OllamaClient;

using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using AIOrchestrator.Support.OllamaClient.Types;

public class OllamaClient
{
    private const string BaseUrl = "http://localhost:11434";

    private readonly HttpClient _httpClient = new();

    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public async Task<ApiResponse> RequestAsync(string prompt, string model, Role role = Role.User)
    {
        var requestMessage = GetRequestMessage(
            url: $"{BaseUrl}/api/generate",
            request: new()
            {
                Model = model,
                Prompt = prompt,
                Role = role.ToString(),
                Stream = false,
            }
        );

        return await GetResponseAsync(requestMessage);
    }

    public HttpRequestMessage GetRequestMessage(string url, ApiRequest request)
    {
        var requestBodyJson = JsonSerializer.Serialize(request, _jsonSerializerOptions);
        return new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(requestBodyJson, Encoding.UTF8, "application/json"),
        };
    }

    public async Task<ApiResponse> GetResponseAsync(HttpRequestMessage requestMessage)
    {
        var response = await _httpClient.SendAsync(
            requestMessage,
            HttpCompletionOption.ResponseHeadersRead
        );
        var responseJson = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<ApiResponse>(responseJson, _jsonSerializerOptions)!;
    }
}
