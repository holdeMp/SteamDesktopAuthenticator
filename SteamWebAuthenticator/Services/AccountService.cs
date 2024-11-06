using System.Net;
using System.Security.Cryptography;
using System.Text;
using Blazored.LocalStorage;
using SteamAuth;
using SteamAuth.Helpers;
using SteamWebAuthenticator.Interfaces;


namespace SteamWebAuthenticator.Services;

public class AccountService(ILocalStorageService localStorage)
    : IAccountService
{
    private List<SteamGuardAccount> allAccounts = [];

    public List<Confirmation> SelectedAccountConfirmations = [];
    private SteamWeb SteamWeb;
    public async Task InitializeAsync()
    {
        allAccounts = await LoadAccountsListAsync();
        
    }

    public List<SteamGuardAccount> GetAccountsList()
    {
        return allAccounts;
    }

    
    public async Task AcceptMultipleConfirmations(Confirmation[] confs)
    {
        await SteamWeb.SendMultiConfirmationAjax(confs, Constants.Allow);
    }
    
    public async Task<bool> AcceptConfirmation(Confirmation conf)
    {
        return await SteamWeb.SendConfirmationAjax(conf, Constants.Allow);
    }

    public async Task<bool> DenyConfirmation(Confirmation conf)
    {
        return await SteamWeb.SendConfirmationAjax(conf, Constants.Cancel);
    }
    
    public async Task<Confirmation[]> FetchConfirmationsAsync()
    {
        var account = GetSelectedAccount();
        if (account == null) return new List<Confirmation>().ToArray();
        SteamWeb = new SteamWeb(account);
        var urlHelper = new UrlHelper(account);
        var url = urlHelper.GenerateConfirmationUrl();
        var response = await SteamWeb.GetRequest(url);
        var confirmations = await FetchConfirmationInternal(response);
        return confirmations;
    }

    private static async Task<Confirmation[]> FetchConfirmationInternal(string response)
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
    
    public SteamGuardAccount? GetSelectedAccount()
    {
        return allAccounts.Find(a => a.AccountName == SelectedAccountName);
    }
    
    /// <summary>
    /// Decrypts files and populates list UI with accounts
    /// </summary>
    private async Task<List<SteamGuardAccount>> LoadAccountsListAsync()
    {
        var accounts = new List<SteamGuardAccount>();
        foreach (var entry in (await localStorage.KeysAsync()).Where(k => k != Constants.Manifest))
        {
            var encryptedAccountJson = await localStorage.GetItemAsStringAsync(entry);
            if (encryptedAccountJson == null) continue;
            var decryptedJson = encryptedAccountJson.Decrypt();

            var account = await decryptedJson.FromJsonAsync<SteamGuardAccount>();
            accounts.Add(account);
        }

        return accounts;
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