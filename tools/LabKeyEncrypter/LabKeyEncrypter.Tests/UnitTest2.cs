using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using Xunit;

namespace LabKeyEncrypter.Library;

public class UnitTest2
{
    [Fact]
    public void TestEncryptAndWriteToJson()
    {
        /*string filePath = "secret.json";
        string jsonString = File.ReadAllText(filePath);

        var jsonObject = JsonSerializer.Deserialize<JsonElement>(jsonString);
        var plainText = jsonObject.GetProperty("secret").GetString();
        var password = "password";

        var encrypted = LabKeyEncrypter.Encrypt(plainText, password);

        // Create a new JSON object with the updated value
        var updatedJsonObject = new JsonObject
        {
            ["secret"] = encrypted
        };

        // Serialize the updated JSON object back to a string
        string updatedJsonString = JsonSerializer.Serialize(updatedJsonObject);

        // Write the updated JSON string back to the file
        File.WriteAllText(filePath, updatedJsonString);

        // Write the new value to the console
        Console.WriteLine(encrypted);

        // Assert that the file was updated correctly
        string newJsonString = File.ReadAllText(filePath);
        var newJsonObject = JsonSerializer.Deserialize<JsonElement>(newJsonString);
        var newSecret = newJsonObject.GetProperty("secret").GetString();

        Assert.Equal(encrypted, newSecret);*/

         /*       var decrypted = LabKeyEncrypter.Decrypt(plainText, password);

        // Create a new JSON object with the updated value
        var updatedJsonObject2 = new JsonObject
        {
            ["secret"] = decrypted
        };

        // Serialize the updated JSON object back to a string
        string updatedJsonString2 = JsonSerializer.Serialize(updatedJsonObject2);

        // Write the updated JSON string back to the file
        File.WriteAllText(filePath, updatedJsonString2);

        // Write the new value to the console
        Console.WriteLine(decrypted);

        // Assert that the file was updated correctly
        string newJsonString2 = File.ReadAllText(filePath);
        var newJsonObject2 = JsonSerializer.Deserialize<JsonElement>(newJsonString2);
        var newSecret2 = newJsonObject2.GetProperty("secret").GetString();

        Assert.Equal(decrypted, newSecret);*/
    }
}