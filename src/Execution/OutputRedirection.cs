using System;
using System.IO;

/// <summary>Handles output redirection for builtin commands</summary>
/// <remarks>Currently unused - ProcessCommands handles redirection directly</remarks>
public static class OutputRedirection
{
    /// <summary>Executes action with stdout/stderr redirected to files</summary>
    /// <param name="command">The command with redirection info</param>
    /// <param name="action">The action to execute with redirected output</param>
   
    public static void RunWithOutputRedirection(Command command, Action action)
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
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error during output redirection: {ex.Message}");
        }
        finally
        {
            // Restore original streams
            outWriter?.Flush();
            outWriter?.Dispose();
            errWriter?.Flush();
            errWriter?.Dispose();
            Console.SetOut(originalOut);
            Console.SetError(originalErr);
        }
    }
}
