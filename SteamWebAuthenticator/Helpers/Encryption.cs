using System.Text;
using Jose;

namespace SteamAuth.Helpers;

public static class Encryption
{
    private static readonly byte[] SecretKey = Encoding.UTF8.GetBytes(Constants.Password);
    public static string Encrypt(this string plainText)
    {
        return JWT.Encode(plainText, SecretKey, Constants.EncodingALgo); 
    }

    public static string Decrypt(this string cipherText)
    {
        return JWT.Decode(cipherText, SecretKey, Constants.EncodingALgo); 
    }
}
