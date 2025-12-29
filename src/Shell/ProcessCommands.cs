using System;
using System.IO;

/// <summary>Routes commands to builtin handlers or external program execution</summary>

public class ProcessCommands
{
    /// <summary>Processes a command line input</summary>
    /// <param name="input">The raw command line input</param>
    public static void ProcessCommand(string input)
    {
        // First check if it's a pipeline
        var pipeline = CommandLineParser.ParsePipeline(input);
        if (pipeline == null)
            return;

        // If it's a multi-command pipeline, use the pipeline runner
        if (pipeline.IsPipeline)
        {
            PipelineRunner.Execute(pipeline);
            return;
        }

        // Single command - use regular processing
        var command = pipeline.Commands[0];

        // Route command to appropriate handler
        switch (command.Name)
        {
            case "exit":
                Environment.Exit(0);
                break;

            case "echo":
                var echoContent = string.Join(" ", command.Args);
                RunWithRedirections(command, () => Builtins.HandleEcho(echoContent));
                break;

            case "type":
                var typeInput = command.Args.Length > 0 
                    ? $"type {string.Join(" ", command.Args)}" 
                    : "type";
                RunWithRedirections(command, () => Builtins.HandleType(typeInput));
                break;

            case "pwd":
                RunWithRedirections(command, () => Builtins.HandlePwd(command.OutputFile));
                break;

            case "cd":
                Navigation.ChangeDir(command.Args);
                break;

            case "history":
                if (command.Args.Length >= 2 && command.Args[0] == "-r")
                {
                    // history -r <path> - read from file
                    History.ReadFromFile(command.Args[1]);
                }
                else if (command.Args.Length >= 2 && command.Args[0] == "-w")
                {
                    // history -w <path> - write to file (silent, no output)
                    History.WriteToFile(command.Args[1]);
                }
                else if (command.Args.Length >= 2 && command.Args[0] == "-a")
                {
                    // history -a <path> - append to file (silent, no output)
                    History.WriteToFile(command.Args[1]);
                }
                else if (command.Args.Length > 0 && int.TryParse(command.Args[0], out int limit))
                {
                    // history <n> - show last n entries
                    History.Print(limit);
                }
                else
                {
                    // history - show all entries
                    History.Print();
                }
                break;

            case "cat":
                RunWithRedirections(command, () => Builtins.HandleCat(command.Args));
                break;

            default:
                HandleExternalCommand(command);
                break;
        }
    }

    //------------------------------------------------------------------------------------------------------

    /// <summary>Executes external programs from PATH</summary>
    /// <param name="command">The command to execute</param>
    private static void HandleExternalCommand(Command command)
    {
        var programPath = ProcessRunner.FindInPath(command.Name);
        if (programPath != null)
        {
            ProcessRunner.RunExternalProgram(
                programPath,
                command.Name,
                command.Args,
                command.OutputFile,
                command.ErrorFile,
                command.AppendOutput);
        }
        else
        {
            Console.WriteLine($"{command.Name}: command not found");
        }
    }

    //------------------------------------------------------------------------------------------------------

    /// <summary>Executes a builtin command with optional I/O redirection</summary>
    /// <param name="command">The command with redirection info</param>
    /// <param name="action">The builtin action to execute</param>
    private static void RunWithRedirections(Command command, Action action)
    {
        var originalOut = Console.Out;
        var originalErr = Console.Error;
        StreamWriter? outWriter = null;
        StreamWriter? errWriter = null;

        try
        {
            // Redirect stdout if specified
            if (command.OutputFile != null)
            {
                var fileMode = command.AppendOutput ? FileMode.Append : FileMode.Create;
                var stream = new FileStream(command.OutputFile, fileMode, FileAccess.Write);
                outWriter = new StreamWriter(stream) { AutoFlush = true };
                Console.SetOut(outWriter);
            }

            // Redirect stderr if specified
            if (command.ErrorFile != null)
            {
                var fileMode = command.AppendOutput ? FileMode.Append : FileMode.Create;
                var stream = new FileStream(command.ErrorFile, fileMode, FileAccess.Write);
                errWriter = new StreamWriter(stream) { AutoFlush = true };
                Console.SetError(errWriter);
            }

            // Execute the action with redirected output
            action();
        }
        finally
        {
            // Restore original streams
            errWriter?.Dispose();
            outWriter?.Dispose();
            Console.SetOut(originalOut);
            Console.SetError(originalErr);
        }
    }
}
