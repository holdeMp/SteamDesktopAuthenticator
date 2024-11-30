using System.Text.Json.Serialization;
// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global

namespace SteamWebAuthenticator.Models;

// ReSharper disable once ClassNeverInstantiated.Global
public class SteamAccessToken
{
    [JsonPropertyName("exp")]
    public long Exp { get; set; }
}