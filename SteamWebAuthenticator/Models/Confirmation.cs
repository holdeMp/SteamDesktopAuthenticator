using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

// ReSharper disable UnusedMember.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

namespace SteamWebAuthenticator.Models
{
    [SuppressMessage("ReSharper", "CollectionNeverUpdated.Global")]
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
    public class Confirmation
    {
        [JsonPropertyName("id")] 
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("nonce")]
        public string Key { get; set; } = string.Empty;

        [JsonPropertyName("creator_id")]
        public string Creator { get; set; } = string.Empty;

        [JsonPropertyName("headline")]
        public string Headline { get; set; } = string.Empty;

        [JsonPropertyName("summary")] 
        public List<string> Summary { get; set; } = [];

        [JsonPropertyName(Constants.Accept)]
        public string Accept { get; set; } = string.Empty;

        [JsonPropertyName(Constants.Cancel)]
        public string Cancel { get; set; } = string.Empty;

        [JsonPropertyName("icon")]
        public string Icon { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        [JsonConverter(typeof(JsonStringEnumConverter ))]
        public EMobileConfirmationType ConfType { get; set; } = EMobileConfirmationType.Invalid;
        
        public bool IsSelected { get; set; }
    }
}
