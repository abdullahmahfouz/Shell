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
            if (args[i] == ">")
            {
                if (i + 1 < args.Length)
                {
                    outputFile = args[i + 1];
                    i++; // skip file name
                }
            }
            else
            {
                finalArgs.Add(args[i]);
            }
        }

        return new Command(command, finalArgs.ToArray(), outputFile, input);
    }
}
