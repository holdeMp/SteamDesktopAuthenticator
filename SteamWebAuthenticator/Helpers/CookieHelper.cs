using System.Net;
using System.Text;
using Core;
using SteamWebAuthenticator.Models;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global

namespace SteamWebAuthenticator.Helpers;

public static class CookieHelper
{
    private const string SteamCommunityCom = "steamcommunity.com";
    public static CookieContainer GetCookies(Account account)
    {
        if (account == null)
            throw new InvalidOperationException(Messages.SelectedAccountIsEmpty);
        if (string.IsNullOrWhiteSpace(account.SessionId))
            account.SessionId = GenerateSessionId();
        var cookies = new CookieContainer();
        foreach (var domain in new[] { SteamCommunityCom, "store.steampowered.com" })
        {
            cookies.Add(new Cookie("steamLoginSecure", GetSteamLoginSecure(account), "/", domain));
            cookies.Add(new Cookie("sessionid", account.SessionId, "/", domain));
            cookies.Add(new Cookie("mobileClient", "android", "/", domain));
            cookies.Add(new Cookie("mobileClientVersion", "777777 3.6.4", "/", domain));
        }
        return cookies;
    }
    
    public static List<Cookie> GetCommonSteamCookies(Account account)
    {
        var cookies = new List<Cookie>();
        var currentTimestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
        var updatedTradeEligibility = $"%7B%22allowed%22%3A1%2C%22allowed_at_time%22%3A0%2C%22steamguard_required_days%22%3A15%2C%22new_device_cooldown_days%22%3A0%2C%22time_checked%22%3A{currentTimestamp}%7D";
        foreach (var domain in new[] { SteamCommunityCom, "store.steampowered.com" })
        {
            const string steam = "steam";
            var commonCookies = new List<Cookie>
            {
                new("extproviders_252490", steam, "/", domain),
                new("extproviders_440", steam, "/", domain),
                new("extproviders_570", steam, "/", domain),
                new("extproviders_578080", steam, "/", domain),
                new("extproviders_730", steam, "/", domain),
                new("extproviders_753", steam, "/", domain),
                new("steamCurrencyId", "18", "/", domain),
                new("browserid", "3242113653530249135", "/", domain),
                new("timezoneOffset", "7200%2C0", "/", domain),
                new("recentlyVisitedAppHubs", 
                    "236850%2C335300%2C431960%2C105600%2C1222140%2C629760%2C1238860%2C317400%2C374320%2C553850%2C1245620%2C730%2C1538570%2C1568590%2C440%" +
                    "2C393380%2C570", "/", SteamCommunityCom),
                new("webTradeEligibility",updatedTradeEligibility, "/", domain),
                new("rgDiscussionPrefs", "%7B%22cTopicRepliesPerPage%22%3A50%7D", "/", domain),
                new("steamCountry", "UA%7Cfba0df89ce417fea9c64dc15e853dc9a", "/", domain),
                new("strInventoryLastContext", "730_2", "/", SteamCommunityCom)
            };
            foreach (var cookie in commonCookies)
            {
                cookies.Add(cookie);
                cookies.Add(new Cookie("steamLoginSecure", GetSteamLoginSecure(account), "/", domain));
                cookies.Add(new Cookie("sessionid", account.SessionId, "/", domain));
                cookies.Add(new Cookie("mobileClient", "android", "/", domain));
                cookies.Add(new Cookie("mobileClientVersion", "777777 3.6.4", "/", domain));
            }
        }

        return cookies;
    }

    public static string GetSteamLoginSecure(Account account)
    {
        if (account == null)
            throw new InvalidOperationException(Messages.SelectedAccountIsEmpty);
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