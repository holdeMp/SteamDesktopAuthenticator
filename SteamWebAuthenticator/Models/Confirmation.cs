using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using SteamAuth;

namespace SteamWebAuthenticator.Models
{
    [SuppressMessage("ReSharper", "CollectionNeverUpdated.Global")]
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
    public class Confirmation
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("nonce")]
        public string Key { get; set; }

        [JsonPropertyName("creator_id")]
        public string Creator { get; set; }

        [JsonPropertyName("headline")]
        public string Headline { get; set; }

        [JsonPropertyName("summary")]
        public List<string> Summary { get; set; }

        [JsonPropertyName(Constants.Accept)]
        public string Accept { get; set; }

        [JsonPropertyName(Constants.Cancel)]
        public string Cancel { get; set; }

        [JsonPropertyName("icon")]
        public string Icon { get; set; }

        [JsonPropertyName("type")]
        [JsonConverter(typeof(JsonStringEnumConverter ))]
        public EMobileConfirmationType ConfType { get; set; } = EMobileConfirmationType.Invalid;
        
        public bool IsSelected { get; set; }
    }
}
