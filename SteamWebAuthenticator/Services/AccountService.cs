using Blazored.LocalStorage;
using SteamAuth;
using SteamAuth.Helpers;
using SteamWebAuthenticator.Helpers;
using SteamWebAuthenticator.Interfaces;
using SteamWebAuthenticator.Models;


namespace SteamWebAuthenticator.Services;

public class AccountService : IAccountService
{
    private readonly List<Account> _allAccounts = [];

    public List<Confirmation> SelectedAccountConfirmations { get; set; } = [];
    private SteamWeb? _steamWeb;
    private readonly ILocalStorageService? _localStorageService;
    public AccountService(ILocalStorageService localStorage)
    {
        _localStorageService = localStorage;
    }
    
    public AccountService(Account account)
    {
        _allAccounts.Add(account);
        SelectedAccountName = account.Username;
        _steamWeb = new SteamWeb(account, CookieHelper.GetCookies(account));
    }
    

    public async Task InitializeAsync()
    {
        if (_allAccounts.Count == 0)
        {
            await LoadAccountsListAsync();
        }
        if (SelectedAccount != null) _steamWeb = new SteamWeb(SelectedAccount, CookieHelper.GetCookies(SelectedAccount));
    }

    public List<Account> GetAccountsList()
    {
        return _allAccounts;
    }

    public bool IsConfirmationsLoading { get; private set; } = true;
    
    public Account? SelectedAccount
    {
        get { return _allAccounts.Find(a => a.Username == SelectedAccountName); }
    }
    
        
    public async Task<bool> AcceptMultipleConfirmationsAsync(List<Confirmation> confirmations)
    {
        if (_steamWeb == null) return false;
        IsConfirmationsLoading = true;
        NotifyStateChanged();
        var res =  await _steamWeb.SendMultiConfirmationAjax(confirmations, Constants.Allow);
        IsConfirmationsLoading = false;
        NotifyStateChanged();
        return res;
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
        _steamWeb = new SteamWeb(SelectedAccount, CookieHelper.GetCookies(SelectedAccount));
        var urlHelper = new UrlHelper(SelectedAccount);
        var url = urlHelper.GenerateConfirmationUrl();
        var response = await _steamWeb.GetRequest(url);
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
        if (_localStorageService != null)
        {
            foreach (var entry in (await _localStorageService.KeysAsync()).Where(k => k.EndsWith(Constants.MaFile)))
            {
                var encryptedAccountJson = await _localStorageService.GetItemAsStringAsync(entry);
                if (encryptedAccountJson == null) continue;
                var decryptedJson = encryptedAccountJson.Decrypt();
                var account = await decryptedJson.FromJsonAsync<Account>();
                _allAccounts.Add(account);
            }
        }

        if (_allAccounts.Count > 0)
        {
            SelectedAccountName = _allAccounts.First().Username;
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