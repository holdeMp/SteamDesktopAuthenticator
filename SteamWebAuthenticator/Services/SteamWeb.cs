using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using SteamAuth.Helpers;
using SteamWebAuthenticator;
using SteamWebAuthenticator.Services;

namespace SteamAuth
{
    public class SteamWeb
    {
        private const string MobileAppUserAgent = "okhttp/3.12.12";
        private readonly HttpClient httpClient;
        private readonly UrlHelper urlHelper;
        
        public SteamWeb(SteamGuardAccount account)
        {
            // Create an HttpClientHandler to handle cookies
            var handler = new HttpClientHandler
            {
                CookieContainer = account.Session.GetCookies(),
                UseCookies = true
            };

            // Initialize HttpClient with the handler
            httpClient = new HttpClient(handler)
            {
                DefaultRequestHeaders =
                {
                    UserAgent = { new ProductInfoHeaderValue(MobileAppUserAgent) }
                }
            };

            urlHelper = new UrlHelper(account);
        }

        public async Task<bool> SendConfirmationAjax(Confirmation conf, string op)
        {
            var url = "https://steamcommunity.com/mobileconf/ajaxop";
            string queryString = "?op=" + op + "&";
            // tag is different from op now
            string tag = op == Constants.Allow ?Constants.Accept :Constants.Reject;
            queryString += urlHelper.GenerateConfirmationQueryParams(tag);
            queryString += "&cid=" + conf.Id + "&ck=" + conf.Key;
            url += queryString;

            string response = await GetRequest(url);

            var confResponse = await response.FromJsonAsync<SendConfirmationResponse>();
            return confResponse.Success;
        }

        public async Task<bool> SendMultiConfirmationAjax(Confirmation[] confirmations, string op)
        {
            string url = APIEndpoints.COMMUNITY_BASE + "/mobileconf/multiajaxop";
            // tag is different from op now
            string tag = op ==Constants.Allow ?Constants.Accept :Constants.Reject;
            string query = "op=" + op + "&" + urlHelper.GenerateConfirmationQueryParams(tag);
            query = confirmations
                .Aggregate(query, (current, conf) => current + ("&cid[]=" + conf.Id + "&ck[]=" + conf.Key));

            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(SteamWeb.MobileAppUserAgent);
            httpClient.DefaultRequestHeaders.AcceptEncoding.ParseAdd("gzip, deflate");
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation(
                "Content-Type", "application/x-www-form-urlencoded; charset=UTF-8");
            var content = new StringContent(query, Encoding.UTF8, "application/x-www-form-urlencoded");
            var response = httpClient.PostAsync(url, content).Result;
        
            response.EnsureSuccessStatusCode();

            var responseContent = response.Content.ReadAsStringAsync().Result;

            var confResponse = await responseContent.FromJsonAsync<SendConfirmationResponse>();
            return confResponse.Success;
        }
        
        public async Task<string> GetRequest(string url)
        {
            var response = await httpClient.GetStringAsync(url);
            return response;
        }

        public async Task<string> PostRequest(string url, Dictionary<string, string> body)
        {
            var content = new FormUrlEncodedContent(body);
            var response = await httpClient.PostAsync(url, content);
            response.EnsureSuccessStatusCode(); // Throw if not a success code.
            return await response.Content.ReadAsStringAsync();
        }
    }
}
