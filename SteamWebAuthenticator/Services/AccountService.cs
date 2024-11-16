using SteamAuth;
using SteamAuth.Helpers;
using SteamKit2;
using SteamKit2.Authentication;
using SteamKit2.Internal;
using SteamWebAuthenticator.Helpers;
using SteamWebAuthenticator.Interfaces;
using SteamWebAuthenticator.Models;
using SteamWebAuthenticator.Models.Responses;


namespace SteamWebAuthenticator.Services;

public class AccountService : IAccountService
{
    private List<Account> allAccounts = [];
    private SteamWeb? steamWeb;
    private bool isRunning;
    private const byte MinimumAccessTokenValidityMinutes = 5;
    private readonly SteamClient steamClient = new();
    public bool IsConfirmationsLoading { get; set; }
    public Account? SelectedAccount
    {
        get { return allAccounts.Find(a => a.Username == SelectedAccountName); }
    }
    
    public AccountService()
    {
        if (!Directory.Exists(Constants.Accounts))
        {
            Directory.CreateDirectory(Constants.Accounts);
            return;
        }
        var accountFiles = Directory.GetFiles(Constants.Accounts, "*.json");
        var containsAccounts = accountFiles.Length > 0;

        if (!containsAccounts) return;
        foreach (var accountFile in accountFiles)
        {
            var accountJson = File.ReadAllText(accountFile);
            allAccounts.Add(accountJson.FromJson<Account>());
        }
        var minimumValidUntil = DateTime.UtcNow.AddMinutes(MinimumAccessTokenValidityMinutes);
        SelectedAccountName = allAccounts.First().Username;
        if (SelectedAccount != null && 
            SelectedAccount.SteamId != default && 
            !string.IsNullOrEmpty(SelectedAccount.SteamAccessToken) && 
            SelectedAccount.AccessTokenValidUntil >= minimumValidUntil) return;
        Connect();
    }
    
    public AccountService(Account account)
    {
        allAccounts.Add(account);
        SelectedAccountName = account.Username;
        steamWeb = new SteamWeb(account, CookieHelper.GetCookies(account));
    }
    
    public List<Account> GetAccountsList()
    {
        return allAccounts;
    }

    private void Connect()
    {
        isRunning = true;
        if (steamClient.IsConnected) {
            return;
        }
        
        var manager = new CallbackManager(steamClient);

        manager.Subscribe<SteamClient.ConnectedCallback>(OnConnected);


        steamClient.Connect();
        Task.Run(() => { while (isRunning) { manager.RunWaitCallbacks(TimeSpan.FromSeconds(1)); } });
    }

    private async void OnConnected(SteamClient.ConnectedCallback callback)
    {
        if (SelectedAccount != null)
        {
            var authSession = await steamClient.Authentication.BeginAuthSessionViaCredentialsAsync(
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

        isRunning = false;
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
        IsConfirmationsLoading = true;
        NotifyStateChanged();
        try
        {
            steamWeb = new SteamWeb(SelectedAccount, CookieHelper.GetCookies(SelectedAccount));
            var urlHelper = new UrlHelper(SelectedAccount);
            var url = urlHelper.GenerateConfirmationUrl();
            var response = await steamWeb.GetConfUrlAsync(url);

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
        allAccounts = accounts;
        if (allAccounts.Count > 0)
        {
            SelectedAccountName = allAccounts.First().Username;
            await FetchConfirmationsAsync();
            NotifyStateChanged();
        }
    }
    
    /// <summary>
    /// Decrypts files and populates list UI with accounts
    /// </summary>
    public async Task LoadAccountsListAsync()
    {
        string accountsDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "accounts");

        if (Directory.Exists(accountsDirectory))
        {
            var accountFiles = Directory.GetFiles(accountsDirectory, $"*{Constants.MaFile}");
            foreach (var filePath in accountFiles)
            {
                var encryptedAccountJson = await File.ReadAllTextAsync(filePath);
                if (string.IsNullOrEmpty(encryptedAccountJson)) continue;

                var decryptedJson = encryptedAccountJson.Decrypt();
                var account = await decryptedJson.FromJsonAsync<Account>();
                allAccounts.Add(account);
            }
        }

        if (allAccounts.Count > 0)
        {
            SelectedAccountName = allAccounts.First().Username;
            await FetchConfirmationsAsync();
            NotifyStateChanged();
        }
    }

    public event Action? OnChange;

    private string? selectedAccountName;
    public string? SelectedAccountName
    {
        get => selectedAccountName;
        set
        {
            selectedAccountName = value;
            NotifyStateChanged();
        }
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}