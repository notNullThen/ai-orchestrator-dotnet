namespace AIOrchestrator.OllamaClient;

using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Types;

internal sealed class OllamaClient
{
    private const string BaseUrl = "http://localhost:11434";

    private readonly HttpClient _httpClient = new();

    public async Task<ApiResponse> RequestAsync(
        string prompt,
        string model,
        Role role = Role.User,
        ApiRequestOptions? options = null
    )
    {
        var requestMessage = GetRequestMessage(
            url: $"{BaseUrl}/api/generate",
            request: new()
            {
                Model = model,
                Prompt = prompt,
                Role = role.ToString(),
                Stream = false,
                Options = options,
            }
        );

        return await GetResponseAsync(requestMessage);
    }

    public static HttpRequestMessage GetRequestMessage(string url, ApiRequest request)
    {
        var requestBodyJson = JsonSerializer.Serialize(request);
        return new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(requestBodyJson, Encoding.UTF8, "application/json"),
        };
    }

    public async Task<ApiResponse> GetResponseAsync(HttpRequestMessage requestMessage)
    {
        HttpResponseMessage response;
        try
        {
            response = await _httpClient.SendAsync(
                requestMessage,
                HttpCompletionOption.ResponseHeadersRead
            );
            response.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException ex)
        {
            if (ex.StatusCode == null)
            {
                throw new Exception("Couldn't connect to Ollama server", ex);
            }

            throw new Exception("Ollama API error", ex);
        }

        var responseJson = await response.Content.ReadAsStringAsync();

        if (string.IsNullOrWhiteSpace(responseJson))
        {
            throw new Exception("Ollama API returned an empty response.");
        }

        try
        {
            return JsonSerializer.Deserialize<ApiResponse>(responseJson)!;
        }
        catch (JsonException ex)
        {
            throw new Exception(
                $"Failed to deserialize Ollama API response. Content: {responseJson}",
                ex
            );
        }
    }
}
