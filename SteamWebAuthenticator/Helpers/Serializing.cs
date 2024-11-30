using System.Text.Json;
using Serilog;
using Serilog.Core;
// ReSharper disable UnusedMember.Global

namespace SteamWebAuthenticator.Helpers;

public static class Serializing
{
    public static string ToJson(this string plainText)
    {
        using var memoryStream = new MemoryStream();
        JsonSerializer.Serialize(memoryStream, plainText);
        memoryStream.Seek(0, SeekOrigin.Begin);

        using var reader = new StreamReader(memoryStream);
        return reader.ReadToEnd();
    }
    
    public static T FromJson<T>(this string json)
    {
        using var memoryStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(json));
        var result = JsonSerializer.Deserialize<T>(memoryStream);
        return result!;
    }
    
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