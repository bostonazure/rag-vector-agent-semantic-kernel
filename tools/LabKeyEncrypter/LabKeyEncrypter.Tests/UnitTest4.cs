using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using Xunit;

namespace LabKeyEncrypter.Library;

public class UnitTest4
{
    [Fact]
    public void EncryptDecrypt_LongPassword()
    {
        var plainText = "This is a secret";
        var password = "thisisanintentiallyverylongpasswordthatisonlymentforunittesting1234567891011121314151617181920";

        var encrypted = LabKeyEncrypter.Encrypt(plainText, password);
        var decrypted = LabKeyEncrypter.Decrypt(encrypted, password);

        Assert.Equal(plainText, decrypted);
    }
}
