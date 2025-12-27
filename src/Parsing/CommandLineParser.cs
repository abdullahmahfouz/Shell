using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>Parses command-line input into Command objects with quote and redirection handling</summary>
public static class CommandLineParser
{
    /// <summary>Parses a command line string into a Pipeline object (handles pipe operator)</summary>
    /// <param name="input">The raw command line input</param>
    /// <returns>A Pipeline object with parsed commands, or null if empty</returns>
    public static Pipeline? ParsePipeline(string input)
    {
        var commandStrings = SplitByPipe(input);
        var commands = new List<Command>();

        foreach (var cmdStr in commandStrings)
        {
            var trimmed = cmdStr.Trim();
            if (string.IsNullOrEmpty(trimmed))
                continue;

            var cmd = ParseSingleCommand(trimmed);
            if (cmd != null)
            {
                commands.Add(cmd);
            }
        }

        if (commands.Count == 0)
            return null;

        return new Pipeline(commands);
    }

    /// <summary>Splits command line by pipe operator, respecting quotes</summary>
    /// <param name="input">The raw command line input</param>
    /// <returns>Array of command strings separated by pipes</returns>
    private static string[] SplitByPipe(string input)
    {
        var commands = new List<string>();
        var current = new System.Text.StringBuilder();
        var inSingleQuote = false;
        var inDoubleQuote = false;
        var i = 0;

        while (i < input.Length)
        {
            var ch = input[i];

            // Handle quote state transitions
            if (ch == '\'' && !inDoubleQuote)
            {
                inSingleQuote = !inSingleQuote;
                current.Append(ch);
                i++;
                continue;
            }

            if (ch == '"' && !inSingleQuote)
            {
                inDoubleQuote = !inDoubleQuote;
                current.Append(ch);
                i++;
                continue;
            }

            // Handle backslash escapes in double quotes
            if (ch == '\\' && inDoubleQuote && i + 1 < input.Length)
            {
                current.Append(ch);
                current.Append(input[i + 1]);
                i += 2;
                continue;
            }

            // Pipe operator outside quotes splits commands
            if (ch == '|' && !inSingleQuote && !inDoubleQuote)
            {
                commands.Add(current.ToString());
                current.Clear();
                i++;
                continue;
            }

            current.Append(ch);
            i++;
        }

        // Add the last command
        if (current.Length > 0)
        {
            commands.Add(current.ToString());
        }

        return commands.ToArray();
    }

    /// <summary>Parses a command line string into a Command object</summary>
    /// <param name="input">The raw command line input</param>
    /// <returns>A Command object with parsed arguments and redirection info, or null if empty</returns>
    public static Command? Parse(string input)
    {
        return ParseSingleCommand(input);
    }

    /// <summary>Parses a single command (without pipes) into a Command object</summary>
    /// <param name="input">The raw command line input</param>
    /// <returns>A Command object with parsed arguments and redirection info, or null if empty</returns>
    private static Command? ParseSingleCommand(string input)
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
