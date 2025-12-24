using System;
using System.Collections.Generic;
using System.Linq;

public static class CommandLineParser
{
    public static Command? Parse(string input)
    {
        var parts = Quoting.ParseCommandLine(input);
        if (parts.Length == 0)
        {
            return null;
        }

        var command = parts[0];
        var args = parts.Skip(1).ToArray();

        string? outputFile = null;
        var finalArgs = new List<string>();

        for (var i = 0; i < args.Length; i++)
        {
            var arg = args[i];

            // Handle stdout redirection tokens: ">", "1>", and no-space forms like ">/tmp/file" or "1>/tmp/file"
            if (arg == ">" || arg == "1>")
            {
                if (i + 1 < args.Length)
                {
                    outputFile = args[i + 1];
                    i++; // skip the filename token
                }
                continue;
            }

            if (arg.StartsWith(">") || arg.StartsWith("1>"))
            {
                var trimmed = arg.StartsWith("1>") ? arg.Substring(2) : arg.Substring(1);
                if (!string.IsNullOrEmpty(trimmed))
                {
                    outputFile = trimmed;
                    continue;
                }
            }

            finalArgs.Add(arg);
        }

        return new Command(command, finalArgs.ToArray(), outputFile, input);
    }
}
