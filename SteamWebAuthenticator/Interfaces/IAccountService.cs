using SteamAuth;

namespace SteamWebAuthenticator.Interfaces;

public interface IAccountService
{
    List<SteamGuardAccount> GetAccountsList();
    Task InitializeAsync();
    string? SelectedAccountName { get; set; }
    event Action OnChange;
    Task<Confirmation[]> FetchConfirmationsAsync();
}