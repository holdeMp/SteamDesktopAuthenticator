using System.Text.Json.Serialization;
// ReSharper disable ClassNeverInstantiated.Global

namespace SteamAuth;

public class SendConfirmationResponse
{
    [JsonPropertyName("success")] 
    public bool Success { get; set; }
}
