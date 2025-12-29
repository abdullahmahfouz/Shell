using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

/// <summary>Executes pipelines of commands with proper I/O redirection between processes</summary>
public static class PipelineRunner
{
    /// <summary>Executes a pipeline of commands (external and/or built-in)</summary>
    /// <param name="pipeline">The pipeline containing commands to execute</param>
    public static void Execute(Pipeline pipeline)
    {
        if (pipeline.Commands.Count < 2)
        {
            // Single command, use regular execution
            return;
        }

        var commands = pipeline.Commands;
        
        // For a two-command pipeline, handle the special cases with builtins
        if (commands.Count == 2)
        {
            ExecuteTwoCommandPipeline(commands[0], commands[1]);
            return;
        }

        // For longer pipelines (future extension), fall back to external-only handling
        ExecuteExternalPipeline(pipeline);
    }

    /// <summary>Executes a two-command pipeline, handling builtins</summary>
    private static void ExecuteTwoCommandPipeline(Command first, Command second)
    {
        var firstIsBuiltin = Builtins.IsBuiltin(first.Name);
        var secondIsBuiltin = Builtins.IsBuiltin(second.Name);

        if (firstIsBuiltin && secondIsBuiltin)
        {
            // Both are builtins: capture first's output, feed to second
            ExecuteBuiltinToBuiltin(first, second);
        }
        else if (firstIsBuiltin)
        {
            // First is builtin, second is external
            ExecuteBuiltinToExternal(first, second);
        }
        else if (secondIsBuiltin)
        {
            // First is external, second is builtin
            ExecuteExternalToBuiltin(first, second);
        }
        else
        {
            // Both external
            ExecuteExternalToExternal(first, second);
        }
    }

    /// <summary>Executes a builtin piped to another builtin</summary>
    private static void ExecuteBuiltinToBuiltin(Command first, Command second)
    {
        // Capture first builtin's output
        var output = CaptureBuiltinOutput(first);
        
        // Execute second builtin with input from first (most builtins ignore stdin)
        using var inputReader = new StringReader(output);
        var originalIn = Console.In;
        Console.SetIn(inputReader);
        
        try
        {
            ExecuteBuiltinCommand(second);
        }
        finally
        {
            Console.SetIn(originalIn);
        }
    }

    /// <summary>Executes a builtin piped to an external command</summary>
    private static void ExecuteBuiltinToExternal(Command first, Command second)
    {
        // Capture builtin's output
        var output = CaptureBuiltinOutput(first);
        
        // Find the external program
        var programPath = ProcessRunner.FindInPath(second.Name);
        if (programPath == null)
        {
            Console.WriteLine($"{second.Name}: command not found");
            return;
        }

        // Start external process with redirected stdin
        var startInfo = new ProcessStartInfo
        {
            FileName = programPath,
            UseShellExecute = false,
            RedirectStandardInput = true,
            RedirectStandardOutput = false,
            RedirectStandardError = false
        };

        foreach (var arg in second.Args)
        {
            startInfo.ArgumentList.Add(arg);
        }

        using var process = Process.Start(startInfo);
        if (process == null) return;

        // Write builtin output to external process stdin
        process.StandardInput.Write(output);
        process.StandardInput.Close();
        
        process.WaitForExit();
    }

    /// <summary>Executes an external command piped to a builtin</summary>
    private static void ExecuteExternalToBuiltin(Command first, Command second)
    {
        // Find the external program
        var programPath = ProcessRunner.FindInPath(first.Name);
        if (programPath == null)
        {
            Console.WriteLine($"{first.Name}: command not found");
            return;
        }

        // Start external process with redirected stdout
        var startInfo = new ProcessStartInfo
        {
            FileName = programPath,
            UseShellExecute = false,
            RedirectStandardInput = false,
            RedirectStandardOutput = true,
            RedirectStandardError = false
        };

        foreach (var arg in first.Args)
        {
            startInfo.ArgumentList.Add(arg);
        }

        using var process = Process.Start(startInfo);
        if (process == null) return;

        // Capture external output
        var output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();

        // Execute builtin with external output as stdin
        using var inputReader = new StringReader(output);
        var originalIn = Console.In;
        Console.SetIn(inputReader);
        
        try
        {
            ExecuteBuiltinCommand(second);
        }
        finally
        {
            Console.SetIn(originalIn);
        }
    }

