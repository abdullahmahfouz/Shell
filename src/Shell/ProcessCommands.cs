using System;
using System.IO;

public class ProcessCommands
{
    public static void ProcessCommand(string input)
    {
        var command = CommandLineParser.Parse(input);
        if (command == null) return;

        switch (command.Name)
        {
            case "exit":
                Environment.Exit(0);
                break;

            case "echo":
            {
                var echoContent = string.Join(" ", command.Args);
                // Builtin echo supports optional stdout/stderr redirection
                RunWithRedirections(command, () => Builtins.HandleEcho(echoContent));
                break;
            }

            case "type":
            {
                var typeInput = command.Args.Length > 0 ? $"type {string.Join(" ", command.Args)}" : "type";
                // Builtin type prints info about builtins or PATH lookups
                RunWithRedirections(command, () => Builtins.HandleType(typeInput));
                break;
            }

            case "pwd":
                // Builtin pwd prints current directory (optionally redirected)
                RunWithRedirections(command, () => Builtins.HandlePwd(command.OutputFile));
                break;

            case "cd":
                // cd affects current process; no redirection support needed
                Navigation.ChangeDir(command.Args);
                break;

            case "cat":
                // cat reads files/stdin; allow redirection for stdout/stderr
                RunWithRedirections(command, () => Builtins.HandleCat(command.Args));
                break;

            default:
            {
                var programPath = ProcessRunner.FindInPath(command.Name);
                if (programPath != null)
                {
                    // External: run via PATH resolution with exec -a so argv[0] = command name
                    ProcessRunner.RunExternalProgram(programPath, command.Name, command.Args, command.OutputFile, command.ErrorFile, command.AppendOutput);
                }
                else
                {
                    Console.WriteLine($"{command.Name}: command not found");
                }
                break;
            }
        }
    }

    private static void RunWithRedirections(Command command, Action action)
    {
        var originalOut = Console.Out;
        var originalErr = Console.Error;
        StreamWriter? outWriter = null;
        StreamWriter? errWriter = null;

        try
        {
            if (command.OutputFile != null)
            {
                var fileMode = command.AppendOutput ? FileMode.Append : FileMode.Create;
                var stream = new FileStream(command.OutputFile, fileMode, FileAccess.Write);
                outWriter = new StreamWriter(stream) { AutoFlush = true };
                Console.SetOut(outWriter);
            }

            if (command.ErrorFile != null)
            {
                var fileMode = command.AppendOutput ? FileMode.Append : FileMode.Create;
                var stream = new FileStream(command.ErrorFile, fileMode, FileAccess.Write);
                errWriter = new StreamWriter(stream) { AutoFlush = true };
                Console.SetError(errWriter);
            }

            action();
        }
        finally
        {
            // Flush/close redirected streams then restore console streams
            errWriter?.Dispose();
            outWriter?.Dispose();
            Console.SetOut(originalOut);
            Console.SetError(originalErr);
        }
    }
}
