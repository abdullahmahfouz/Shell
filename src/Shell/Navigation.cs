using System;
using System.IO;

public class Navigation
{
    public static void ChangeDir(string[] args)
    {
        var homeDir = Environment.GetEnvironmentVariable("HOME");

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

    public static string GetCurrentDirectory() => Directory.GetCurrentDirectory();
}
