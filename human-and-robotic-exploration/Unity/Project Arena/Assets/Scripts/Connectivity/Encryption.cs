using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Polimi.GameCollective.Connectivity {

    /// <summary>
    /// Encryption utilities. Decryption is actually not required client-side.
    /// </summary>
    public class Encryption {

        public static string Encrypt(string messageString, string keyString, string ivString) {
            return EncryptRJ256(messageString, keyString, ivString);
        }

        public static string Decrypt(string messageString, string keyString, string ivString) {
            return DecryptRJ256(messageString, keyString, ivString);
        }

        public static string Hash(string message) {
            return HashMD5(message);
        }

        private static string EncryptRJ256(string messageString, string keyString, string ivString) {
            byte[] message = Encoding.ASCII.GetBytes(messageString);
            byte[] key = Encoding.ASCII.GetBytes(keyString);
            byte[] iv = Encoding.ASCII.GetBytes(ivString);

            RijndaelManaged algorithm = new RijndaelManaged() {
                Padding = PaddingMode.Zeros,
                Mode = CipherMode.CBC,
                KeySize = 256,
                BlockSize = 256
            };

            using (ICryptoTransform encryptor = algorithm.CreateEncryptor(key, iv)) {
                using (MemoryStream memoryStream = new MemoryStream()) {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor,
                        CryptoStreamMode.Write)) {
                        cryptoStream.Write(message, 0, message.Length);
                        cryptoStream.FlushFinalBlock();
                        byte[] encrypted = memoryStream.ToArray();
                        return (Convert.ToBase64String(encrypted));
                    }
                }
            }
        }

        private static string DecryptRJ256(string messageString, string keyString, string ivString) {
            byte[] message = Convert.FromBase64String(messageString);
            byte[] key = Encoding.ASCII.GetBytes(keyString);
            byte[] iv = Encoding.ASCII.GetBytes(ivString);

            RijndaelManaged algorithm = new RijndaelManaged() {
                Padding = PaddingMode.Zeros,
                Mode = CipherMode.CBC,
                KeySize = 256,
                BlockSize = 256
            };

            ICryptoTransform decryptor = algorithm.CreateDecryptor(key, iv);
            using (MemoryStream memoryStream = new MemoryStream(message)) {
                using (CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor,
                    CryptoStreamMode.Read)) {
                    byte[] decrypted = new byte[message.Length];
                    cryptoStream.Read(decrypted, 0, decrypted.Length);
                    return (Encoding.ASCII.GetString(decrypted));
                }
            }
        }

        private static string HashMD5(string message) {
            using (MD5 md5 = MD5.Create()) {
                byte[] inputBytes = Encoding.UTF8.GetBytes(message);
                byte[] hashBytes = md5.ComputeHash(inputBytes);
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++) {
                    builder.Append(hashBytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }

}