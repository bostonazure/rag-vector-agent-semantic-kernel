namespace Configuration;
public static class PathUtils
{
    public static string FindAncestorDirectory(string directoryName)
    {
        var currentDirectory = Directory.GetCurrentDirectory();
        var directory = new DirectoryInfo(currentDirectory);

        while (directory != null)
        {
            if (directory.GetDirectories(directoryName).Any())
            {
                return Path.Combine(directory.FullName, directoryName);
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException($"Could not find directory '{directoryName}' in any ancestor directory.");
    }
}