using Jose;

namespace SteamWebAuthenticator;

public static class Constants
{
    public static string Accounts => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Accounts");
    public const string Allow = "allow";
    public const string Cancel = "cancel";
    public const string Accept = "accept";
    public const string Reject = "reject";
    public const JwsAlgorithm EncodingALgo = JwsAlgorithm.HS256;
    public const string MaFile = ".maFile";
    public const string Password = "s;WV!g)E6B%[2X>3,7ec~z\nB&C{~8\"Nrg!^WpKzEsdqkQ\nGBmv&$+:/t;\"f@z%usa7U>\nMQ93[r-*eg=y+F4&{(BZcn\nNz5]^%.=!~sk8\"D`yg24Jd";
}