// Import necessary namespaces for shell functionality
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;

class Program
{
    // Entry point of the shell program
    static void Main(string[] args)
    {
        // Main shell loop - continuously read and execute commands
        while (true)
        {
            // Display the shell prompt
            Console.Write("$ ");
            // Read user input
            var input = Console.ReadLine();
            
            // Process the command if input is not null
            if (input != null)
            {
                ProcessCommands.ProcessCommand(input);
            }
        }
    }
}