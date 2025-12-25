using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>Parses command-line input into Command objects with quote and redirection handling</summary>
public static class CommandLineParser
{
    /// <summary>Parses a command line string into a Command object</summary>
    /// <param name="input">The raw command line input</param>
    /// <returns>A Command object with parsed arguments and redirection info, or null if empty</returns>
    public static Command? Parse(string input)
    {
        var parts = Quoting.ParseCommandLine(input);
        
        if (parts.Length == 0)
            return null;

        var command = parts[0];
        var args = parts.Skip(1).ToArray();

        string? outputFile = null;
        string? errorFile = null;
        var appendFile = false;
        var finalArgs = new List<string>();

        // Extract redirection operators from arguments
        for (var i = 0; i < args.Length; i++)
        {
            var arg = args[i];

            // Append operators: >> and 1>>
            if (arg == ">>" || arg == "1>>")
            {
                appendFile = true;
                if (i + 1 < args.Length)
                {
                    outputFile = args[++i];
                }
                continue;
            }

            if (arg.StartsWith(">>") || arg.StartsWith("1>>"))
            {
                appendFile = true;
                var trimmed = arg.StartsWith("1>>") ? arg[3..] : arg[2..];
                if (!string.IsNullOrEmpty(trimmed))
                {
                    outputFile = trimmed;
                    continue;
                }
            }

            // Error append: 2>>
            if (arg == "2>>")
            {
                appendFile = true;
                if (i + 1 < args.Length)
                {
                    errorFile = args[++i];
                }
                continue;
            }

            if (arg.StartsWith("2>>"))
            {
                appendFile = true;
                var trimmed = arg[3..];
                if (!string.IsNullOrEmpty(trimmed))
                {
                    errorFile = trimmed;
                    continue;
                }
            }

            // Truncate operators: > and 1>
            if (arg == ">" || arg == "1>")
            {
                appendFile = false;
                if (i + 1 < args.Length)
                {
                    outputFile = args[++i];
                }
                continue;
            }

            if (arg.StartsWith(">") && !arg.StartsWith(">>"))
            {
                appendFile = false;
                var trimmed = arg.StartsWith("1>") ? arg[2..] : arg[1..];
                if (!string.IsNullOrEmpty(trimmed))
                {
                    outputFile = trimmed;
                    continue;
                }
            }

            // Error truncate: 2>
            if (arg == "2>")
            {
                if (i + 1 < args.Length)
                {
                    errorFile = args[++i];
                }
                continue;
            }

            if (arg.StartsWith("2>"))
            {
                var trimmed = arg[2..];
                if (!string.IsNullOrEmpty(trimmed))
                {
                    errorFile = trimmed;
                    continue;
                }
            }

            finalArgs.Add(arg);
        }

        return new Command(command, finalArgs.ToArray(), outputFile, errorFile, input, appendFile);
    }
}