    /// <summary>Executes two external commands in a pipeline</summary>
    private static void ExecuteExternalToExternal(Command first, Command second)
    {
        var firstPath = ProcessRunner.FindInPath(first.Name);
        var secondPath = ProcessRunner.FindInPath(second.Name);

        if (firstPath == null)
        {
            Console.WriteLine($"{first.Name}: command not found");
            return;
        }
        if (secondPath == null)
        {
            Console.WriteLine($"{second.Name}: command not found");
            return;
        }

        // Start first process
        var startInfo1 = new ProcessStartInfo
        {
            FileName = firstPath,
            UseShellExecute = false,
            RedirectStandardInput = false,
            RedirectStandardOutput = true,
            RedirectStandardError = false
        };
        foreach (var arg in first.Args)
            startInfo1.ArgumentList.Add(arg);

        // Start second process
        var startInfo2 = new ProcessStartInfo
        {
            FileName = secondPath,
            UseShellExecute = false,
            RedirectStandardInput = true,
            RedirectStandardOutput = false,
            RedirectStandardError = false
        };
        foreach (var arg in second.Args)
            startInfo2.ArgumentList.Add(arg);

        using var process1 = Process.Start(startInfo1);
        using var process2 = Process.Start(startInfo2);
        
        if (process1 == null || process2 == null) return;

        // Copy data from process1 stdout to process2 stdin in background
        var copyTask = Task.Run(() =>
        {
            try
            {
                byte[] buffer = new byte[4096];
                int bytesRead;
                
                while ((bytesRead = process1.StandardOutput.BaseStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    process2.StandardInput.BaseStream.Write(buffer, 0, bytesRead);
                    process2.StandardInput.BaseStream.Flush();
                }
                
                process2.StandardInput.Close();
            }
            catch
            {
                try { process2.StandardInput.Close(); } catch { }
            }
        });

        // Wait for second process to complete
        process2.WaitForExit();

        // Clean up first process if still running
        if (!process1.HasExited)
        {
            try
            {
                if (!process1.WaitForExit(100))
                {
                    process1.Kill();
                }
            }
            catch { }
        }

        copyTask.Wait(1000);
    }

    /// <summary>Captures the stdout output of a builtin command</summary>
    private static string CaptureBuiltinOutput(Command cmd)
    {
        var originalOut = Console.Out;
        using var writer = new StringWriter();
        Console.SetOut(writer);
        
        try
        {
            ExecuteBuiltinCommand(cmd);
        }
        finally
        {
            Console.SetOut(originalOut);
        }
        
        return writer.ToString();
    }

    /// <summary>Executes a builtin command</summary>
    private static void ExecuteBuiltinCommand(Command cmd)
    {
        switch (cmd.Name)
        {
            case "echo":
                var echoContent = string.Join(" ", cmd.Args);
                Builtins.HandleEcho(echoContent);
                break;

            case "type":
                var typeInput = cmd.Args.Length > 0 
                    ? $"type {string.Join(" ", cmd.Args)}" 
                    : "type";
                Builtins.HandleType(typeInput);
                break;

            case "pwd":
                Builtins.HandlePwd(null);
                break;

            case "cat":
                Builtins.HandleCat(cmd.Args);
                break;

            case "cd":
                Navigation.ChangeDir(cmd.Args);
                break;

            case "exit":
                Environment.Exit(0);
                break;

    case "history":
        if (cmd.Args.Length > 0 && int.TryParse(cmd.Args[0], out int limit))
        {
            History.Print(limit);
        }
        else
        {
            History.Print();
        }
        break;
        }
    }

    /// <summary>Legacy method for external-only pipelines with more than 2 commands</summary>
    private static void ExecuteExternalPipeline(Pipeline pipeline)
    {
        var commands = pipeline.Commands;
        var processes = new List<Process>();
        var copyTasks = new List<Task>();

        try
        {
            for (int i = 0; i < commands.Count; i++)
            {
                var cmd = commands[i];
                var programPath = ProcessRunner.FindInPath(cmd.Name);
                
                if (programPath == null)
                {
                    Console.WriteLine($"{cmd.Name}: command not found");
                    foreach (var p in processes)
                    {
                        try { p.Kill(); } catch { }
                        p.Dispose();
                    }
                    return;
                }

                var startInfo = new ProcessStartInfo
                {
                    FileName = programPath,
                    UseShellExecute = false,
                    RedirectStandardInput = i > 0,
                    RedirectStandardOutput = i < commands.Count - 1,
                    RedirectStandardError = false
                };

                foreach (var arg in cmd.Args)
                    startInfo.ArgumentList.Add(arg);

                var process = new Process { StartInfo = startInfo };
                process.Start();
                processes.Add(process);

                if (i > 0)
                {
                    var previousProcess = processes[i - 1];
                    var currentProcess = process;
                    
                    var copyTask = Task.Run(() =>
                    {
                        try
                        {
                            byte[] buffer = new byte[4096];
                            int bytesRead;
                            
                            while ((bytesRead = previousProcess.StandardOutput.BaseStream.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                currentProcess.StandardInput.BaseStream.Write(buffer, 0, bytesRead);
                                currentProcess.StandardInput.BaseStream.Flush();
                            }
                            
                            currentProcess.StandardInput.Close();
                        }
                        catch
                        {
                            try { currentProcess.StandardInput.Close(); } catch { }
                        }
                    });
                    copyTasks.Add(copyTask);
                }
            }

            var lastProcess = processes[^1];
            lastProcess.WaitForExit();

            foreach (var process in processes)
            {
                if (!process.HasExited)
                {
                    try
                    {
                        if (process.StartInfo.RedirectStandardInput)
                        {
                            try { process.StandardInput.Close(); } catch { }
                        }
                        
                        if (!process.WaitForExit(100))
                        {
                            process.Kill();
                        }
                    }
                    catch { }
                }
            }

            Task.WaitAll(copyTasks.ToArray(), 1000);
        }
        finally
        {
            foreach (var process in processes)
            {
                process.Dispose();
            }
        }
    }
}
