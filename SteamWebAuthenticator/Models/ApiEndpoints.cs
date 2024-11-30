namespace SteamWebAuthenticator.Models
{
    public static class ApiEndpoints
    {
        private const string SteamApiBase = "https://api.steampowered.com";
        public const string CommunityBase = "https://steamcommunity.com";
        private const string TwoFactorBase = SteamApiBase + "/ITwoFactorService/%s/v0001";
        public static readonly string TwoFactorTimeQuery = TwoFactorBase.Replace("%s", "QueryTime");
    }
}
