using System;
using System.Text;
using System.Collections.Generic;

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
            var input = AutoCompletion.ReadInput();
            if (input != null)
            {

                // Process the command
                ProcessCommands.ProcessCommand(input);
            }
        }
    }
}
