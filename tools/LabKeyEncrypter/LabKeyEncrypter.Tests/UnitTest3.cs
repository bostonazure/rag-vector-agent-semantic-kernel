using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using Xunit;

namespace LabKeyEncrypter.Library;

public class UnitTest3
{
    [Fact]
    public void TestDecryptAndWriteToJson()
    {
        /*string filePath = "secret.json";
        string jsonString = File.ReadAllText(filePath);

        var jsonObject = JsonSerializer.Deserialize<JsonElement>(jsonString);
        var plainText = jsonObject.GetProperty("secret").GetString();
        var password = "password";

        var decrypted = LabKeyEncrypter.Decrypt(plainText, password);

        // Create a new JSON object with the updated value
        var updatedJsonObject = new JsonObject
        {
            ["secret"] = decrypted
        };

        // Serialize the updated JSON object back to a string
        string updatedJsonString = JsonSerializer.Serialize(updatedJsonObject);

        // Write the updated JSON string back to the file
        File.WriteAllText(filePath, updatedJsonString);

        // Write the new value to the console
        Console.WriteLine(decrypted);

        // Assert that the file was updated correctly
        string newJsonString = File.ReadAllText(filePath);
        var newJsonObject = JsonSerializer.Deserialize<JsonElement>(newJsonString);
        var newSecret = newJsonObject.GetProperty("secret").GetString();

        Assert.Equal(decrypted, newSecret);*/
    }
}