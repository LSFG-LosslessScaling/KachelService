using System.Security.Cryptography;
using System.Text;

namespace KachelService.Services
{
    public class Aes
    {

        private string Password = "fregfreHGGHJgjh76hgGJHG((hjk";


        public string EncryptToBase64(string originalText)
        {
            var userBytes = Encoding.UTF8.GetBytes(originalText); // UTF8 saves Space
            SymmetricAlgorithm crypt = System.Security.Cryptography.Aes.Create(); // (Default: AES-CCM (Counter with CBC-MAC))
            crypt.Key = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(Password)); // MD5: 128 Bit Hash
            crypt.IV = new byte[16]; // by Default. IV[] to 0.. is OK simple crypt
            using var memoryStream = new MemoryStream();
            using var cryptoStream = new CryptoStream(memoryStream, crypt.CreateEncryptor(), CryptoStreamMode.Write);
            cryptoStream.Write(userBytes, 0, userBytes.Length); // User Data
            cryptoStream.FlushFinalBlock();
            var resultString = Convert.ToBase64String(memoryStream.ToArray());

            resultString = resultString.Replace("+", "%2B");
            resultString = resultString.Replace("/", "%2F");
            resultString = resultString.Replace("=", "%3D");

            return resultString;
        }

        public string DecryptFromBase64(string encryptedText)
        {
            encryptedText = encryptedText.Replace("%2B", "+");
            encryptedText = encryptedText.Replace("%2F", "/");
            encryptedText = encryptedText.Replace("%3D", "=");

            var encryptedBytes = Convert.FromBase64String(encryptedText);
            SymmetricAlgorithm crypt = System.Security.Cryptography.Aes.Create();
            crypt.Key = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(Password));
            crypt.IV = new byte[16];
            using var memoryStream = new MemoryStream();
            using var cryptoStream = new CryptoStream(memoryStream, crypt.CreateDecryptor(), CryptoStreamMode.Write);
            cryptoStream.Write(encryptedBytes, 0, encryptedBytes.Length);
            cryptoStream.FlushFinalBlock();
            var allBytes = memoryStream.ToArray();
            var userLen = allBytes.Length;
            var resultString = Encoding.UTF8.GetString(allBytes, 0, userLen);

            return resultString;
        }
    }
}
