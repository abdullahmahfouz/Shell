// Import necessary namespaces for shell functionality
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;

public class ProcessRunner
{
    // Search for a command in the PATH environment variable
    public static string? FindInPath(string command)
    {
        // Get the PATH environment variable
        var pathEnv = Environment.GetEnvironmentVariable("PATH");

        // Return null if PATH is not set
        if (pathEnv == null) return null;

        // Split PATH into individual directories
        var directories = pathEnv.Split(Path.PathSeparator);

        // Search each directory for the command
        foreach (var dir in directories)
        {
            // Construct the full path to the potential executable
            var fullPath = Path.Combine(dir, command);

            // Check if file exists and is executable
            if (File.Exists(fullPath) && IsExecutable(fullPath))
            {
                return fullPath;
            }
        }

        // Command not found in any PATH directory
        return null;
    }

    // Check if a file has execute permissions (Unix-style)
    private static bool IsExecutable(string path)
    {
        try
        {
            // Get Unix file permissions (only available on Unix-like systems)
            var unixFileMode = File.GetUnixFileMode(path);
            // Check if any execute permission is set (user, group, or other)
            return (unixFileMode & UnixFileMode.UserExecute) != 0 ||
                   (unixFileMode & UnixFileMode.GroupExecute) != 0 ||
                   (unixFileMode & UnixFileMode.OtherExecute) != 0;
        }
        catch
        {
            // If unable to check permissions, assume not executable
            return false;
        }
    }

    // Run an external program with specified arguments
    public static void RunExternalProgram(string path, string commandName, string[] args, string? outputFile = null)
    {
        // To properly set argv[0] to just the command name (not full path),
        // we need to use exec -a through a shell
        ProcessStartInfo startInfo = new ProcessStartInfo();
        startInfo.FileName = "/bin/sh";

        // Build the command: exec -a <commandName> <fullPath> <args...>
        // exec -a allows us to set argv[0] explicitly
        // Escape single quotes in commandName and path
        var escapedCommandName = commandName.Replace("'", "'\\''");
        var escapedPath = path.Replace("'", "'\\''");
        var escapedArgs = args.Select(a => $"'{a.Replace("'", "'\\''")}'");
        var commandLine = $"exec -a '{escapedCommandName}' '{escapedPath}' {string.Join(" ", escapedArgs)}";

        if (outputFile != null)
        {
            commandLine += $" > '{outputFile}'";
        }

        startInfo.ArgumentList.Add("-c");
        startInfo.ArgumentList.Add(commandLine);
        startInfo.UseShellExecute = false;

        try
        {
            // Start the process and wait for it to complete
            using (Process? process = Process.Start(startInfo))
            {
                // Wait for the external program to finish execution
                process?.WaitForExit();
            }
        }
        catch (Exception ex)
        {
            // Display error message if program execution fails
            Console.WriteLine($"Error running external program: {ex.Message}");
        }
    }
}