using System.Text.Json.Serialization;

// ReSharper disable ClassNeverInstantiated.Global

namespace SteamWebAuthenticator.Models;

public class SendConfirmationResponse
{
    [JsonPropertyName("success")] 
    public bool Success { get; set; }
}
