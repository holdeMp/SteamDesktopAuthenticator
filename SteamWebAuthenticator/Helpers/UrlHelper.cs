using System.Collections.Specialized;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using SteamWebAuthenticator.Models;

namespace SteamWebAuthenticator.Helpers;

public class UrlHelper(Account account)
{
    public async Task<string> GenerateConfirmationUrlAsync(string tag = "conf")
    {
        const string endpoint = ApiEndpoints.CommunityBase + "/mobileconf/getlist?";
        var queryString = await GenerateConfirmationQueryParamsAsync(tag);
        return endpoint + queryString;
    }

    public async Task<string> GenerateConfirmationQueryParamsAsync(string? tag)
    {
        if (string.IsNullOrEmpty(account.DeviceId))
            throw new ArgumentException("Device ID is not present");

        var queryParams = await GenerateConfirmationQueryParamsAsNvcAsync(tag);

        return string.Join("&", queryParams.AllKeys.Select(key => $"{key}={queryParams[key]}"));
    }


    private async Task<NameValueCollection> GenerateConfirmationQueryParamsAsNvcAsync(string? tag)
    {
        if (string.IsNullOrEmpty(account.DeviceId))
            throw new ArgumentException("Device ID is not present");

        var time = await TimeAligner.GetSteamTimeAsync();

        var ret = new NameValueCollection
        {
            { "p", account.DeviceId },
            { "a", account.SteamId.ToString() },
            { "k", _generateConfirmationHashForTime(time, tag) },
            { "t", time.ToString() },
            { "m", "react" },
            { "tag", tag }
        };

        return ret;
    }
    
    private string _generateConfirmationHashForTime(long time, string? tag)
    {
        var decode = Convert.FromBase64String(account.IdentitySecret);
        var n2 = 8;
        if (tag != null)
        {
            if (tag.Length > 32)
            {
                n2 = 8 + 32;
            }
            else
            {
                n2 = 8 + tag.Length;
            }
        }
        var array = new byte[n2];
        var n3 = 8;
        while (true)
        {
            var n4 = n3 - 1;
            if (n3 <= 0)
            {
                break;
            }
            array[n4] = (byte)time;
            time >>= 8;
            n3 = n4;
        }
        if (tag != null)
        {
            Array.Copy(Encoding.UTF8.GetBytes(tag), 0, array, 8, n2 - 8);
        }
        var hmacGenerator = new HMACSHA1();
        hmacGenerator.Key = decode;
        var hashedData = hmacGenerator.ComputeHash(array);
        var encodedData = Convert.ToBase64String(hashedData, Base64FormattingOptions.None);
        var hash = WebUtility.UrlEncode(encodedData);
        return hash;
    }
}