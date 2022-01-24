using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace OAuth2AuthenticationBroker.Functions
{
    internal static class HashGenerator
    {
        public static string ComputeStringToSha256Hash(string plainText)
        {
            string result = "";
            // Create a SHA256 hash from string   
            using (var sha256 = SHA256.Create())
            {
                var challengeBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(plainText));
                result = Convert.ToBase64String(challengeBytes)
                    .TrimEnd('=')
                    .Replace('+', '-')
                    .Replace('/', '_');
            }
            return result;
        }
    }
}
