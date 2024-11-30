using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Core;
using Serilog;
using SteamWebAuthenticator.Helpers;
// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable SuggestVarOrType_BuiltInTypes

namespace SteamWebAuthenticator.Models;

public class Account : SerializableFile
{
    private const byte MinimumAccessTokenValidityMinutes = 5;

    private static byte[] _steamGuardCodeTranslations =
        [50, 51, 52, 53, 54, 55, 56, 57, 66, 67, 68, 70, 71, 72, 74, 75, 77, 78, 80, 81, 82, 84, 86, 87, 88, 89];

    private string? _backingSessionId;
    private string? _backingDeviceId;
    private Cookie? _backingTradeBackSessionCookie;
    private string _backingXcsrfToken = string.Empty;
    private string _backingSteamAccessToken = string.Empty;

    public string SteamGuardCode { get; set; } = string.Empty;

    public string? Password { get; init; }
    
    public ulong SteamId { get; set; }

    public bool IsAuthenticated
    {
        get
        {
            var minimumValidUntil = DateTime.UtcNow.AddMinutes(MinimumAccessTokenValidityMinutes);
            var res = !string.IsNullOrEmpty(SteamAccessToken) &&
                       AccessTokenValidUntil >= minimumValidUntil;
            return res;
        }
    }

    public string DeviceId
    {
        get => _backingDeviceId ?? GenerateDeviceId();
        init => SetProperty(ref _backingDeviceId, value);
    }

    public Cookie? TradeBackSessionCookie
    {
        get => _backingTradeBackSessionCookie;
        set => SetProperty(ref _backingTradeBackSessionCookie, value);
    }
    
    public string XcsrfToken
    {
        get => _backingXcsrfToken;
        set => SetProperty(ref _backingXcsrfToken, value);
    }

    [JsonPropertyName("identity_secret")] 
    public string IdentitySecret { get; init; } = string.Empty;
    
    [JsonPropertyName("shared_secret")]
    public string SharedSecret { get; init; } = string.Empty;
    
    public List<Confirmation> Confirmations { get; set; } = [];
    
    [JsonPropertyName("SessionID")] 
    public string SessionId { 
        get => _backingSessionId ?? string.Empty;
        set => SetProperty(ref _backingSessionId, value);
    }
    
    public DateTime? AccessTokenValidUntil { get; set; }
    
    public string? SteamAccessToken { 		
        get => _backingSteamAccessToken;

        set {
            AccessTokenValidUntil = null;

            if (string.IsNullOrEmpty(value)) {
                _backingSteamAccessToken = string.Empty;
                return;
            }

            if (!Utilities.TryReadJsonWebToken(value, out var accessToken)) {
                Log.Error(Strings.FormatErrorIsInvalid(nameof(accessToken)));

                return;
            }

            _backingSteamAccessToken = value;

            if (accessToken.ValidTo > DateTime.MinValue) {
                AccessTokenValidUntil = accessToken.ValidTo;
            }

            if (!string.IsNullOrWhiteSpace(Username))
            {
                SaveAsync();
            }
        }
    }
    
    public string? SteamRefreshToken { get; set; }
    public string? PreviouslyStoredGuardData { get; set; }
    public bool ShouldRememberPassword { get; init; } = true;

    public bool IsLoggedIn { get; init; }
    
    public static string GenerateDeviceId()
    {
        return "android:" + Guid.NewGuid();
    }
    
    public override Task SaveAsync()  => SaveAsync(this);
    
    public async Task<Account> CreateOrLoadAsync(string filePath) {
        ArgumentException.ThrowIfNullOrEmpty(filePath);
        if (!File.Exists(filePath)) {
            await Utilities.InBackgroundAsync(() => SaveAsync(this));
            return this;
        }
		
        var json = await File.ReadAllTextAsync(filePath);

        var account = JsonSerializer.Deserialize<Account>(json);
        if (account != null && !string.IsNullOrEmpty(json))
        {
            return account;
        }

        Log.Error(Strings.FormatErrorIsEmpty(nameof(json)));
        return this;
    }
    
    public async Task<string> GenerateSteamGuardCodeAsync()
    {
        var time = await TimeAligner.GetSteamTimeAsync();
        return GenerateSteamGuardCodeForTime(time);
    }
    
    public string GenerateSteamGuardCodeForTime(long time)
    {
        if (SharedSecret.Length == 0)
        {
            return "";
        }

        string sharedSecretUnescaped = Regex.Unescape(SharedSecret);
        byte[] sharedSecretArray = Convert.FromBase64String(sharedSecretUnescaped);
        var timeArray = new byte[8];

        time /= 30L;

        for (var i = 8; i > 0; i--)
        {
            timeArray[i - 1] = (byte)time;
            time >>= 8;
        }

        var hmacGenerator = new HMACSHA1();
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
    
    private void SetProperty<T>(ref T backingField, T value)
    {
        backingField = value;
        if (!string.IsNullOrWhiteSpace(Username))
        {
            SaveAsync();
        }
    }
}