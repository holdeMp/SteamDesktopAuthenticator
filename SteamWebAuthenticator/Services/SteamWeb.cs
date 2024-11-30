using System.Net;
using System.Text;
using SteamWebAuthenticator.Helpers;
using SteamWebAuthenticator.Models;
using SteamWebAuthenticator.Models.Responses;
// ReSharper disable SuggestVarOrType_BuiltInTypes
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global

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
            string tag = op == Constants.Allow ? Constants.Accept : Constants.Reject;
            queryString += await _urlHelper.GenerateConfirmationQueryParamsAsync(tag);
            queryString += "&cid=" + conf.Id + "&ck=" + conf.Key;
            url += queryString;
            await GetRequestAsync(url);
            return true;
        }

        public async Task<bool> SendMultiConfirmationAjax(List<Confirmation> confirmations, string op)
        {
            const string url = ApiEndpoints.CommunityBase + "/mobileconf/multiajaxop";
            // tag is different from op now
            string tag = op == Constants.Allow ? Constants.Accept : Constants.Reject;
            string query = "op=" + op + "&" + await _urlHelper.GenerateConfirmationQueryParamsAsync(tag);
            query = confirmations
                .Aggregate(query, (current, conf) => current + "&cid[]=" + conf.Id + "&ck[]=" + conf.Key);

            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(MobileAppUserAgent);
            _httpClient.DefaultRequestHeaders.AcceptEncoding.ParseAdd("gzip, deflate");
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation(
                "Content-Type", "application/x-www-form-urlencoded; charset=UTF-8");
            var content = new StringContent(query, Encoding.UTF8, "application/x-www-form-urlencoded");
            var response = await _httpClient.PostAsync(url, content);
        
            response.EnsureSuccessStatusCode();
            return true;
        }
        
        public async Task<ConfirmationsResponse> GetConfUrlAsync(string url)
        {
            var respContent = await _httpClient.GetStringAsync(url);
            var response = await respContent.FromJsonAsync<ConfirmationsResponse>();
            return response;
        }
        
        public async Task<string> GetRequestAsync(string url)
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
