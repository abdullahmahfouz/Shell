using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

public class ProcessRunner
{
    public static string? FindInPath(string command)
    {
        var pathEnv = Environment.GetEnvironmentVariable("PATH");
        if (pathEnv == null) return null;

        var directories = pathEnv.Split(Path.PathSeparator);
        foreach (var dir in directories)
        {
            var fullPath = Path.Combine(dir, command);
            if (File.Exists(fullPath) && IsExecutable(fullPath))
            {
                return fullPath;
            }
        }
        return null;
    }

    private static bool IsExecutable(string path)
    {
        try
        {
            var unixFileMode = File.GetUnixFileMode(path);
            return (unixFileMode & UnixFileMode.UserExecute) != 0 ||
                   (unixFileMode & UnixFileMode.GroupExecute) != 0 ||
                   (unixFileMode & UnixFileMode.OtherExecute) != 0;
        }
        catch
        {
            return false;
        }
    }

    public static void RunExternalProgram(string path, string commandName, string[] args, string? outputFile = null)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "/bin/sh",
            UseShellExecute = false
        };

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
}
