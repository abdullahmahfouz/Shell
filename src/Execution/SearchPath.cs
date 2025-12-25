/// <summary>
/// Handles searching for executables in the system PATH environment variable.
/// Used for auto-completion to find external commands that match a given prefix.
/// </summary>
public class SearchPath
{
    /// <summary>
    /// Searches all directories in PATH for executables starting with the given prefix.
    /// </summary>
    /// <param name="prefix">The prefix to match against executable names</param>
    /// <returns>List of executable names that start with the prefix</returns>
    /// <remarks>
    /// - Uses Path.PathSeparator for cross-platform compatibility (: on Mac/Linux, ; on Windows)
    /// - Silently ignores directories that cannot be accessed (permissions, doesn't exist, etc.)
    /// - Only includes files that pass ProcessRunner.IsExecutable() check
    /// </remarks>
    public static List<string> Search(string prefix)
    {
        // List to store matching executable names
        var results = new List<string>();
        
        // Get the PATH environment variable (contains directories separated by : or ;)
        // Example (Mac): "/usr/local/bin:/usr/bin:/bin"
        var pathEnv = Environment.GetEnvironmentVariable("PATH");

        // Return empty list if PATH is not set
        if (string.IsNullOrEmpty(pathEnv))
            return results;
        
        // Split PATH into individual directories
        // Path.PathSeparator = ':' on Mac/Linux, ';' on Windows
        var directories = pathEnv.Split(Path.PathSeparator);
        
        // Search each directory in PATH
        foreach (var dir in directories)
        {
            try
            {
                // Get all files in the current directory
                var files = Directory.GetFiles(dir);
                
                // Check each file for a match
                foreach (var file in files)
                {
                    // Extract just the filename (without the directory path)
                    // Example: "/usr/bin/echo" → "echo"
                    var fileName = Path.GetFileName(file);
                    
                    // Check if filename starts with prefix AND is executable
                    if (fileName.StartsWith(prefix) && ProcessRunner.IsExecutable(file))
                    {
                        results.Add(fileName);
                    }
                }
            }
            catch (Exception)
            {
                // Silently ignore directories that:
                // - Don't exist
                // - Can't be accessed (permission denied)
                // - Have other IO errors
            }
        }
        
        return results;
    }
}