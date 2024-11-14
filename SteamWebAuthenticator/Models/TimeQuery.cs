using System.Text.Json.Serialization;
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace SteamAuth.Models;

// ReSharper disable once ClassNeverInstantiated.Global
public class TimeQuery
{
    [JsonPropertyName("response")] 
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public TimeQueryResponse Response { get; set; }

    // ReSharper disable once ClassNeverInstantiated.Global
    public class TimeQueryResponse
    {
        [JsonPropertyName("server_time")] 
        public string ServerTime { get; set; }
    }
}