using System.Text.Json;

namespace SteamWebAuthenticator.Helpers;

public static class Serializing
{
    public static async Task<string> ToJsonAsync(this string plainText)
    {
        await using var memoryStream = new MemoryStream();
        await JsonSerializer.SerializeAsync(memoryStream, plainText);
        memoryStream.Seek(0, SeekOrigin.Begin);

        using var reader = new StreamReader(memoryStream);
        return await reader.ReadToEndAsync();
    }
    
    public static async Task<T> FromJsonAsync<T>(this string json)
    {
        using var memoryStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(json));
        var result = await JsonSerializer.DeserializeAsync<T>(memoryStream);
        return result!;
    }
}