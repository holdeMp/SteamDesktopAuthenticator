using System.Text.Json.Serialization;

namespace SteamAuth;

// ReSharper disable once ClassNeverInstantiated.Global
public class SteamAccessToken
{
    [JsonPropertyName("exp")]
    public long Exp { get; set; }
}