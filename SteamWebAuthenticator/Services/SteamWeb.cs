using System.Net;
using System.Net.Http.Headers;
using System.Text;
using SteamAuth;
using SteamAuth.Helpers;
using SteamWebAuthenticator.Helpers;
using SteamWebAuthenticator.Models;

namespace SteamWebAuthenticator.Services
{
    public class SteamWeb
    {
        private const string MobileAppUserAgent = "okhttp/3.12.12";
        private readonly HttpClient _httpClient;
        private readonly UrlHelper _urlHelper;
        
        public SteamWeb(Account account, CookieContainer cookies)
        {
            var handler = new HttpClientHandler
            {
                CookieContainer = cookies,
                UseCookies = true
            };
            _httpClient = new HttpClient(handler);

            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", MobileAppUserAgent);

            _urlHelper = new UrlHelper(account);
        }

        public async Task<bool> SendConfirmationAjax(Confirmation conf, string op)
        {
            var url = "https://steamcommunity.com/mobileconf/ajaxop";
            string queryString = "?op=" + op + "&";
            // tag is different from op now
            string tag = op == Constants.Allow ? Constants.Accept :Constants.Reject;
            queryString += _urlHelper.GenerateConfirmationQueryParams(tag);
            queryString += "&cid=" + conf.Id + "&ck=" + conf.Key;
            url += queryString;

            string response = await GetRequest(url);

            var confResponse = await response.FromJsonAsync<SendConfirmationResponse>();
            return confResponse.Success;
        }

        public async Task<bool> SendMultiConfirmationAjax(List<Confirmation> confirmations, string op)
        {
            string url = APIEndpoints.COMMUNITY_BASE + "/mobileconf/multiajaxop";
            // tag is different from op now
            string tag = op ==Constants.Allow ? Constants.Accept :Constants.Reject;
            string query = "op=" + op + "&" + _urlHelper.GenerateConfirmationQueryParams(tag);
            query = confirmations
                .Aggregate(query, (current, conf) => current + ("&cid[]=" + conf.Id + "&ck[]=" + conf.Key));

            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(SteamWeb.MobileAppUserAgent);
            _httpClient.DefaultRequestHeaders.AcceptEncoding.ParseAdd("gzip, deflate");
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation(
                "Content-Type", "application/x-www-form-urlencoded; charset=UTF-8");
            var content = new StringContent(query, Encoding.UTF8, "application/x-www-form-urlencoded");
            var response = _httpClient.PostAsync(url, content).Result;
        
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();

            var confResponse = await responseContent.FromJsonAsync<SendConfirmationResponse>();
            return confResponse.Success;
        }
        
        public async Task<string> GetRequest(string url)
        {
            var response = await _httpClient.GetStringAsync(url);
            return response;
        }

        public async Task<string> PostRequest(string url, Dictionary<string, string> body)
        {
            var content = new FormUrlEncodedContent(body);
            var response = await _httpClient.PostAsync(url, content);
            response.EnsureSuccessStatusCode(); // Throw if not a success code.
            return await response.Content.ReadAsStringAsync();
        }
    }
}
