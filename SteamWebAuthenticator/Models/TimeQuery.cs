using System.Text.Json.Serialization;
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace SteamWebAuthenticator.Models;

// ReSharper disable once ClassNeverInstantiated.Global
public class TimeQuery
{
    [JsonPropertyName("response")]
    public TimeQueryResponse Response { get; set; } = new();

    public class TimeQueryResponse
    {
        [JsonPropertyName("server_time")] 
        public string ServerTime { get; set; } = string.Empty;
    }
}