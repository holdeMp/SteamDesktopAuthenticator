using System;
using System.Windows.Forms;
using System.Diagnostics;
using CommandLine;

namespace Steam_Desktop_Authenticator
{
    internal static class Program
    {
        private static Process PriorProcess()
        // Returns a System.Diagnostics.Process pointing to
        // a pre-existing process with the same name as the
        // current one, if any; or null if the current process
        // is unique.
        {
            try
            {
                var curr = Process.GetCurrentProcess();
                var procs = Process.GetProcessesByName(curr.ProcessName);
                foreach (Process p in procs)
                {
                    if ((p.Id != curr.Id) &&
                        (p.MainModule.FileName == curr.MainModule.FileName))
                        return p;
                }
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main(string[] args)
        {
            // run the program only once
            if (PriorProcess() != null)
            {
                MessageBox.Show(Strings.AnotherInstance);
                return;
            }

            // Parse command line arguments
            var result = Parser.Default.ParseArguments<CommandLineOptions>(args);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Manifest man;

            try
            {
                man = Manifest.GetManifest();
            }
            catch (ManifestParseException)
            {
                // Manifest file was corrupted, generate a new one.
                try
                {
                    MessageBox.Show(@"Your settings were unexpectedly corrupted and were reset to defaults.", @"Steam Desktop Authenticator", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    man = Manifest.GenerateNewManifest(true);
                }
                catch (MaFileEncryptedException)
                {
                    // An maFile was encrypted, we're fucked.
                    MessageBox.Show(@"Sorry, but SDA was unable to recover your accounts since you used encryption.
You'll need to recover your Steam accounts by removing the authenticator.
Click OK to view instructions.", @"Steam Desktop Authenticator", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Process.Start(@"https://github.com/Jessecar96/SteamDesktopAuthenticator/wiki/Help!-I'm-locked-out-of-my-account");
                    return;
                }
            }

            if (man.FirstRun)
            {
                if (man.Entries.Count > 0)
                {
                    // Already has accounts, just run
                    MainForm mf = new MainForm();
                    mf.SetEncryptionKey(result.Value.EncryptionKey);
                    mf.StartSilent(result.Value.Silent);
                    Application.Run(mf);
                }
                else
                {
                    // No accounts, run welcome form
                    Application.Run(new WelcomeForm());
                }
            }
            else
            {
                MainForm mf = new MainForm();
                mf.SetEncryptionKey(result.Value.EncryptionKey);
                mf.StartSilent(result.Value.Silent);
                Application.Run(mf);
            }
        }
    }
}
