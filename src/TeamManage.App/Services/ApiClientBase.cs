using System.Net.Http.Json;

namespace TeamManage.App.Services;

/// <summary>
/// Shared helpers for typed API client services: consistent error handling
/// and JSON (de)serialization against the TeamManage.Api backend.
/// </summary>
public abstract class ApiClientBase
{
    protected readonly HttpClient Http;

    protected ApiClientBase(HttpClient http)
    {
        Http = http;
    }

    protected static async Task EnsureSuccessAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        string message = $"Request failed with status {(int)response.StatusCode}.";
        try
        {
            var problem = await response.Content.ReadFromJsonAsync<ErrorPayload>();
            if (!string.IsNullOrWhiteSpace(problem?.Error))
            {
                message = problem.Error;
            }
            else if (!string.IsNullOrWhiteSpace(problem?.Detail))
            {
                message = problem.Detail;
            }
        }
        catch
        {
            // Response body wasn't JSON in the expected shape; fall back to the generic message.
        }

        throw new ApiException(message);
    }

    private class ErrorPayload
    {
        public string? Error { get; set; }

        public string? Detail { get; set; }
    }
}
