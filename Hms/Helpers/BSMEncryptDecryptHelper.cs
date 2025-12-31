using System.Security.Cryptography;
using System.Text;

namespace Hms.Helpers
{
    public static class BSMEncryptDecryptHelper
    {
        public static string Encrypt(string plainText, string key, int keySize = 256)
        {
            if (string.IsNullOrEmpty(plainText) || string.IsNullOrEmpty(key))
                throw new ArgumentNullException("Plain text or key cannot be null");

            byte[] keyBytes = GetKeyBytes(key, keySize);
            byte[] iv = new byte[16];

            using (Aes aes = Aes.Create())
            {
                aes.Key = keyBytes;
                aes.IV = iv;

                using (ICryptoTransform encryptor = aes.CreateEncryptor())
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    using (StreamWriter sw = new StreamWriter(cs))
                    {
                        sw.Write(plainText);
                    }

                    byte[] encrypted = ms.ToArray();
                    return Convert.ToBase64String(encrypted);
                }
            }
        }

        public static string Decrypt(string cipherText, string key, int keySize = 256)
        {
            if (string.IsNullOrEmpty(cipherText) || string.IsNullOrEmpty(key))
                throw new ArgumentNullException("Cipher text or key cannot be null");

            byte[] keyBytes = GetKeyBytes(key, keySize);
            byte[] iv = new byte[16]; // Same IV used for encryption
            byte[] cipherBytes = Convert.FromBase64String(cipherText);

            using (Aes aes = Aes.Create())
            {
                aes.Key = keyBytes;
                aes.IV = iv;

                using (ICryptoTransform decryptor = aes.CreateDecryptor())
                using (MemoryStream ms = new MemoryStream(cipherBytes))
                using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                using (StreamReader sr = new StreamReader(cs))
                {
                    return sr.ReadToEnd();
                }
            }
        }

        private static byte[] GetKeyBytes(string key, int keySize)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(key));
                if (keySize == 256)
                    return hash;

                byte[] keyBytes = new byte[keySize / 8];
                Array.Copy(hash, keyBytes, Math.Min(hash.Length, keyBytes.Length));
                return keyBytes;
            }
        }
    }

}
