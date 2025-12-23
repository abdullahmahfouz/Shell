// Import necessary namespaces for shell functionality
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks.Dataflow;

public class HandleCommands
{
    // Define the set of built-in shell commands
    private static HashSet<string> builtinCommands = new HashSet<string> { "exit", "echo", "type", "pwd", "cd", "cat"};
    
    // Handle the type command - identifies whether a command is builtin or external
    public static void HandleType(string input)
    {
        // Extract the command name from the input
        var split = input.Split(' ', 2);
        var command = split[1];
        
        // Check if it's a built-in command
        if (builtinCommands.Contains(command))
        {
            Console.WriteLine($"{command} is a shell builtin");
        }
        else
        {
            // Search for the command in PATH directories
            var foundPath = ProcessRunner.FindInPath(command);
            if (foundPath != null)
            {
                // Command found in PATH - display its location
                Console.WriteLine($"{command} is {foundPath}");
            }
            else
            {
                // Command not found anywhere
                Console.WriteLine($"{command}: not found");
            }
        }
    }

    // Handle the echo command - prints text with proper quote handling
    public static void HandleEcho(string content)
    {
        // Parse and process quotes in the content
        var output = Quoting.ParseQuotedString(content);
        Console.WriteLine(output);
    }

    // Handle the cat command - concatenate and display file contents
    public static void HandleCat(string[] args)
    {
        // If no files are provided, read from standard input
        if (args.Length == 0)
        {
            string? line;
            while ((line = Console.ReadLine()) != null)
            {
                Console.WriteLine(line);
            }
        }
        else
        {
            // Read and print each file specified in arguments
            foreach (var filePath in args)
            {
                try
                {
                    // Read the entire file content as-is
                    var content = File.ReadAllText(filePath);
                    // Write content without adding extra newlines
                    Console.Write(content);
                }
                catch (FileNotFoundException)
                {
                    // File does not exist
                    Console.WriteLine($"cat: {filePath}: No such file or directory");
                }
                catch (Exception ex)
                {
                    // Other errors (permission denied, etc.)
                    Console.WriteLine($"cat: {filePath}: {ex.Message}");
                }
            }
        }
    }
}