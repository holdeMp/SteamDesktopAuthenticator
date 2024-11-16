using SteamAuth;
using SteamWebAuthenticator.Models;

namespace SteamWebAuthenticator.Interfaces;

public interface IAccountService
{
    Task SetAccountsList(List<Account> accounts);
    List<Account> GetAccountsList();
    string? SelectedAccountName { get; set; }
    Account? SelectedAccount { get; }
    event Action OnChange;
    public bool IsConfirmationsLoading { get; set; }
    Task<bool> AcceptMultipleConfirmationsAsync(List<Confirmation> confirmations);
    Task FetchConfirmationsAsync();
    Task<bool> AcceptConfirmationAsync(Confirmation conf);
}