using System.Diagnostics.CodeAnalysis;
using Microsoft.IdentityModel.JsonWebTokens;
using Serilog;

namespace SteamWebAuthenticator.Helpers;

public static class Utilities
{
    public static void InBackground<T>(Func<T> function, bool longRunning = false) {
        ArgumentNullException.ThrowIfNull(function);

        InBackground(void () => function(), longRunning);
    }

    private static async void InBackground(Action action, bool longRunning = false) {
        ArgumentNullException.ThrowIfNull(action);

        var options = TaskCreationOptions.DenyChildAttach;

        if (longRunning) {
            options |= TaskCreationOptions.LongRunning | TaskCreationOptions.PreferFairness;
        }

        await Task.Factory.StartNew(action, CancellationToken.None, options, TaskScheduler.Default).ConfigureAwait(false);
    }
    
    public static bool TryReadJsonWebToken(string token, [NotNullWhen(true)] out JsonWebToken? result) {
        ArgumentException.ThrowIfNullOrEmpty(token);

        try {
            result = new JsonWebToken(token);
        } catch (Exception e) {
            Log.Error(e.Message + e.StackTrace);

            result = null;

            return false;
        }

        return true;
    }
}