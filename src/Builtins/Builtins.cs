using System;
using System.Collections.Generic;
using System.IO;

/// <summary>Implements shell built-in commands</summary>
public static class Builtins
{
    public static readonly HashSet<string> BuiltinCommands = new HashSet<string>
    {
        "exit", "echo", "type", "pwd", "cd"
    };

    /// <summary>Checks if a command is a builtin</summary>
    /// <param name="command">The command name to check</param>
    /// <returns>True if the command is a builtin, false otherwise</returns>
    public static bool IsBuiltin(string command) => BuiltinCommands.Contains(command);

    //------------------------------------------------------------------------------------------------------

    /// <summary>Identifies if a command is builtin or external</summary>
    /// <param name="input">The type command input</param>
    public static void HandleType(string input)
    {
        var split = input.Split(' ', 2);
        if (split.Length < 2)
        {
            Console.WriteLine("type: missing operand");
            return;
        }

        var command = split[1];

        // Check if it's a builtin command
        if (BuiltinCommands.Contains(command))
        {
            Console.WriteLine($"{command} is a shell builtin");
            return;
        }

        // Search PATH for external command
        var foundPath = ProcessRunner.FindInPath(command);
        if (foundPath != null)
        {
            Console.WriteLine($"{command} is {foundPath}");
        }
        else
        {
            Console.WriteLine($"{command}: not found");
        }
    }

    //------------------------------------------------------------------------------------------------------

    /// <summary>Prints arguments to stdout</summary>
    /// <param name="content">The content to echo</param>
  
    public static void HandleEcho(string content)
    {
        Console.WriteLine(content);
    }

    //------------------------------------------------------------------------------------------------------

    /// <summary>Concatenates and displays file contents</summary>
    /// <param name="args">File paths to concatenate, or empty to read stdin</param>
    public static void HandleCat(string[] args)
    {
        // Read from stdin if no files provided
        if (args.Length == 0)
        {
            string? line;
            while ((line = Console.ReadLine()) != null)
            {
                Console.WriteLine(line);
            }
            return;
        }

        // Read and print each file
        foreach (var filePath in args)
        {
            try
            {
                var content = File.ReadAllText(filePath);
                Console.Write(content);
            }
            catch (FileNotFoundException)
            {
                Console.Error.WriteLine($"cat: {filePath}: No such file or directory");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"cat: {filePath}: {ex.Message}");
            }
        }
    }

    //------------------------------------------------------------------------------------------------------

    /// <summary>Prints the current working directory</summary>
    /// <param name="outputFile">Optional file to write output to</param>
    public static void HandlePwd(string? outputFile)
    {
        var currentDir = Navigation.GetCurrentDirectory();

        if (outputFile == null)
        {
            Console.WriteLine(currentDir);
            return;
        }

        try
        {
            File.WriteAllText(outputFile, currentDir + Environment.NewLine);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"pwd: {outputFile}: {ex.Message}");
        }
    }
}
