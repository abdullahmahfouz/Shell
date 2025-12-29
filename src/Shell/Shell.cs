using System;
using System.Text;
using System.Collections.Generic;

/// <summary>Main shell REPL (Read-Eval-Print Loop)</summary>
class Program
{
    /// <summary>Entry point for the shell application</summary>
    static void Main(string[] args)
    {
        // Load history from file on startup
        string historyFile = GetHistoryFilePath();
        if (historyFile != null)
        {
            History.ReadFromFile(historyFile);
        }

        while (true)
        {
            // Display prompt
            Console.Write("$ ");
          
            // Read user input
            var input = AutoCompletion.ReadInput();
            if (input != null)
            {
                // Add command to history
                History.Add(input);

                // Process the command
                ProcessCommands.ProcessCommand(input);
            }
        }
    }

    /// <summary>Gets the history file path from environment or default</summary>
    private static string GetHistoryFilePath()
    {
        // Check HISTFILE environment variable first
        var histFile = Environment.GetEnvironmentVariable("HISTFILE");
        if (!string.IsNullOrEmpty(histFile))
        {
            return histFile;
        }

        // Default: ~/.shell_history
        var home = Environment.GetEnvironmentVariable("HOME");
        if (!string.IsNullOrEmpty(home))
        {
            return System.IO.Path.Combine(home, ".shell_history");
        }

        return null;
    }
}
