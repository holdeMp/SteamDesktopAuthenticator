using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using SteamAuth;

// ReSharper disable ClassNeverInstantiated.Global

namespace SteamWebAuthenticator.Models
{
    public class SteamGuardAccount
    {
        [JsonPropertyName("shared_secret")]
        public string SharedSecret { get; set; }

        [JsonPropertyName("serial_number")]
        public string SerialNumber { get; set; }

        [JsonPropertyName("revocation_code")]
        public string RevocationCode { get; set; }

        [JsonPropertyName("uri")]
        public string Uri { get; set; }

        [JsonPropertyName("server_time")]
        public long ServerTime { get; set; }

        [JsonPropertyName("account_name")]
        public string AccountName { get; set; }

        [JsonPropertyName("token_gid")]
        public string TokenGid { get; set; }


        [JsonPropertyName("secret_1")]
        public string Secret1 { get; set; }

        [JsonPropertyName("status")]
        public int Status { get; set; }

        [JsonPropertyName("device_id")]
        public string DeviceId { get; set; }
        
        public SessionData Session { get; set; }
        
        private static readonly byte[] SteamGuardCodeTranslations = "23456789BCDFGHJKMNPQRTVWXY"u8.ToArray();

        
        
        public string GenerateSteamGuardCode()
        {
            return GenerateSteamGuardCodeForTime(TimeAligner.GetSteamTime());
        }

        private string GenerateSteamGuardCodeForTime(long time)
        {
            if (string.IsNullOrEmpty(SharedSecret))
            {
                return "";
            }

            var sharedSecretUnescaped = Regex.Unescape(SharedSecret);
            var sharedSecretArray = Convert.FromBase64String(sharedSecretUnescaped);
            var timeArray = new byte[8];

            time /= 30L;

            for (var i = 8; i > 0; i--)
            {
                timeArray[i - 1] = (byte)time;
                time >>= 8;
            }

            var hmacGenerator = new HMACSHA1();
            hmacGenerator.Key = sharedSecretArray;
            var hashedData = hmacGenerator.ComputeHash(timeArray);
            var codeArray = new byte[5];
            
            var b = (byte)(hashedData[19] & 0xF);
            int codePoint = (hashedData[b] & 0x7F) << 24 | (hashedData[b + 1] & 0xFF) << 16 | (hashedData[b + 2] & 0xFF) << 8 | (hashedData[b + 3] & 0xFF);

            for (var i = 0; i < 5; ++i)
            {
                codeArray[i] = SteamGuardCodeTranslations[codePoint % SteamGuardCodeTranslations.Length];
                codePoint /= SteamGuardCodeTranslations.Length;
            }
          
            return Encoding.UTF8.GetString(codeArray);
        }
    }
}
