using System;
using System.IO;

public static class OutputRedirection
{
    public static void RunWithOutputRedirection(Command command, Action action)
    {
        var originalOut = Console.Out;
        var originalErr = Console.Error;
        StreamWriter? outWriter = null;
        StreamWriter? errWriter = null;
        try
        {
            if (command.OutputFile != null)
            {
                // Use FileMode.Append or FileMode.Create based on AppendOutput flag
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
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error during output redirection: {ex.Message}");
        }
        finally
        {
            outWriter?.Flush();
            outWriter?.Dispose();
            errWriter?.Flush();
            errWriter?.Dispose();
            Console.SetOut(originalOut);
            Console.SetError(originalErr);
        }
    }
}
