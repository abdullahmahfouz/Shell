using System;
using System.Collections.Generic;
using System.IO;

public static class Builtins
{
    private static readonly HashSet<string> BuiltinCommands = new HashSet<string> { "exit", "echo", "type", "pwd", "cd" };

    public static bool IsBuiltin(string command) => BuiltinCommands.Contains(command);

    public static void HandleType(string input)
    {
        var split = input.Split(' ', 2);
        if (split.Length < 2)
        {
            Console.WriteLine("type: missing operand");
            return;
        }

        var command = split[1];

        if (BuiltinCommands.Contains(command))
        {
            Console.WriteLine($"{command} is a shell builtin");
            return;
        }

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

    public static void HandleEcho(string content)
    {
        // Content is already unquoted/escaped by the command-line parser, so just echo it.
        Console.WriteLine(content);
    }

    public static void HandleCat(string[] args)
    {
        if (args.Length == 0)
        {
            string? line;
            while ((line = Console.ReadLine()) != null)
            {
                Console.WriteLine(line);
            }
            return;
        }

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
