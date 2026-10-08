using System.Text.Json;

namespace TeamManage.App.Services;

/// <summary>
/// Simple JSON file-based cache under the app's local data directory, used so
/// teams/matches lists remain viewable when the device is offline. Not intended
/// for sensitive data - only non-secret API response payloads should be cached here.
/// </summary>
public interface ILocalCacheService
{
    Task SaveAsync<T>(string key, T data);

    Task<T?> LoadAsync<T>(string key);
}

public class LocalCacheService : ILocalCacheService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task SaveAsync<T>(string key, T data)
    {
        try
        {
            await using var stream = File.Create(GetFilePath(key));
            await JsonSerializer.SerializeAsync(stream, data, JsonOptions);
        }
        catch
        {
            // Caching is a best-effort convenience; failures here should never
            // interrupt the primary network flow.
        }
    }

    public async Task<T?> LoadAsync<T>(string key)
    {
        var path = GetFilePath(key);
        if (!File.Exists(path))
        {
            return default;
        }

        try
        {
            await using var stream = File.OpenRead(path);
            return await JsonSerializer.DeserializeAsync<T>(stream, JsonOptions);
        }
        catch
        {
            return default;
        }
    }

    private static string GetFilePath(string key) =>
        Path.Combine(FileSystem.AppDataDirectory, $"cache_{key}.json");
}
