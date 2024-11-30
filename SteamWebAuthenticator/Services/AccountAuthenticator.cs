using SteamKit2.Authentication;
using SteamWebAuthenticator.Models;
// ReSharper disable SuggestVarOrType_BuiltInTypes

namespace SteamWebAuthenticator.Services;

public class AccountAuthenticator(Account account) : IAuthenticator
{
    private int _deviceCodesGenerated;
    public async Task<string> GetDeviceCodeAsync(bool previousCodeWasIncorrect)
    {
        if (previousCodeWasIncorrect)
        {
            // After 2 tries - tell the user that there seems to be an issue
            if (_deviceCodesGenerated > 2)
                throw new InvalidOperationException(
                    "There seems to be an issue logging into your account with these two factor codes. Are you sure SDA is still your authenticator?");

            await Task.Delay(30000);
        }


        string deviceCode = await account.GenerateSteamGuardCodeAsync();
        _deviceCodesGenerated++;

        return deviceCode;
    }

    
    public Task<string> GetEmailCodeAsync(string email, bool previousCodeWasIncorrect)
    {
        throw new NotImplementedException();
    }

    public Task<bool> AcceptDeviceConfirmationAsync()
    {
        return Task.FromResult(true);
    }
}