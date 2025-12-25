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
        string? errorFile = null;
        var appendFile = false;
        var finalArgs = new List<string>();

        for (var i = 0; i < args.Length; i++)
        {
            var arg = args[i];

            if( arg == ">>" || arg == "1>>"){
                appendFile = true;
                if (i + 1 < args.Length){
                    outputFile = args[i + 1];
                    i++; // skip the filename token
                    continue;   
                }
            }

            if( arg.StartsWith(">>") || arg.StartsWith("1>>")){
                appendFile = true;
                var trimmed = arg.StartsWith("1>>") ? arg.Substring(3) : arg.Substring(2);
                if (!string.IsNullOrEmpty(trimmed)){
                    outputFile = trimmed;
                    continue;   
                }
            }

         

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

            // stderr redirection: "2>", "2>/file"
            if (arg == "2>")
            {
                if (i + 1 < args.Length) { errorFile = args[i + 1]; i++; }
                continue;
            }
            if (arg.StartsWith("2>"))
            {
                var trimmed = arg.Substring(2);
                if (!string.IsNullOrEmpty(trimmed)) { errorFile = trimmed; continue; }
            }

            finalArgs.Add(arg);
        }

        return new Command(command, finalArgs.ToArray(), outputFile, errorFile, input, appendFile);
    }
}
