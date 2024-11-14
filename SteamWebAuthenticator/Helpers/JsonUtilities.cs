using System.Text.Json;

namespace SteamWebAuthenticator.Helpers;

public static class JsonUtilities
{
    public static string ToJsonText<T>(this T obj) => JsonSerializer.Serialize(obj);
}