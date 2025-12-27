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
    /// <summary>Executes a pipeline of two external commands</summary>
    /// <param name="pipeline">The pipeline containing commands to execute</param>
    public static void Execute(Pipeline pipeline)
    {
        if (pipeline.Commands.Count < 2)
        {
            // Single command, use regular execution
            return;
        }

        var commands = pipeline.Commands;
        var processes = new List<Process>();
        var copyTasks = new List<Task>();

        try
        {
            // Create processes for each command in the pipeline
            for (int i = 0; i < commands.Count; i++)
            {
                var cmd = commands[i];
                var programPath = ProcessRunner.FindInPath(cmd.Name);
                
                if (programPath == null)
                {
                    Console.WriteLine($"{cmd.Name}: command not found");
                    // Clean up any already started processes
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
                    RedirectStandardInput = i > 0,  // First command reads from terminal
                    RedirectStandardOutput = i < commands.Count - 1,  // Last command writes to terminal
                    RedirectStandardError = false
                };

                // Add arguments
                foreach (var arg in cmd.Args)
                {
                    startInfo.ArgumentList.Add(arg);
                }

                var process = new Process { StartInfo = startInfo };
                process.Start();
                processes.Add(process);

                // Connect previous command's stdout to this command's stdin
                if (i > 0)
                {
                    var previousProcess = processes[i - 1];
                    var currentProcess = process;
                    
                    // Copy data from previous stdout to current stdin in background
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
                            // Ignore errors during pipe copy (process may have exited)
                            try { currentProcess.StandardInput.Close(); } catch { }
                        }
                    });
                    copyTasks.Add(copyTask);
                }
            }

            // Wait for the last process to complete (this is what determines pipeline completion)
            // For example, in `tail -f file | head -n 5`, head will exit after 5 lines
            // which will cause a broken pipe to tail, terminating it
            var lastProcess = processes[^1];
            lastProcess.WaitForExit();

            // After the last process exits, give a brief moment for cleanup
            // Then terminate any remaining processes (like tail -f)
            foreach (var process in processes)
            {
                if (!process.HasExited)
                {
                    try
                    {
                        // Close the stdin to signal end of input
                        if (process.StartInfo.RedirectStandardInput)
                        {
                            try { process.StandardInput.Close(); } catch { }
                        }
                        
                        // Give a small window for graceful exit
                        if (!process.WaitForExit(100))
                        {
                            process.Kill();
                        }
                    }
                    catch { }
                }
            }

            // Wait for copy tasks to complete
            Task.WaitAll(copyTasks.ToArray(), 1000);
        }
        finally
        {
            // Dispose all processes
            foreach (var process in processes)
            {
                process.Dispose();
            }
        }
    }
}
