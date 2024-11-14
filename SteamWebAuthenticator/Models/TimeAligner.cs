﻿using System;
using System.Net;
using System.Net.Http;
using System.Text;
using SteamAuth.Helpers;
using SteamAuth.Models;
using SteamWebAuthenticator.Helpers;

namespace SteamAuth
{
    /// <summary>
    /// Class to help align system time with the Steam server time. Not super advanced; probably not taking some things into account that it should.
    /// Necessary to generate up-to-date codes. In general, this will have an error of less than a second, assuming Steam is operational.
    /// </summary>
    public static class TimeAligner
    {
        private static bool _aligned;
        private static int _timeDifference;
        
        public static long GetSteamTime()
        {
            if (!_aligned)
            {
                AlignTime();
            }
            return GetSystemUnixTime() + _timeDifference;
        }

        private static async void AlignTime()
        {
            var currentTime = GetSystemUnixTime();
            using var client = new HttpClient();
            var jsonString = await "steamid=0".ToJsonAsync();
            var content = new StringContent(jsonString, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await client.PostAsync(APIEndpoints.TWO_FACTOR_TIME_QUERY, content);
            var responseContent = await response.Content.ReadAsStringAsync();
            var query = await responseContent.FromJsonAsync<TimeQuery>();
            _timeDifference = (int)(int.Parse(query.Response.ServerTime) - currentTime);
            _aligned = true;
        }

        private static long GetSystemUnixTime()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }
    }
}
