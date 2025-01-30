using System.ComponentModel.Design;
using System.Diagnostics;
using Microsoft.Extensions.FileProviders;

public class ConfigureLabKeys
{
   // bool Verbose => true;
   bool Verbose => false;
   string Password { init; get; }

   public ConfigureLabKeys(string password)
   {
      Password = password;
      Console.WriteLine(Directory.GetCurrentDirectory());
   }

   // go up until you find `labs` directory, verify `.../labs/keys` is there,
   // then get to work
   public void RandomizeDecryptDistribute()
   {
      string keysPath = FindKeysDirectoryPath(".");
#pragma warning disable CS8600
      string labsPath = Path.GetDirectoryName(keysPath);
#pragma warning restore CS8600
      string localSettingsFile = "appsettings.Local.json";
      string localSettingsPath = Path.Combine(keysPath, localSettingsFile);

      // if appsettings.Local.json exists in .../labs/keys, then WE ARE DONE
      if (File.Exists(localSettingsPath))
      {
         Console.WriteLine("╔═══════════════════════════════════╗");
         Console.WriteLine("    ⏭️ Skipping Lab Configuration 🦘");
         Console.WriteLine("       🤖🤖 ✔✔ ready to go ✔✔ 🚀🚀   ");
         Console.WriteLine("╚═══════════════════════════════════╝");

         return;
      }
      else
      {
         Console.WriteLine("╔══════════════════════════════════════════════╗");
         Console.WriteLine("  😄 One-time Lab Configuration ✅ COMPLETED 😊 ");
         Console.WriteLine("             🤖🤖  Let's go!   🚀🚀             ");
         Console.WriteLine("╚══════════════════════════════════════════════╝");
      }

      // randomize (choose one of the options randomly)
      var mySettingsFile = RandomlySelect(keysPath);
      if (Verbose)
      {
         Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
         Console.WriteLine($"Selected: {mySettingsFile}");
         Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
      }

      try
      {
         // decrypt in place in .../labs/keys directory
         LabKeyEncrypter.Library.LabKeyJsonFileValueEncrypter.DecryptJsonValues(mySettingsFile, Password);
         var myDecryptedSettingsFilePath = mySettingsFile.Replace("_encrypted.json", ".json");

         // copy path/keys/1_appsettings.Local.json to path/keys/appsettings.Local.json
         File.Copy(myDecryptedSettingsFilePath, localSettingsPath, overwrite: true);
      }
      catch (System.Security.Cryptography.CryptographicException ex)
      {
         Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
         Console.WriteLine($"Error decrypting {mySettingsFile}: {ex.Message}");
         Console.WriteLine($"Double-check that password \'{Password}\' is correct.");
         Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
         // exit from Console application
         Environment.Exit(1);
      }

      // distribute (distribute decrypyed copies to all the labs)

#pragma warning disable CS8604
      var allLabPaths = GetAllLabPaths(labsPath);
#pragma warning restore CS8604

      string settingsPath = Path.Combine(keysPath, localSettingsFile);
      foreach (var labPath in allLabPaths)
      {
         var labSettingsPath = Path.Combine(labPath, localSettingsFile);
         File.Copy(localSettingsPath, labSettingsPath, overwrite: true);
      }

      Console.WriteLine("╔═════════════════════════════╗");
      Console.WriteLine("║ Lab key decryption COMPLETE ║");
      Console.WriteLine("╚═════════════════════════════╝");
   }

   // TODO: don't hard-code ".../labs/keys" path
   public string FindKeysDirectoryPath(string startPath)
   {
      DirectoryInfo currentDir = new DirectoryInfo(startPath);

      while (currentDir != null)
      {
         if (string.Equals(currentDir.Name, "labs", StringComparison.OrdinalIgnoreCase))
         {
            string labsPath = currentDir.FullName;
            string keysPath = Path.Combine(labsPath, "keys");

            if (Path.Exists(keysPath))
               return keysPath;

            throw new InvalidOperationException($"Found {labsPath}, but not {keysPath}");
         }

#pragma warning disable CS8600
         currentDir = currentDir.Parent;
#pragma warning restore CS8600
         if (Verbose) Console.WriteLine($"just moved currentDir ==> {currentDir}");
      }

      throw new InvalidOperationException("Unable to locate directory 'labs' or 'labs/keys'.");
   }

   public string RandomlySelect(string path)
   {
      // at path there are files named like:
      // 1_appsettings.Local.json, 2_appsettings.Local.json, ..., 17_appsettings.Local.json, etc.
      // randomly pick one

      string searchPattern = "*_appsettings.Local_encrypted.json";
      string[] files = Directory.GetFiles(path, searchPattern);

      var random = new Random();
      var index = random.Next(files.Length);
      return files[index];
   }

   public string[] GetAllLabPaths(string labsPath)
   {
      // under labsPath there are directories named like:
      // lab0/src/start, lab0/src/end, lab1/src/start, lab1/src/end, ..., lab17/src/end, etc.
      // some may be named differently like lab3-XYZ/src/start, lab3-XYZ/src/end, but starts with "labX"
      // return only the labX/src/start and labX/src/end subdirectories

      var labNPaths = Directory.GetDirectories(labsPath, "lab*");
      var startPaths = new List<string>(labNPaths.Length);
      var endPaths = new List<string>(labNPaths.Length);

      foreach (var labNPath in labNPaths)
      {
         if (Verbose) Console.WriteLine($"adding ━━ {labNPath} ━━");

         var labNPathSrcStart = Path.Combine(labNPath, "src/start");
         if (Directory.Exists(labNPathSrcStart))
            startPaths.Add(labNPathSrcStart);

         var labNPathSrcEnd = Path.Combine(labNPath, "src/end");
         if (Directory.Exists(labNPathSrcEnd))
            endPaths.Add(labNPathSrcEnd);
      }

      var allPaths = startPaths.Concat(endPaths).ToArray();

      foreach (var path in allPaths)
      {
         if (Verbose) Console.WriteLine($"╋ {path}");
      }

      return allPaths;
   }
}
