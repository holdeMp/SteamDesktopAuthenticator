using System.Collections.Specialized;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using SteamAuth;
using SteamWebAuthenticator.Models;

namespace SteamWebAuthenticator.Helpers;

public class UrlHelper(Account account)
{
    public string GenerateConfirmationUrl(string tag = "conf")
    {
        string endpoint = APIEndpoints.COMMUNITY_BASE + "/mobileconf/getlist?";
        string queryString = GenerateConfirmationQueryParams(tag);
        return endpoint + queryString;
    }

    public string GenerateConfirmationQueryParams(string? tag)
    {
        if (string.IsNullOrEmpty(account.DeviceId))
            throw new ArgumentException("Device ID is not present");

        var queryParams = GenerateConfirmationQueryParamsAsNvc(tag);

        return string.Join("&", queryParams.AllKeys.Select(key => $"{key}={queryParams[key]}"));
    }


    private NameValueCollection GenerateConfirmationQueryParamsAsNvc(string? tag)
    {
        if (string.IsNullOrEmpty(account.DeviceId))
            throw new ArgumentException("Device ID is not present");

        long time = TimeAligner.GetSteamTime();

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
        byte[] decode = Convert.FromBase64String(account.IdentitySecret);
        int n2 = 8;
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
        int n3 = 8;
        while (true)
        {
            int n4 = n3 - 1;
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
        byte[] hashedData = hmacGenerator.ComputeHash(array);
        string encodedData = Convert.ToBase64String(hashedData, Base64FormattingOptions.None);
        string hash = WebUtility.UrlEncode(encodedData);
        return hash;
    }
}