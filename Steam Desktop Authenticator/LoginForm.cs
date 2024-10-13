using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using SteamAuth;
using SteamKit2;
using SteamKit2.Authentication;
using System.IO;

namespace Steam_Desktop_Authenticator
{
    public partial class LoginForm : Form
    {
        private readonly SteamGuardAccount account;
        private readonly LoginType loginReason;
        private readonly CallbackManager callbackManager;
        public SessionData Session;
        private readonly SteamClient steamClient;
        private string username;
        private string password;
        private bool isRunning;
        private string previouslyStoredGuardData;
        private readonly SteamUser steamUser;
        public LoginForm(LoginType loginReason = LoginType.Initial, SteamGuardAccount account = null)
        {
            InitializeComponent();
            this.loginReason = loginReason;
            this.account = account;
            
            try
            {
                if (this.loginReason != LoginType.Initial)
                {
                    if (account != null) txtUsername.Text = account.AccountName;
                    txtUsername.Enabled = false;
                }

                labelLoginExplanation.Text = this.loginReason switch
                {
                    LoginType.Refresh =>
                        "Your Steam credentials have expired. For trade and market confirmations to work properly, please login again.",
                    LoginType.Import => "Please login to your Steam account import it.",
                    _ => labelLoginExplanation.Text
                };

                steamClient = new SteamClient();
                callbackManager = new CallbackManager(steamClient);
                callbackManager.Subscribe<SteamClient.ConnectedCallback>(OnConnected);
                callbackManager.Subscribe<SteamClient.DisconnectedCallback>(OnDisconnected);
                steamUser = steamClient.GetHandler<SteamUser>() ?? throw new InvalidOperationException(nameof(steamUser));
                callbackManager.Subscribe<SteamUser.LoggedOffCallback>(OnLoggedOff);
            }
            catch (Exception)
            {
                MessageBox.Show(@"Failed to find your account. Try closing and re-opening SDA.", @"Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
            }
        }

        private void ResetLoginButton()
        {
            btnSteamLogin.Enabled = true;
            btnSteamLogin.Text = @"Login";
        }

        private void btnSteamLogin_Click(object sender, EventArgs e)
        {
            isRunning = true;
            btnSteamLogin.Enabled = false;
            btnSteamLogin.Text = @"Logging in...";

            username = txtUsername.Text;
            password = txtPassword.Text;

            steamClient.Connect();

            Task.Run(() => { while (isRunning) { callbackManager.RunWaitCallbacks(TimeSpan.FromSeconds(1)); } });
        }

        private async void OnConnected(SteamClient.ConnectedCallback callback)
        {
            CredentialsAuthSession authSession;
            var storedGuardDataPath = $"{nameof(previouslyStoredGuardData)}_{username}";
            try
            {
                if (File.Exists(storedGuardDataPath))
                {
                    previouslyStoredGuardData = await File.ReadAllTextAsync(storedGuardDataPath);
                }
                authSession = await steamClient.Authentication.BeginAuthSessionViaCredentialsAsync(new AuthSessionDetails
                {
                    Username = username,
                    Password = password,
                    IsPersistentSession = true,
                    GuardData = previouslyStoredGuardData,
                    Authenticator = new UserFormAuthenticator(account),
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Strings.SteamLoginError, MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
                return;
            }

            // Starting polling Steam for authentication response
            AuthPollResult pollResponse;
            try
            {
                pollResponse = await authSession.PollingWaitForResultAsync();
                if (pollResponse.NewGuardData != null)
                {
                    // When using certain two factor methods (such as email 2fa), guard data may be provided by Steam
                    // for use in future authentication sessions to avoid triggering 2FA again (this works similarly to the old sentry file system).
                    // Do note that this guard data is also a JWT token and has an expiration date.
                    previouslyStoredGuardData = pollResponse.NewGuardData;
                    await File.WriteAllTextAsync(storedGuardDataPath, pollResponse.NewGuardData);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Strings.SteamLoginError, MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
                return;
            }

            steamUser.LogOn(new SteamUser.LogOnDetails
            {
                Username = pollResponse.AccountName,
                AccessToken = pollResponse.RefreshToken,
                ShouldRememberPassword = true, // If you set IsPersistentSession to true, this also must be set to true for it to work correctly
            });

            // Build a SessionData object
            var sessionData = new SessionData()
            {
                SteamID = authSession.SteamID.ConvertToUInt64(),
                AccessToken = pollResponse.AccessToken,
                RefreshToken = pollResponse.RefreshToken,
            };

            //Login succeeded
            Session = sessionData;

            switch (loginReason)
            {
                // If we're only logging in for an account import, stop here
                case LoginType.Import:
                    Close();
                    return;
                // If we're only logging in for a session refresh then save it and exit
                case LoginType.Refresh:
                {
                    var man = Manifest.GetManifest();
                    account.FullyEnrolled = true;
                    account.Session = sessionData;
                    HandleManifest(man, true);
                    Close();
                    return;
                }
            }

            // Show a dialog to make sure they really want to add their authenticator
            var result =
                MessageBox.Show(Strings.LoginSucceded,
                    Strings.SteamLogin, MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
            if (result == DialogResult.Cancel)
            {
                MessageBox.Show(Strings.Aborted, Strings.SteamLogin, MessageBoxButtons.OK, MessageBoxIcon.Error);
                ResetLoginButton();
                return;
            }

            // Begin linking mobile authenticator
            var linker = new AuthenticatorLinker(sessionData);

            var linkResponse = AuthenticatorLinker.LinkResult.GeneralFailure;
            while (linkResponse != AuthenticatorLinker.LinkResult.AwaitingFinalization)
            {
                try
                {
                    linkResponse = await linker.AddAuthenticator();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(Strings.ErrorAddingYourAuthenticator + ex.Message, Strings.SteamLogin, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    ResetLoginButton();
                    return;
                }

                switch (linkResponse)
                {
                    case AuthenticatorLinker.LinkResult.MustProvidePhoneNumber:

                        // Show the phone input form
                        PhoneInputForm phoneInputForm = new PhoneInputForm(account);
                        phoneInputForm.ShowDialog();
                        if (phoneInputForm.Canceled)
                        {
                            Close();
                            return;
                        }

                        linker.PhoneNumber = phoneInputForm.PhoneNumber;
                        linker.PhoneCountryCode = phoneInputForm.CountryCode;
                        break;

                    case AuthenticatorLinker.LinkResult.AuthenticatorPresent:
                        MessageBox.Show(Strings.AlreadyLinkedAuthenticator, Strings.SteamLogin, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        Close();
                        return;

                    case AuthenticatorLinker.LinkResult.FailureAddingPhone:
                        MessageBox.Show(Strings.FailedAddingPhoneNumber, Strings.SteamLogin, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        linker.PhoneNumber = null;
                        break;

                    case AuthenticatorLinker.LinkResult.MustRemovePhoneNumber:
                        linker.PhoneNumber = null;
                        break;

                    case AuthenticatorLinker.LinkResult.MustConfirmEmail:
                        MessageBox.Show(Strings.CheckEmail, Strings.SteamLogin, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        break;

                    case AuthenticatorLinker.LinkResult.GeneralFailure:
                        MessageBox.Show(Strings.ErrorAddingAuthenticator, Strings.SteamLoginError, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        Close();
                        return;
                }
            } // End while loop checking for AwaitingFinalization

            var manifest = Manifest.GetManifest();
            string passKey = null;
            switch (manifest.Entries.Count)
            {
                case 0:
                    passKey = manifest.PromptSetupPassKey("Please enter an encryption passkey. Leave blank or hit cancel to not encrypt (VERY INSECURE).");
                    break;
                case > 0 when manifest.Encrypted:
                {
                    var passKeyValid = false;
                    while (!passKeyValid)
                    {
                        var passKeyForm = new InputForm("Please enter your current encryption passkey.");
                        passKeyForm.ShowDialog();
                        if (!passKeyForm.Canceled)
                        {
                            passKey = passKeyForm.txtBox.Text;
                            passKeyValid = manifest.VerifyPasskey(passKey);
                            if (passKeyValid) continue;
                            MessageBox.Show(Strings.InvalidPasskey);
                        }
                        else
                        {
                            Close();
                            return;
                        }
                    }

                    break;
                }
            }

            //Save the file immediately; losing this would be bad.
            if (!manifest.SaveAccount(linker.LinkedAccount, passKey != null, passKey))
            {
                manifest.RemoveAccount(linker.LinkedAccount);
                MessageBox.Show(Strings.UnableToSaveMobile);
                Close();
                return;
            }

            MessageBox.Show(Strings.MobileAuthenticatorNotLinked + linker.LinkedAccount.RevocationCode);

            var finalizeResponse = AuthenticatorLinker.FinalizeResult.GeneralFailure;
            while (finalizeResponse != AuthenticatorLinker.FinalizeResult.Success)
            {
                InputForm smsCodeForm = new InputForm("Please input the SMS code sent to your phone.");
                smsCodeForm.ShowDialog();
                if (smsCodeForm.Canceled)
                {
                    manifest.RemoveAccount(linker.LinkedAccount);
                    Close();
                    return;
                }

                var confirmRevocationCode = new InputForm("Please enter your revocation code to ensure you've saved it.");
                confirmRevocationCode.ShowDialog();
                if (!confirmRevocationCode.txtBox.Text.Equals(linker.LinkedAccount.RevocationCode, StringComparison.CurrentCultureIgnoreCase))
                {
                    MessageBox.Show(Strings.InvalidRevocationCode);
                    manifest.RemoveAccount(linker.LinkedAccount);
                    Close();
                    return;
                }

                string smsCode = smsCodeForm.txtBox.Text;
                finalizeResponse = await linker.FinalizeAddAuthenticator(smsCode);

                switch (finalizeResponse)
                {
                    case AuthenticatorLinker.FinalizeResult.BadSMSCode:
                        continue;

                    case AuthenticatorLinker.FinalizeResult.UnableToGenerateCorrectCodes:
                        MessageBox.Show(Strings.UnableGenerateCode + linker.LinkedAccount.RevocationCode);
                        manifest.RemoveAccount(linker.LinkedAccount);
                        Close();
                        return;

                    case AuthenticatorLinker.FinalizeResult.GeneralFailure:
                        MessageBox.Show(Strings.UnableFinalizeAuthenticator + linker.LinkedAccount.RevocationCode);
                        manifest.RemoveAccount(linker.LinkedAccount);
                        Close();
                        return;
                }
            }

            //Linked, finally. Re-save with FullyEnrolled property.
            manifest.SaveAccount(linker.LinkedAccount, passKey != null, passKey);
            MessageBox.Show(Strings.SuccessLink + linker.LinkedAccount.RevocationCode);
            Close();
        }

        private void OnLoggedOff(SteamUser.LoggedOffCallback callback)
        {
            ArgumentNullException.ThrowIfNull(callback);

            steamClient.Disconnect();
        }

        private void OnDisconnected(SteamClient.DisconnectedCallback callback)
        {
            isRunning = false;
        }

        private void HandleManifest(Manifest man, bool isRefreshing = false)
        {
            string passKey = null;
            switch (man.Entries.Count)
            {
                case 0:
                    passKey = man.PromptSetupPassKey("Please enter an encryption passkey. Leave blank or hit cancel to not encrypt (VERY INSECURE).");
                    break;
                case > 0 when man.Encrypted:
                {
                    var passKeyValid = false;
                    while (!passKeyValid)
                    {
                        var passKeyForm = new InputForm("Please enter your current encryption passkey.");
                        passKeyForm.ShowDialog();
                        if (!passKeyForm.Canceled)
                        {
                            passKey = passKeyForm.txtBox.Text;
                            passKeyValid = man.VerifyPasskey(passKey);
                            if (!passKeyValid)
                            {
                                MessageBox.Show(Strings.InvalidPasskey, Strings.SteamLogin, MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                        else
                        {
                            Close();
                            return;
                        }
                    }

                    break;
                }
            }

            man.SaveAccount(account, passKey != null, passKey);
            if (isRefreshing)
            {
                MessageBox.Show(Strings.SessionRefreshed, Strings.SteamLogin, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show(Strings.SuccessLink + account.RevocationCode, Strings.SteamLogin, MessageBoxButtons.OK);
            }
            Close();
        }

        private void LoginForm_Load(object sender, EventArgs e)
        {
            if (account is { AccountName: not null })
            {
                txtUsername.Text = account.AccountName;
            }
        }

        public enum LoginType
        {
            Initial,
            Refresh,
            Import
        }
    }
}
