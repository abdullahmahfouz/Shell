using System;
using System.Collections.Generic;

public class ProcessCommands
{
    public static void ProcessCommand(string input)
    {
        var command = CommandLineParser.Parse(input);
        if (command == null)
        {
            return;
        }

        switch (command.Name)
        {
            case "exit":
                Environment.Exit(0);
                break;

            case "echo":
            {
                var echoContent = string.Join(" ", command.Args);
                RunWithOptionalRedirection(command.OutputFile, () => Builtins.HandleEcho(echoContent));
                break;
            }

            case "type":
            {
                var typeInput = command.Args.Length > 0 ? $"type {string.Join(" ", command.Args)}" : "type";
                RunWithOptionalRedirection(command.OutputFile, () => Builtins.HandleType(typeInput));
                break;
            }

            case "pwd":
                Builtins.HandlePwd(command.OutputFile);
                break;

            case "cd":
                Navigation.ChangeDir(command.Args);
                break;

            case "cat":
                RunWithOptionalRedirection(command.OutputFile, () => Builtins.HandleCat(command.Args));
                break;

            default:
                RunExternal(command);
                break;
        }
    }

    private static void RunWithOptionalRedirection(string? outputFile, Action action)
    {
        if (outputFile != null)
        {
            OutputRedirection.RunWithOutputRedirection(outputFile, action);
        }
        else
        {
            action();
        }
    }

    private static void RunExternal(Command command)
    {
        var programPath = ProcessRunner.FindInPath(command.Name);
        if (programPath != null)
        {
            ProcessRunner.RunExternalProgram(programPath, command.Name, command.Args, command.OutputFile);
        }
        else
        {
            Console.WriteLine($"{command.Name}: command not found");
        }
    }
}
