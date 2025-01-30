# LabKeyEncrypter

## How to use LabKeyEncrypter Console App

```
tools\LabKeyEncrypter\LabKeyEncrypter.ConsoleApp\LabKeyEncrypterConsoleApp.cs
```

Run using the encrypt parameter on file {filename}.json to generate or overwrite the file {filename}_encrypted.json with encrypted values

Run using the decrypt parameter on file {filename}_encrypted.json to generate or overwrite the file {filename}.json with decrypted values

## Required parameters

1. "encrypt" or "decrypt"

2. The path of a json file

3. A password

## Examples

```
dotnet run encrypt Unencrypted.json password
dotnet run decrypt Unencrypted_encrypted.json password
```