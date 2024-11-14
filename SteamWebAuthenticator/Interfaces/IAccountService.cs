using SteamAuth;
using SteamWebAuthenticator.Models;

namespace SteamWebAuthenticator.Interfaces;

public interface IAccountService
{
    List<Account> GetAccountsList();
    Task LoadAccountsListAsync();
    Task InitializeAsync();
    string? SelectedAccountName { get; set; }
    Account? SelectedAccount { get; }
    event Action OnChange;
    public bool IsConfirmationsLoading { get; }
    Task<bool> AcceptMultipleConfirmationsAsync(List<Confirmation> confirmations);
    Task FetchConfirmationsAsync();
    Task<bool> AcceptConfirmationAsync(Confirmation conf);
}