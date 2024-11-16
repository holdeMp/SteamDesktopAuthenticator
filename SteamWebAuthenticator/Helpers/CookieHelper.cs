using System.Net;
using System.Text;
using Core;
using SteamAuth;
using SteamWebAuthenticator.Models;

namespace SteamWebAuthenticator.Helpers;

public static class CookieHelper
{
    public static CookieContainer GetCookies(Account account)
    {
        if (account == null)
            throw new Exception(Messages.SelectedAccountIsEmpty);
        if (string.IsNullOrWhiteSpace(account.SessionId))
            account.SessionId = GenerateSessionId();
        var cookies = new CookieContainer();
        foreach (var domain in new[] { "steamcommunity.com", "store.steampowered.com" })
        {
            cookies.Add(new Cookie("steamLoginSecure", GetSteamLoginSecure(account), "/", domain));
            cookies.Add(new Cookie("sessionid", account.SessionId, "/", domain));
            cookies.Add(new Cookie("mobileClient", "android", "/", domain));
            cookies.Add(new Cookie("mobileClientVersion", "777777 3.6.4", "/", domain));
        }
        return cookies;
    }
    
    public static string CookieContainerToString(CookieContainer cookieContainer, Uri uri)
    {
        var cookieString = new StringBuilder();
        
        var cookies = cookieContainer.GetCookies(uri);

        foreach (Cookie cookie in cookies)
        {
            cookieString.Append($"{cookie.Name}={cookie.Value}; ");
        }

        if (cookieString.Length > 0)
        {
            cookieString.Length -= 2;
        }

        return cookieString.ToString();
    }

    private static string GetSteamLoginSecure(Account account)
    {
        if (account == null)
            throw new Exception(Messages.SelectedAccountIsEmpty);
        return account.SteamId + "%7C%7C" + account.SteamAccessToken;
    }
    
    public static string GenerateSessionId()
    {
        return GetRandomHexNumber(32);
    }

    private static string GetRandomHexNumber(int digits)
    {
        var random = new Random();
        var buffer = new byte[digits / 2];
        random.NextBytes(buffer);
        var result = string.Concat(buffer.Select(x => x.ToString("X2")).ToArray());
        if (digits % 2 == 0)
            return result;
        return result + random.Next(16).ToString("X");
    }
}