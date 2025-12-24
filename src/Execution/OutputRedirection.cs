using System;
using System.IO;

public static class OutputRedirection
{
    public static void RunWithOutputRedirection(string? outputFile, Action action)
    {
        var originalOut = Console.Out;
        try
        {
            if (outputFile != null)
            {
                var fs = File.Create(outputFile);
                var sw = new StreamWriter(fs) { AutoFlush = true };
                Console.SetOut(sw);
            }

            action();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during output redirection: {ex.Message}");
        }
        finally
        {
            if (outputFile != null)
            {
                Console.Out.Flush();
                Console.Out.Dispose();
            }
            Console.SetOut(originalOut);
        }
    }
}
