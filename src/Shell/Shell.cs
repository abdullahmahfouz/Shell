using System;

/// <summary>Main shell REPL (Read-Eval-Print Loop)</summary>
class Program
{
    /// <summary>Entry point for the shell application</summary>
    static void Main(string[] args)
    {
        while (true)
        {
            // Display prompt
            Console.Write("$ ");
            
            // Read user input
            var input = Console.ReadLine();
            if (input != null)
            {
                // Process the command
                ProcessCommands.ProcessCommand(input);
            }
        }
    }
}
