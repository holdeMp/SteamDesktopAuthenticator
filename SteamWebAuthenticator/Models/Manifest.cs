using System.Collections.Generic;
using System.Text.Json.Serialization;
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable CollectionNeverUpdated.Global

namespace SteamAuth.Models;

public class Manifest
{
    [JsonPropertyName("entries")] 
    public List<ManifestEntry> Entries { get; set; } = [];

    [JsonPropertyName("periodic_checking")]
    public bool PeriodicChecking { get; set; }

    [JsonPropertyName("periodic_checking_interval")]
    public int PeriodicCheckingInterval { get; set; } = 5;

    [JsonPropertyName("periodic_checking_checkall")]
    public bool CheckAllAccounts { get; set; }

    [JsonPropertyName("auto_confirm_market_transactions")]
    public bool AutoConfirmMarketTransactions { get; set; }

    [JsonPropertyName("auto_confirm_trades")]
    public bool AutoConfirmTrades { get; set; }
    
    public class ManifestEntry
    {
        [JsonPropertyName("encryption_iv")]
        public string Iv { get; set; }

        [JsonPropertyName("encryption_salt")]
        public string Salt { get; set; }

        [JsonPropertyName("filename")]
        public string Filename { get; set; }

        [JsonPropertyName("steamid")]
        public ulong SteamId { get; set; }
    }
}
