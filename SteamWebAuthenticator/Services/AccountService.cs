using SteamKit2;
using SteamKit2.Authentication;
using SteamKit2.Internal;
using SteamWebAuthenticator.Helpers;
using SteamWebAuthenticator.Interfaces;
using SteamWebAuthenticator.Models;


namespace SteamWebAuthenticator.Services;

public class AccountService : IAccountService
{
    private List<Account> _allAccounts = [];
    private SteamWeb? _steamWeb;
    private const byte MinimumAccessTokenValidityMinutes = 5;
    private readonly SteamClient _steamClient = new();
    private void NotifyStateChanged() => OnChange?.Invoke();
    private string? _selectedAccountName;
    public bool IsConnected { get; set; }
    private bool IsConnecting { get; set; }
    public bool IsConfirmationsLoading { get; set; }
    public Account? SelectedAccount
    {
        get { return _allAccounts.Find(a => a.Username == SelectedAccountName); }
    }
    public event Action? OnChange;

    public string? SelectedAccountName
    {
        get => _selectedAccountName;
        set
        {
            _selectedAccountName = value;
            NotifyStateChanged();
        }
    }
    
    public AccountService()
    {
        EnsureAccountsDirectoryExists();
        LoadAccounts();

        if (_allAccounts.Count == 0)
            return;

        InitializeSelectedAccount();
        ValidateAndConnect();
    }
    
    public AccountService(Account account)
    {
        _allAccounts.Add(account);
        SelectedAccountName = account.Username;
        _steamWeb = new SteamWeb(account, CookieHelper.GetCookies(account));
    }
    
    public List<Account> GetAccountsList()
    {
        return _allAccounts;
    }

    private static void EnsureAccountsDirectoryExists()
    {
        if (!Directory.Exists(Constants.Accounts))
        {
            Directory.CreateDirectory(Constants.Accounts);
        }
    }

    private void LoadAccounts()
    {
        var accountFiles = Directory.GetFiles(Constants.Accounts, "*.json");
        foreach (var accountFile in accountFiles)
        {
            var accountJson = File.ReadAllText(accountFile);
            var account = accountJson.FromJson<Account>();
            _allAccounts.Add(account);
        }
    }

    private void InitializeSelectedAccount()
    {
        SelectedAccountName = _allAccounts.First().Username;

        if (SelectedAccount == null)
            return;

        if (string.IsNullOrWhiteSpace(SelectedAccount.SessionId))
        {
            SelectedAccount.SessionId = CookieHelper.GenerateSessionId();
        }
    }

    private void ValidateAndConnect()
    {
        var minimumValidUntil = DateTime.UtcNow.AddMinutes(MinimumAccessTokenValidityMinutes);

        if (SelectedAccount != null &&
            SelectedAccount.SteamId != default &&
            !string.IsNullOrEmpty(SelectedAccount.SteamAccessToken) &&
            SelectedAccount.AccessTokenValidUntil >= minimumValidUntil)
        {
            IsConnected = true;
        }
        else
        {
            Connect();
        }
    }

    private void Connect()
    {
        IsConnecting = true;
        if (_steamClient.IsConnected) {
            return;
        }
        
        var manager = new CallbackManager(_steamClient);

        manager.Subscribe<SteamClient.ConnectedCallback>(OnConnected);


        _steamClient.Connect();
        Task.Run(() => { while (IsConnecting) { manager.RunWaitCallbacks(TimeSpan.FromSeconds(1)); } });
    }

    private void OnConnected(SteamClient.ConnectedCallback callback)
    {
        _ = HandleConnectedAsync(callback).ContinueWith(t =>
        {
            if (t.Exception != null)
            {
                Log.Error(t.Exception, "Error in HandleConnectedAsync");
            }
        }, TaskContinuationOptions.OnlyOnFaulted);
    }

    private async Task HandleConnectedAsync(SteamClient.ConnectedCallback callback)
    {
        if (SelectedAccount != null)
        {
            var authSession = await _steamClient.Authentication.BeginAuthSessionViaCredentialsAsync(
                new AuthSessionDetails
                {
                    Username = SelectedAccount.Username,
                    Password = SelectedAccount.Password,
                    IsPersistentSession = SelectedAccount.ShouldRememberPassword,
                    GuardData = SelectedAccount.PreviouslyStoredGuardData,
                    Authenticator = new AccountAuthenticator(SelectedAccount),
                    PlatformType = EAuthTokenPlatformType.k_EAuthTokenPlatformType_MobileApp,
                    ClientOSType = EOSType.Android9,
                });

            var pollResponse = await authSession.PollingWaitForResultAsync();
            SelectedAccount.PreviouslyStoredGuardData = pollResponse.NewGuardData;
            SelectedAccount.SteamId = authSession.SteamID.ConvertToUInt64();
            SelectedAccount.SteamRefreshToken = pollResponse.RefreshToken;
            SelectedAccount.SteamAccessToken = pollResponse.AccessToken;
        }

        IsConnecting = false;
        IsConnected = true;
    }
    
    public async Task<bool> AcceptMultipleConfirmationsAsync(List<Confirmation> confirmations)
    {
        if (_steamWeb == null)
        {
            return false;
        }
        IsConfirmationsLoading = true;
        NotifyStateChanged();
        try
        {
            var res = await _steamWeb.SendMultiConfirmationAjax(confirmations, Constants.Allow);
            return res;
        }
        finally
        {
            IsConfirmationsLoading = false;
            NotifyStateChanged();
        }
    }
    
    public async Task<bool> AcceptConfirmationAsync(Confirmation conf)
    {
        if (_steamWeb == null) return false;
        IsConfirmationsLoading = true;
        NotifyStateChanged();
        var res = await _steamWeb.SendConfirmationAjax(conf, Constants.Allow);
        IsConfirmationsLoading = false;
        NotifyStateChanged();
        return res;
    }

    public async Task<bool> DenyConfirmation(Confirmation conf)
    {
        return _steamWeb != null && await _steamWeb.SendConfirmationAjax(conf, Constants.Cancel);
    }

    public async Task FetchConfirmationsAsync()
    {
        if (SelectedAccount == null) return;
        IsConfirmationsLoading = true;
        NotifyStateChanged();
        try
        {
            _steamWeb = new SteamWeb(SelectedAccount, CookieHelper.GetCookies(SelectedAccount));
            var urlHelper = new UrlHelper(SelectedAccount);
            var url = urlHelper.GenerateConfirmationUrl();
            var response = await _steamWeb.GetConfUrlAsync(url);

            if (response.NeedAuthentication)
            {
                Connect();
                await FetchConfirmationsAsync();
            }
            SelectedAccount.Confirmations = response.Confirmations.ToList();
        }
        finally
        {
            IsConfirmationsLoading = false;
            NotifyStateChanged();
        }
    }
    
    public async Task SetAccountsList(List<Account> accounts)
    {
        _allAccounts = accounts;
        if (_allAccounts.Count > 0)
        {
            SelectedAccountName = _allAccounts.First().Username;
            await FetchConfirmationsAsync();
            NotifyStateChanged();
        }
    }

}