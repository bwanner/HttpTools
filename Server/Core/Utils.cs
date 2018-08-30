using System.Security.Cryptography;
using System.Text;

namespace Batzill.Server.Core
{
    public static class Utils
    {
        public static string CalculateMD5Hash(string input)
        {
            // step 1, calculate MD5 hash from input
            MD5 md5 = MD5.Create();
            byte[] inputBytes = Encoding.ASCII.GetBytes(input);

            byte[] hash = md5.ComputeHash(inputBytes);
            // step 2, convert byte array to hex string
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)

            {
                sb.Append(hash[i].ToString("x2"));
            }

            return sb.ToString();
        }

        public static string GenerateKeyHash(string key)
        {
            // add some salt + repitition to make rainbow attack harder
            const int rep = 42;
            const string salt = "bae83a2e-13cd-4b85-a670-24ebd1d88e99";
            
            for(int i = 0; i < rep; i++)
            {
                key = Utils.CalculateMD5Hash(string.Format($"{key}#{salt}"));
            }

            return key;
        }
    }
}
