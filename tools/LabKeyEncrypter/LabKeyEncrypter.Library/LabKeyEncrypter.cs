using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace LabKeyEncrypter.Library;
public class LabKeyEncrypter
{
    private static readonly int SaltSize = 16; // 128 bit
    private static readonly int KeySize = 32; // 256 bit
    private static readonly int Iterations = 10000;

#pragma warning disable SYSLIB0041

    public static string Encrypt(string plainText, string password)
    {
        using (var aes = Aes.Create())
        {
            var salt = GenerateSalt();
            var key = new Rfc2898DeriveBytes(password, salt, Iterations).GetBytes(KeySize);

            aes.Key = key;
            aes.GenerateIV();

            using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
            using (var ms = new MemoryStream())
            {
                using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                using (var sw = new StreamWriter(cs))
                {
                    sw.Write(plainText);
                }

                var iv = aes.IV;
                var encrypted = ms.ToArray();

                var result = new byte[salt.Length + iv.Length + encrypted.Length];
                Buffer.BlockCopy(salt, 0, result, 0, salt.Length);
                Buffer.BlockCopy(iv, 0, result, salt.Length, iv.Length);
                Buffer.BlockCopy(encrypted, 0, result, salt.Length + iv.Length, encrypted.Length);

                return Convert.ToBase64String(result);
            }
        }
    }

    public static string Decrypt(string cipherText, string password)
    {
        var fullCipher = Convert.FromBase64String(cipherText);

        var salt = new byte[SaltSize];
        Buffer.BlockCopy(fullCipher, 0, salt, 0, salt.Length);

        var iv = new byte[16];
        Buffer.BlockCopy(fullCipher, salt.Length, iv, 0, iv.Length);

        var cipher = new byte[fullCipher.Length - salt.Length - iv.Length];
        Buffer.BlockCopy(fullCipher, salt.Length + iv.Length, cipher, 0, cipher.Length);

        var key = new Rfc2898DeriveBytes(password, salt, Iterations).GetBytes(KeySize);

        using (var aes = Aes.Create())
        {
            aes.Key = key;
            aes.IV = iv;

            using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
            using (var ms = new MemoryStream(cipher))
            using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
            using (var sr = new StreamReader(cs))
            {
                return sr.ReadToEnd();
            }
        }
    }

    private static byte[] GenerateSalt()
    {
        var salt = new byte[SaltSize];
        RandomNumberGenerator.Fill(salt);
        return salt;
    }
}