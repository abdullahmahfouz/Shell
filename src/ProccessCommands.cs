using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection.Metadata;

public class ProcessCommands
{
    // Define the set of built-in shell commands that don't need to be searched in PATH
    private static HashSet<string> builtinCommands = new HashSet<string> { "exit", "echo", "type", "pwd", "cd" };

    // Parse and execute the input command
    public static void ProcessCommand(string input)
    {
        // Parse the command and arguments properly handling quotes
        var parts = Quoting.ParseCommandLine(input);

        // Return early if no command was entered
        if (parts.Length == 0) return;

        var command = parts[0]; // First element is the command
        var args = parts.Skip(1).ToArray(); // Get everything after the command as arguments

        string? outputFile = null;
        var finalargs = new List<string>();

        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == ">")
            {
                if (i + 1 < args.Length)
                {
                    outputFile = args[i + 1];
                    i++; // Skip the filename in the next iteration
                }
            }
            else
            {
                finalargs.Add(args[i]);
            }
        }
        args = finalargs.ToArray();

        // Handle exit command - terminates the shell
        if (command == "exit")
        {
            // You might want to parse args[0] for the exit code later
            Environment.Exit(0);
        }
        // Handle echo command - prints arguments to console
        else if (command == "echo")
        {
            // Rebuild echo content without the redirection tokens
            var echoContent = string.Join(" ", args);
            if (outputFile != null)
            {
                HandleCommands.RunWithOutputRedirection(input, outputFile, () => HandleCommands.HandleEcho(echoContent));
            }
            else
            {
                HandleCommands.HandleEcho(echoContent);
            }
        }
        // Handle type command - displays information about a command
        else if (command == "type")
        {
            // Build a clean input without redirection to avoid confusing the handler
            var typeInput = args.Length > 0 ? $"type {string.Join(" ", args)}" : "type";
            if (outputFile != null)
            {
                HandleCommands.RunWithOutputRedirection(input, outputFile, () => HandleCommands.HandleType(typeInput));
            }
            else
            {
                HandleCommands.HandleType(typeInput);
            }
        }
        else if (command == "pwd")
        {
            // Print the current working directory
            HandleCommands.HandlePwd(outputFile);
        }
        // Handle cd command - changes the current working directory
        else if (command == "cd")
        {
            Navigation.ChangeDir(args);
        }
        // Handle cat command - concatenate and display file contents
        else if (command == "cat")
        {
            if (outputFile != null)
            {
                HandleCommands.RunWithOutputRedirection(input, outputFile, () => HandleCommands.HandleCat(args));
            }
            else
            {
                HandleCommands.HandleCat(args);
            }
        }
        else
        {
            // EXTERNAL PROGRAM LOGIC
            // Try to find the command in PATH directories
            string? programPath = ProcessRunner.FindInPath(command);

            if (programPath != null)
            {
                // Execute the external program with provided arguments
                // Pass both the full path and the original command name
                ProcessRunner.RunExternalProgram(programPath, command, args, outputFile);
            }
            else
            {
                // Command not found in PATH or as builtin
                Console.WriteLine($"{command}: command not found");
            }
        }
    }
}