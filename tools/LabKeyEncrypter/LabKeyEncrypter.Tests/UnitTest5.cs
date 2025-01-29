using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using Xunit;

namespace LabKeyEncrypter.Library;

public class UnitTest5
{
    [Fact]
    public void Decrypt_Corrupted()
    {
        var plainText = "This is a secret";
        var password = "password";

        var encrypted = "HFtK6d+wgtbXMuaIVNTE1kpvb/M4+sOBbRnlq8RomRrWwVECOi4sTamwL19nXXpENvu8UTKO2owy2jf6916lJA==";

        var decrypted = LabKeyEncrypter.Decrypt(encrypted, password);

        Console.WriteLine(decrypted);

        Assert.Equal(plainText, decrypted);
    }

    [Fact]
    public void Decrypt_CorruptedPayload_ThrowsException()
    {
        var password = "password";

        var encrypted = "aFtK6d+wgtbXMuaIVNTE1kpvb/M4+sOBbRnlq8RomRrWwVECOi4sTamwL19nXXpENvu8UTKO2owy2jf6916lJA==";

        Assert.Throws<System.Security.Cryptography.CryptographicException>(() =>
        {
            var decrypted = LabKeyEncrypter.Decrypt(encrypted, password);
        });
    }

    [Fact]
    public void Decrypt_CorruptedBase64_ThrowsException()
    {
        var password = "password";

        var encrypted = "HFtK6d+wgtbXMuaIVNTE1kpvb/M4+sOBbRnlq8RomRrWwVECOi4sTamwL19nXXpENvu8UTKO2owy2jf6916lJA00==";

        Assert.Throws<System.FormatException>(() =>
        {
            var decrypted = LabKeyEncrypter.Decrypt(encrypted, password);
        });
    }
}
