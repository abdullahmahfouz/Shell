using System;
using System.IO;

public static class OutputRedirection
{
    public static void RunWithOutputRedirection(Command command, Action action)
        {
            var originalOut = Console.Out;
            StreamWriter outWriter = null;
            try
            {
                if (command.OutputFile != null)
                {
                   outWriter = new StreamWriter(command.OutputFile, false) { AutoFlush = true };
                   Console.SetOut(outWriter);
                }
    
                action();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during output redirection: {ex.Message}");
            }
            finally
            {
                if (outWriter != null)
                {
                    Console.Out.Flush();
                    outWriter.Dispose();
                }
                Console.SetOut(originalOut);
            }
        }
}
