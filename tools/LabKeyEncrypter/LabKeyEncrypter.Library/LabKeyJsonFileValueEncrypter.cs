using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.Json;

namespace LabKeyEncrypter.Library;
public class LabKeyJsonFileValueEncrypter
{


    public static void EncryptJsonValues(string filePath, string password)
    {
        string jsonString = File.ReadAllText(filePath);
        var jsonObject = JObject.Parse(jsonString);

        foreach (var property in jsonObject.Properties())
        {
            var plainText = property.Value.ToString();
            var encrypted = LabKeyEncrypter.Encrypt(plainText, password);
            property.Value = encrypted;
        }
        var encryptedFilePath = filePath.Replace(".json", "_encrypted.json");

        File.WriteAllText(encryptedFilePath, jsonObject.ToString());
    }

    public static bool DecryptJsonValues(string filePath, string password)
    {
        string jsonString = File.ReadAllText(filePath);
        var jsonObject = JObject.Parse(jsonString);

        foreach (var property in jsonObject.Properties())
        {
            var encrypted = property.Value.ToString();
            var decrypted = LabKeyEncrypter.Decrypt(encrypted, password);
            property.Value = JToken.Parse(decrypted); // Parse the decrypted value to ensure it's properly formatted
        }

        var decryptedFilePath = filePath.Replace("_encrypted.json", ".json");

        File.WriteAllText(decryptedFilePath, jsonObject.ToString());
        return true;
    }

}