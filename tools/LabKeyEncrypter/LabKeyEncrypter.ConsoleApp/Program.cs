using LabKeyEncrypter.Library;

if (args.Length != 3)
{
    Console.WriteLine("Usage: LabKeyEncrypter.ConsoleApp <encrypt|decrypt> <file> <password>");
    return;
}
var operation = args[0];
var filePath = args[1];
var password = args[2];


if (operation == "encrypt")
{
    try
    {
        LabKeyJsonFileValueEncrypter.EncryptJsonValues(filePath, password);
    }
    catch (System.IO.FileNotFoundException ex)
    {
        Console.WriteLine("Error: " + ex.Message);
    }
}
else if (operation == "decrypt")
{
    try
    {
        LabKeyJsonFileValueEncrypter.DecryptJsonValues(filePath, password);
    }
    catch (System.Security.Cryptography.CryptographicException ex)
    {
        Console.WriteLine("Error decrypting value: " + ex.Message);
    }
    catch (System.IO.FileNotFoundException ex)
    {
        Console.WriteLine("Error: " + ex.Message);
    }
}
else
{
    Console.WriteLine("Invalid operation: " + operation);
}
