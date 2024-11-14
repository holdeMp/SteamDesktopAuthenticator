using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Core;
using Serilog;
using SteamWebAuthenticator.Helpers;

namespace SteamWebAuthenticator.Models;

public class Account : SerializableFile
{
    public string? Username { get; set; }
    public string? Password { get; set; }
    
    public ulong SteamId { get; set; }

    private string? _backingSessionId;
    private string? _backingDeviceId;
    public string DeviceId
    {
        get => _backingDeviceId ?? GenerateDeviceId();
        set
        {
            _backingSessionId = value;
            Save();
        }
    }

    private Cookie? _backingTradeBackSessionCookie;
    public Cookie? TradeBackSessionCookie
    {
        get => _backingTradeBackSessionCookie;
        set
        {
            _backingTradeBackSessionCookie = value;
            Save();
        }
    }
    
    private string _backingXcsrfToken = string.Empty;
    public string XcsrfToken
    {
        get => _backingXcsrfToken;
        set
        {
            _backingXcsrfToken = value;
            Save();
        }
    }

    [JsonPropertyName("identity_secret")]
    public string IdentitySecret { get; set; }
    
    public List<Confirmation> Confirmations { get; set; } = [];
    
    public static string GenerateDeviceId()
    {
        return "android:" + Guid.NewGuid();
    }
    
    [JsonPropertyName("SessionID")] 
    public string SessionId { 
        get => _backingSessionId ?? string.Empty;
        set
        {
            _backingSessionId = value;
            Save();
        } 
    }
    
    private string? _backingSteamAccessToken;
    
    public DateTime? AccessTokenValidUntil;
    private const byte MinimumAccessTokenValidityMinutes = 5;
    public string? SteamAccessToken { 		
        get => _backingSteamAccessToken;

        set {
            AccessTokenValidUntil = null;

            if (string.IsNullOrEmpty(value)) {
                _backingSteamAccessToken = null;

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

            Save();
        }
    }
    public string? SteamRefreshToken { get; set; }
    public string? PreviouslyStoredGuardData { get; set; }
    public bool ShouldRememberPassword { get; set; }

    public bool IsLoggedIn { get; set; }
    
    protected override Task Save()  => Save(this);
    
    public Account CreateOrLoad(string filePath) {
        ArgumentException.ThrowIfNullOrEmpty(filePath);
        FilePath = filePath;
        if (!File.Exists(filePath)) {
            Utilities.InBackground(() => Save(this));
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
}