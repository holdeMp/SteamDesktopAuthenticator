using System.Text.Json.Serialization;
using SteamWebAuthenticator.Models;

namespace SteamAuth;

public class ConfirmationsResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; }

    [JsonPropertyName("needauth")]
    public bool NeedAuthentication { get; set; }

    [JsonPropertyName("conf")]
    public Confirmation[] Confirmations { get; set; }
}