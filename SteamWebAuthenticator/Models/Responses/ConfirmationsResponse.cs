using System.Text.Json.Serialization;

namespace SteamWebAuthenticator.Models.Responses;

// ReSharper disable once ClassNeverInstantiated.Global
public class ConfirmationsResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("message")] 
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("needauth")]
    public bool NeedAuthentication { get; set; }

    [JsonPropertyName("conf")] 
    public Confirmation[] Confirmations { get; set; } = [];
}