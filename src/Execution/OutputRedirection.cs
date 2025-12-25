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
                // append mode if AppendOutput is true
                outWriter = new StreamWriter(command.OutputFile, command.AppendOutput) { AutoFlush = true };
                Console.SetOut(outWriter);
            }

            if (command.ErrorFile != null)
            {
                errWriter = new StreamWriter(command.ErrorFile, command.AppendOutput) { AutoFlush = true };
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
