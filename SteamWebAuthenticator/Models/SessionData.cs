using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace SteamWebAuthenticator.Models;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public class SessionData
{
    [JsonPropertyName("SteamID")]
    public ulong SteamId { get; set; }

    public string AccessToken { get; set; }

    public string RefreshToken { get; set; }
        
    [JsonPropertyName("SessionID")] 
    public string SessionId { get; set; }
}