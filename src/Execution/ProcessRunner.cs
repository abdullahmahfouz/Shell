using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

/// <summary>Handles finding and executing external programs from the PATH environment variable</summary>
public class ProcessRunner
{
    /// <summary>Searches for a command in the PATH environment variable directories</summary>
    /// <param name="command">The command name to search for</param>
    /// <returns>The full path to the executable if found, otherwise null</returns>
    public static string? FindInPath(string command)
    {
        var pathEnv = Environment.GetEnvironmentVariable("PATH");
        if (pathEnv == null)
            return null;

        // Split PATH into individual directories (using ':' on Unix, ';' on Windows)
        var directories = pathEnv.Split(Path.PathSeparator);
        
        foreach (var dir in directories)
        {
            var fullPath = Path.Combine(dir, command);
            
            // Check if file exists and has execute permissions
            if (File.Exists(fullPath) && IsExecutable(fullPath))
                return fullPath;
        }

        return null;
    }

    //------------------------------------------------------------------------------------------------------

    /// <summary>Checks if a file has execute permissions on Unix-like systems</summary>
    /// <param name="path">The path to the file to check</param>
    /// <returns>True if the file has any execute permission bit set, false otherwise</returns>
    public static bool IsExecutable(string path)
    {
        try
        {
            var unixFileMode = File.GetUnixFileMode(path);
            
            // Check if any execute permission is set (user, group, or other)
            return (unixFileMode & UnixFileMode.UserExecute) != 0 ||
                   (unixFileMode & UnixFileMode.GroupExecute) != 0 ||
                   (unixFileMode & UnixFileMode.OtherExecute) != 0;
        }
        catch
        {
            return false;
        }
    }

    //------------------------------------------------------------------------------------------------------

    /// <summary>Runs an external program through /bin/sh with proper argv[0] handling</summary>
    /// <remarks>Uses exec -a to set argv[0] to the command name instead of the full path</remarks>
    /// <param name="path">The full path to the executable</param>
    /// <param name="commandName">The command name to use as argv[0]</param>
    /// <param name="args">Arguments to pass to the program</param>
    /// <param name="outputFile">Optional file to redirect stdout to</param>
    /// <param name="errorFile">Optional file to redirect stderr to</param>
    /// <param name="appendOutput">If true, append to output files instead of truncating</param>
    public static void RunExternalProgram(
        string path,
        string commandName,
        string[] args,
        string? outputFile = null,
        string? errorFile = null,
        bool appendOutput = false)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "/bin/sh",
            UseShellExecute = false
        };

        // Escape single quotes in command name, path, and args
        // Replace ' with '\'' to properly escape within single-quoted strings
        var escapedCommandName = commandName.Replace("'", "'\\''");
        var escapedPath = path.Replace("'", "'\\''");
        var escapedArgs = args.Select(a => $"'{a.Replace("'", "'\\''")}'");
        
        // Build command using exec -a to set argv[0]
        var commandLine = $"exec -a '{escapedCommandName}' '{escapedPath}' {string.Join(" ", escapedArgs)}";

        // Add stdout redirection if specified
        if (outputFile != null)
        {
            string redirectOp = appendOutput ? ">>" : ">";
            commandLine += $" {redirectOp} '{outputFile}'";
        }
        
        // Add stderr redirection if specified
        if (errorFile != null)
        {
            string redirectOp = appendOutput ? "2>>" : "2>";
            commandLine += $" {redirectOp} '{errorFile}'";
        }

        // Pass the command line to shell
        startInfo.ArgumentList.Add("-c");
        startInfo.ArgumentList.Add(commandLine);

        try
        {
            using (Process? process = Process.Start(startInfo))
            {
                process?.WaitForExit();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error running external program: {ex.Message}");
        }
    }

    //------------------------------------------------------------------------------------------------------

    /// <summary>Alternative method using .NET's built-in redirection</summary>
    /// <param name="fileName">The executable to run</param>
    /// <param name="args">Arguments to pass</param>
    /// <param name="outputFile">Optional file to write stdout to</param>
    /// <param name="errorFile">Optional file to write stderr to</param>
    /// <returns>The exit code of the process</returns>
    public static int RunExternal(
        string fileName,
        string[] args,
        string? outputFile,
        string? errorFile)
    {
        var psi = new ProcessStartInfo
        {
            FileName = fileName,
            RedirectStandardOutput = outputFile != null,
            RedirectStandardError = errorFile != null,
            RedirectStandardInput = false,
            UseShellExecute = false
        };
        
        foreach (var a in args)
            psi.ArgumentList.Add(a);

        using var proc = Process.Start(psi);
        if (proc == null)
            return 1;

        // Handle stdout redirection
        if (outputFile != null)
        {
            var stdout = proc.StandardOutput.ReadToEndAsync();
            File.WriteAllText(outputFile, stdout.Result);
        }

        // Handle stderr redirection
        if (errorFile != null)
        {
            var stderr = proc.StandardError.ReadToEndAsync();
            File.WriteAllText(errorFile, stderr.Result);
        }

        proc.WaitForExit();
        return proc.ExitCode;
    }
}
