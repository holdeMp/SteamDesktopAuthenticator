using SteamAuth;

namespace SteamWebAuthenticator.Interfaces;

public interface IAccountService
{
    List<SteamGuardAccount> GetAccountsList();
    Task LoadAccountsListAsync();
    Task InitializeAsync();
    string? SelectedAccountName { get; set; }
    SteamGuardAccount? SelectedAccount { get; }
    event Action OnChange;
    public bool IsConfirmationsLoading { get; }
    Task<bool> AcceptMultipleConfirmationsAsync(List<Confirmation> confirmations);
    Task FetchConfirmationsAsync();
    Task<bool> AcceptConfirmationAsync(Confirmation conf);
}