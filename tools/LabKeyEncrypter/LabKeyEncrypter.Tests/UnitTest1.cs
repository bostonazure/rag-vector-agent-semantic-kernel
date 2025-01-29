using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Security.Cryptography;
using Xunit;

namespace LabKeyEncrypter.Library;

public class UnitTest1
{
    [Fact]
    public void EncryptDecrypt_MatchesOriginal()
    {
        var plainText = "This is a secret";
        var password = "password";

        var encrypted = LabKeyEncrypter.Encrypt(plainText, password);
        var decrypted = LabKeyEncrypter.Decrypt(encrypted, password);

        Assert.Equal(plainText, decrypted);
    }

    [Fact]
    public void Encrypt_FileMatchesOriginal()
    {
        var filePath = "UnencryptedSimple.json";
        var password = "password";

        LabKeyJsonFileValueEncrypter.EncryptJsonValues(filePath, password);

        Assert.True(File.Exists(filePath.Replace(".json", "_encrypted.json")));
    }

    [Fact]
    public void Decrypt_FileMatchesOriginal()
    {
        var filePath = "UnencryptedSimple_encrypted.json";
        var password = "password";

        LabKeyJsonFileValueEncrypter.DecryptJsonValues(filePath, password);

        Assert.True(File.Exists(filePath.Replace("_encrypted.json", ".json")));
    }

    [Fact]
    public void EncryptDecrypt_HashMatchesOriginal()
    {
        var filePath1 = "UnencryptedSimple.json";
        var password = "password";
        string originalHash;
        string newHash;

        using (var sha256Original = SHA256.Create())
        {
            var originalBytes = File.ReadAllBytes(filePath1);
            var originalHashBytes = sha256Original.ComputeHash(originalBytes);
            originalHash = BitConverter.ToString(originalHashBytes).Replace("-", "").ToLowerInvariant();
        }

        LabKeyJsonFileValueEncrypter.EncryptJsonValues(filePath1, password);

        var filePath2 = filePath1.Replace(".json", "_encrypted.json");

        LabKeyJsonFileValueEncrypter.DecryptJsonValues(filePath2, password);

        using (var sha256New = SHA256.Create())
        {
            var newBytes = File.ReadAllBytes(filePath1);
            var newHashBytes = sha256New.ComputeHash(newBytes);
            newHash = BitConverter.ToString(newHashBytes).Replace("-", "").ToLowerInvariant();
        }
        
        Assert.Equal(originalHash, newHash);
    }
}
