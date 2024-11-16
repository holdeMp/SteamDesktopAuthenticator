using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Core;
using Serilog;
using SteamWebAuthenticator.Helpers;

namespace SteamWebAuthenticator.Models;

public class Account : SerializableFile
{
    public string? Username { get; set; }
    public string? Password { get; set; }
    
    public ulong SteamId { get; set; }

    private string? backingSessionId;
    private string? backingDeviceId;
    public string DeviceId
    {
        get => backingDeviceId ?? GenerateDeviceId();
        set
        {
            backingDeviceId = value;
            SaveAsync();
        }
    }

    private Cookie? backingTradeBackSessionCookie;
    public Cookie? TradeBackSessionCookie
    {
        get => backingTradeBackSessionCookie;
        set
        {
            backingTradeBackSessionCookie = value;
            SaveAsync();
        }
    }
    
    private string backingXcsrfToken = string.Empty;
    public string XcsrfToken
    {
        get => backingXcsrfToken;
        set
        {
            backingXcsrfToken = value;
            SaveAsync();
        }
    }

    [JsonPropertyName("identity_secret")] 
    public string IdentitySecret { get; set; } = string.Empty;
    
    [JsonPropertyName("shared_secret")]
    public string SharedSecret { get; set; } = string.Empty;
    
    public List<Confirmation> Confirmations { get; set; } = [];
    
    public static string GenerateDeviceId()
    {
        return "android:" + Guid.NewGuid();
    }
    
    [JsonPropertyName("SessionID")] 
    public string SessionId { 
        get => backingSessionId ?? string.Empty;
        set
        {
            backingSessionId = value;
            SaveAsync();
        } 
    }
    
    private string? backingSteamAccessToken;
    
    public DateTime? AccessTokenValidUntil;
    private const byte MinimumAccessTokenValidityMinutes = 5;
    public string? SteamAccessToken { 		
        get => backingSteamAccessToken;

        set {
            AccessTokenValidUntil = null;

            if (string.IsNullOrEmpty(value)) {
                backingSteamAccessToken = null;

                return;
            }

            if (!Utilities.TryReadJsonWebToken(value, out var accessToken)) {
                Log.Error(Strings.FormatErrorIsInvalid(nameof(accessToken)));

                return;
            }

            backingSteamAccessToken = value;

            if (accessToken.ValidTo > DateTime.MinValue) {
                AccessTokenValidUntil = accessToken.ValidTo;
            }

            SaveAsync();
        }
    }
    public string? SteamRefreshToken { get; set; }
    public string? PreviouslyStoredGuardData { get; set; }
    public bool ShouldRememberPassword { get; set; }

    public bool IsLoggedIn { get; set; }
    public override Task SaveAsync()  => SaveAsync(this);
    
    public Account CreateOrLoad(string filePath) {
        ArgumentException.ThrowIfNullOrEmpty(filePath);
        FilePath = filePath;
        if (!File.Exists(filePath)) {
            Utilities.InBackground(() => SaveAsync(this));
            return this;
        }
		
        var json = File.ReadAllText(filePath);

        var account = JsonSerializer.Deserialize<Account>(json);
        if (account != null)
        {
            account.FilePath = filePath;
            if (!string.IsNullOrEmpty(json)) return account ?? throw new InvalidOperationException();
        }

        Log.Error(Strings.FormatErrorIsEmpty(nameof(json)));
        return this;
    }
    
    public string GenerateSteamGuardCode()
    {
        return GenerateSteamGuardCodeForTime(TimeAligner.GetSteamTime());
    }
    private static byte[] _steamGuardCodeTranslations = [50, 51, 52, 53, 54, 55, 56, 57, 66, 67, 68, 70, 71, 72, 74, 75, 77, 78, 80, 81, 82, 84, 86, 87, 88, 89
    ];
    
    public string GenerateSteamGuardCodeForTime(long time)
    {
        if (SharedSecret.Length == 0)
        {
            return "";
        }

        string sharedSecretUnescaped = Regex.Unescape(SharedSecret);
        byte[] sharedSecretArray = Convert.FromBase64String(sharedSecretUnescaped);
        byte[] timeArray = new byte[8];

        time /= 30L;

        for (int i = 8; i > 0; i--)
        {
            timeArray[i - 1] = (byte)time;
            time >>= 8;
        }

        HMACSHA1 hmacGenerator = new HMACSHA1();
        hmacGenerator.Key = sharedSecretArray;
        byte[] hashedData = hmacGenerator.ComputeHash(timeArray);
        byte[] codeArray = new byte[5];
        
        byte b = (byte)(hashedData[19] & 0xF);
        int codePoint = (hashedData[b] & 0x7F) << 24 | (hashedData[b + 1] & 0xFF) << 16 | (hashedData[b + 2] & 0xFF) << 8 | (hashedData[b + 3] & 0xFF);

        for (int i = 0; i < 5; ++i)
        {
            codeArray[i] = _steamGuardCodeTranslations[codePoint % _steamGuardCodeTranslations.Length];
            codePoint /= _steamGuardCodeTranslations.Length;
        }
        
        return Encoding.UTF8.GetString(codeArray);
    }
}