using System;
using System.IO;

/// <summary>Handles directory navigation for cd and pwd commands</summary>
public class Navigation
{
    /// <summary>Changes the current working directory</summary>
    /// <param name="args">Directory path to change to, or empty for home directory</param>
    public static void ChangeDir(string[] args)
    {
        var homeDir = Environment.GetEnvironmentVariable("HOME");

        // No arguments: go to home directory
        if (args.Length == 0)
        {
            if (homeDir != null)
            {
                Directory.SetCurrentDirectory(homeDir);
            }
            else
            {
                Console.WriteLine("cd: HOME not set");
            }
            return;
        }

        var path = args[0];

        // Handle ~ expansion
        if (path == "~")
        {
            if (homeDir != null)
            {
                path = homeDir;
            }
            else
            {
                Console.WriteLine("cd: HOME not set");
                return;
            }
        }

        // Change directory with error handling
        try
        {
            Directory.SetCurrentDirectory(path);
        }
        catch (DirectoryNotFoundException)
        {
            Console.WriteLine($"cd: {path}: No such file or directory");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"cd: {path}: {ex.Message}");
        }
    }

    //------------------------------------------------------------------------------------------------------

    /// <summary>Returns the current working directory</summary>
    /// <returns>The absolute path of the current directory</returns>
    public static string GetCurrentDirectory() => Directory.GetCurrentDirectory();
}
