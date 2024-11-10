using System.Net;
using System.Text;
using Blazored.LocalStorage;
using SteamAuth;
using SteamAuth.Helpers;
using SteamWebAuthenticator.Interfaces;


namespace SteamWebAuthenticator.Services;

public class AccountService(ILocalStorageService localStorage)
    : IAccountService
{
    private readonly List<SteamGuardAccount> allAccounts = [];

    public List<Confirmation> SelectedAccountConfirmations { get; set; } = [];
    private SteamWeb? steamWeb;
    
    public async Task InitializeAsync()
    {
        if (allAccounts.Count == 0)
        {
            await LoadAccountsListAsync();
        }
        if (SelectedAccount != null) steamWeb = new SteamWeb(SelectedAccount, GetCookies());
    }

    public List<SteamGuardAccount> GetAccountsList()
    {
        return allAccounts;
    }

    public bool IsConfirmationsLoading { get; private set; } = true;
    public async Task RefreshAccessToken()
    {
        if (SelectedAccount == null || steamWeb == null)
            throw new Exception(Messages.SelectedAccountIsEmpty);
        if (string.IsNullOrEmpty(SelectedAccount.Session.RefreshToken))
            throw new Exception("Refresh token is empty");

        if (await IsTokenExpiredAsync(SelectedAccount.Session.RefreshToken!))
            throw new Exception("Refresh token is expired");

        string responseStr;
        try
        {
            var postData = new Dictionary<string, string>
            {
                { "refresh_token", SelectedAccount?.Session.RefreshToken! },
                { "steamid", SelectedAccount?.Session.SteamId.ToString() ?? throw new InvalidOperationException() }
            };
            responseStr = await steamWeb.PostRequest(APIEndpoints.GenerateAccessToken, postData);
        }
        catch (Exception ex)
        {
            throw new Exception("Failed to refresh token: " + ex.Message);
        }

        var response = await responseStr.FromJsonAsync<GenerateAccessTokenForAppResponse>();
        SelectedAccount.Session.AccessToken = response.Response.AccessToken;
    }
    
    public SteamGuardAccount? SelectedAccount
    {
        get { return allAccounts.Find(a => a.AccountName == SelectedAccountName); }
    }

    public async Task<bool> IsAccessTokenExpiredAsync()
    {
        if (string.IsNullOrEmpty(SelectedAccount?.Session?.AccessToken))
            return true;
        
        return await IsTokenExpiredAsync(SelectedAccount?.Session?.AccessToken!);
    }

    public async Task<bool> IsRefreshTokenExpired()
    {
        return string.IsNullOrEmpty(SelectedAccount?.Session?.AccessToken) || await IsTokenExpiredAsync(SelectedAccount?.Session?.AccessToken!);
    }

    private static async Task<bool> IsTokenExpiredAsync(string token)
    {
        var tokenComponents = token.Split('.');
        
        // Fix up base64url to normal base64
        var base64 = tokenComponents[1].Replace('-', '+').Replace('_', '/');

        if (base64.Length % 4 != 0)
        {
            base64 += new string('=', 4 - base64.Length % 4);
        }

        var payloadBytes = Convert.FromBase64String(base64);
        var payload = Encoding.UTF8.GetString(payloadBytes);
        var jwt = await payload.FromJsonAsync<SteamAccessToken>();

        // Compare expire time of the token to the current time
        return DateTimeOffset.UtcNow.ToUnixTimeSeconds() > jwt.Exp;
    }

    private CookieContainer GetCookies()
    {
        if (SelectedAccount == null)
            throw new Exception(Messages.SelectedAccountIsEmpty);
        SelectedAccount.Session.SessionId ??= GenerateSessionId();

        var cookies = new CookieContainer();
        foreach (var domain in new[] { "steamcommunity.com", "store.steampowered.com" })
        {
            cookies.Add(new Cookie("steamLoginSecure", GetSteamLoginSecure(), "/", domain));
            cookies.Add(new Cookie("sessionid", SelectedAccount.Session.SessionId, "/", domain));
            cookies.Add(new Cookie("mobileClient", "android", "/", domain));
            cookies.Add(new Cookie("mobileClientVersion", "777777 3.6.4", "/", domain));
        }
        return cookies;
    }

    private string GetSteamLoginSecure()
    {
        if (SelectedAccount == null)
            throw new Exception(Messages.SelectedAccountIsEmpty);
        return SelectedAccount.Session.SteamId + "%7C%7C" + SelectedAccount.Session.AccessToken;
    }

    private static string GenerateSessionId()
    {
        return GetRandomHexNumber(32);
    }

    private static string GetRandomHexNumber(int digits)
    {
        var random = new Random();
        var buffer = new byte[digits / 2];
        random.NextBytes(buffer);
        var result = string.Concat(buffer.Select(x => x.ToString("X2")).ToArray());
        if (digits % 2 == 0)
            return result;
        return result + random.Next(16).ToString("X");
    }
        
    public async Task<bool> AcceptMultipleConfirmationsAsync(List<Confirmation> confirmations)
    {
        if (steamWeb == null) return false;
        IsConfirmationsLoading = true;
        NotifyStateChanged();
        var res =  await steamWeb.SendMultiConfirmationAjax(confirmations, Constants.Allow);
        IsConfirmationsLoading = false;
        NotifyStateChanged();
        return res;

    }
    
    public async Task<bool> AcceptConfirmationAsync(Confirmation conf)
    {
        if (steamWeb == null) return false;
        IsConfirmationsLoading = true;
        NotifyStateChanged();
        var res = await steamWeb.SendConfirmationAjax(conf, Constants.Allow);
        IsConfirmationsLoading = false;
        NotifyStateChanged();
        return res;
    }

    public async Task<bool> DenyConfirmation(Confirmation conf)
    {
        return steamWeb != null && await steamWeb.SendConfirmationAjax(conf, Constants.Cancel);
    }

    public async Task FetchConfirmationsAsync()
    {
        if (SelectedAccount == null) return;
        steamWeb = new SteamWeb(SelectedAccount, GetCookies());
        var urlHelper = new UrlHelper(SelectedAccount);
        var url = urlHelper.GenerateConfirmationUrl();
        var response = await steamWeb.GetRequest(url);
        IsConfirmationsLoading = true;
        NotifyStateChanged();
        var confirmations = (await FetchConfirmationInternalAsync(response)).ToList();
        SelectedAccount.Confirmations = confirmations;
        IsConfirmationsLoading = false;
        NotifyStateChanged();
    }

    private static async Task<Confirmation[]> FetchConfirmationInternalAsync(string response)
    {
        var confirmationsResponse = await response.FromJsonAsync<ConfirmationsResponse>();

        if (!confirmationsResponse.Success)
        {
            throw new Exception(confirmationsResponse.Message);
        }

        if (confirmationsResponse.NeedAuthentication)
        {
            throw new Exception("Needs Authentication");
        }

        return confirmationsResponse.Confirmations;
    }
    
    
    /// <summary>
    /// Decrypts files and populates list UI with accounts
    /// </summary>
    public async Task LoadAccountsListAsync()
    {
        foreach (var entry in (await localStorage.KeysAsync()).Where(k => k.EndsWith(Constants.MaFile)))
        {
            var encryptedAccountJson = await localStorage.GetItemAsStringAsync(entry);
            if (encryptedAccountJson == null) continue;
            var decryptedJson = encryptedAccountJson.Decrypt();
            var account = await decryptedJson.FromJsonAsync<SteamGuardAccount>();
            allAccounts.Add(account);
        }

        if (allAccounts.Count > 0)
        {
            SelectedAccountName = allAccounts.First().AccountName;
            await FetchConfirmationsAsync();
            NotifyStateChanged();
        }
    }

    public event Action? OnChange;

    private string? _selectedAccountName;
    public string? SelectedAccountName
    {
        get => _selectedAccountName;
        set
        {
            _selectedAccountName = value;
            NotifyStateChanged();
        }
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}