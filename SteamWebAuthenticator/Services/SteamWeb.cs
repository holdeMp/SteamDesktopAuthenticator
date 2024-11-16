using System.Net;
using System.Text;
using SteamAuth;
using SteamWebAuthenticator.Helpers;
using SteamWebAuthenticator.Models;
using SteamWebAuthenticator.Models.Responses;

namespace SteamWebAuthenticator.Services
{
    public class SteamWeb
    {
        private const string MobileAppUserAgent = "okhttp/3.12.12";
        private readonly HttpClient httpClient;
        private readonly UrlHelper urlHelper;
        
        public SteamWeb(Account account, CookieContainer cookies)
        {
            var handler = new HttpClientHandler
            {
                CookieContainer = cookies,
                UseCookies = true
            };
            httpClient = new HttpClient(handler);

            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", MobileAppUserAgent);

            urlHelper = new UrlHelper(account);
        }

        public async Task<bool> SendConfirmationAjax(Confirmation conf, string op)
        {
            var url = "https://steamcommunity.com/mobileconf/ajaxop";
            string queryString = "?op=" + op + "&";
            // tag is different from op now
            string tag = op == Constants.Allow ? Constants.Accept :Constants.Reject;
            queryString += urlHelper.GenerateConfirmationQueryParams(tag);
            queryString += "&cid=" + conf.Id + "&ck=" + conf.Key;
            url += queryString;

            string response = await GetRequestAsync(url);

            var confResponse = await response.FromJsonAsync<SendConfirmationResponse>();
            return confResponse.Success;
        }

        public async Task<bool> SendMultiConfirmationAjax(List<Confirmation> confirmations, string op)
        {
            string url = APIEndpoints.COMMUNITY_BASE + "/mobileconf/multiajaxop";
            // tag is different from op now
            string tag = op ==Constants.Allow ? Constants.Accept :Constants.Reject;
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

            var responseContent = await response.Content.ReadAsStringAsync();

            var confResponse = await responseContent.FromJsonAsync<SendConfirmationResponse>();
            return confResponse.Success;
        }
        
        public async Task<ConfirmationsResponse> GetConfUrlAsync(string url)
        {
            var respContent = await httpClient.GetStringAsync(url);
            var response = await respContent.FromJsonAsync<ConfirmationsResponse>();
            return response;
        }
        
        public async Task<string> GetRequestAsync(string url)
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
