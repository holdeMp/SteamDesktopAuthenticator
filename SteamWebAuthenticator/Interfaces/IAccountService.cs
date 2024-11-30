using SteamWebAuthenticator.Models;

namespace SteamWebAuthenticator.Interfaces;

public interface IAccountService
{
    Task SetAccountsListAsync(List<Account> accounts);
    List<Account> GetAccountsList();
    string? SelectedAccountName { get; set; }
    Account? SelectedAccount { get; }
    event Action OnChange;
    public bool IsConfirmationsLoading { get; set; }
    Task<bool> AcceptMultipleConfirmationsAsync(List<Confirmation> confirmations);
    Task FetchConfirmationsAsync(int maxRetries = 3, int delayMilliseconds = 1000);
    Task<bool> AcceptConfirmationAsync(Confirmation conf);
}