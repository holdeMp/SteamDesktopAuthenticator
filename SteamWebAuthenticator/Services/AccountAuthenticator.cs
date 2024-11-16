using SteamKit2.Authentication;
using SteamWebAuthenticator.Models;

namespace SteamWebAuthenticator.Services;

public class AccountAuthenticator(Account account) : IAuthenticator
{
    private int deviceCodesGenerated = 0;
    public async Task<string> GetDeviceCodeAsync(bool previousCodeWasIncorrect)
    {
        if (previousCodeWasIncorrect)
        {
            // After 2 tries tell the user that there seems to be an issue
            if (deviceCodesGenerated > 2)
                throw new Exception("There seems to be an issue logging into your account with these two factor codes. Are you sure SDA is still your authenticator?");

            await Task.Delay(30000);
        }


        string deviceCode = account.GenerateSteamGuardCode();
        deviceCodesGenerated++;

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