using System.Text.Json.Serialization;
// ReSharper disable ClassNeverInstantiated.Global

namespace SteamAuth;

public class GenerateAccessTokenForAppResponse
{
    [JsonPropertyName("response")]
    public GenerateAccessTokenForAppResponseResponse Response { get; set; }
}

public class GenerateAccessTokenForAppResponseResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; }
}