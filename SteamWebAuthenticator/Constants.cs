namespace SteamWebAuthenticator;

public static class Constants
{
    public static string Accounts => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Accounts");
    public const string Allow = "allow";
    public const string Cancel = "cancel";
    public const string Accept = "accept";
    public const string Reject = "reject";
    public const string JsonExtension = ".json";
}