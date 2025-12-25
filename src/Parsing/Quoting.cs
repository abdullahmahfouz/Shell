using System.Text;
using System.Collections.Generic;

/// <summary>Handles parsing of quoted strings and command-line arguments</summary>
public class Quoting
{
    /// <summary>Parses command line into arguments (handles quotes, escapes, and spaces)</summary>
    /// <param name="input">The command line string to parse</param>
    /// <returns>An array of parsed arguments</returns>
    public static string[] ParseCommandLine(string input)
    {
        var args = new List<string>();
        var currentArg = new StringBuilder();
        var i = 0;
        var inQuotes = false;
        var quoteChar = '\0';

        while (i < input.Length)
        {
            var ch = input[i];

            // Start of quoted string
            if ((ch == '\'' || ch == '"') && !inQuotes)
            {
                inQuotes = true;
                quoteChar = ch;
                i++;
                continue;
            }

            // End of quoted string
            if (inQuotes && ch == quoteChar)
            {
                inQuotes = false;
                quoteChar = '\0';
                i++;
                continue;
            }

            // Space outside quotes ends argument
            if (ch == ' ' && !inQuotes)
            {
                if (currentArg.Length > 0)
                {
                    args.Add(currentArg.ToString());
                    currentArg.Clear();
                }
                i++;
                continue;
            }

            // Backslash escape outside quotes (escapes next char literally)
            if (ch == '\\' && i + 1 < input.Length && !inQuotes)
            {
                i++;
                currentArg.Append(input[i]);
                i++;
                continue;
            }

            // Backslash inside double quotes (selective escape)
            if (ch == '\\' && i + 1 < input.Length && inQuotes && quoteChar == '"')
            {
                char nextChar = input[i + 1];
                if (nextChar == '"' || nextChar == '\\' || nextChar == '$' || nextChar == '`')
                {
                    i++;
                    currentArg.Append(nextChar);
                    i++;
                    continue;
                }
            }

            // Regular character
            currentArg.Append(ch);
            i++;
        }

        if (currentArg.Length > 0)
        {
            args.Add(currentArg.ToString());
        }

        return args.ToArray();
    }

    //------------------------------------------------------------------------------------------------------

    /// <summary>Parses strings for echo (preserves spaces in quotes, collapses outside)</summary>
    /// <param name="input">The string to parse</param>
    /// <returns>The parsed string with proper quote handling</returns>
    public static string ParseQuotedString(string input)
    {
        var result = new StringBuilder();
        var i = 0;

        while (i < input.Length)
        {
            // Single quoted string (literal)
            if (input[i] == '\'')
            {
                i++;
                while (i < input.Length && input[i] != '\'')
                {
                    result.Append(input[i]);
                    i++;
                }
                if (i < input.Length) i++;  // Skip closing quote
                continue;
            }

            // Double quoted string (with selective escape)
            if (input[i] == '"')
            {
                i++;
                while (i < input.Length && input[i] != '"')
                {
                    if (input[i] == '\\' && i + 1 < input.Length)
                    {
                        var next = input[i + 1];
                        if (next == '"' || next == '\\' || next == '$' || next == '`')
                        {
                            i++;
                            result.Append(input[i]);
                            i++;
                            continue;
                        }
                    }
                    result.Append(input[i]);
                    i++;
                }
                if (i < input.Length) i++;  // Skip closing quote
                continue;
            }

            // Multiple spaces collapse to one outside quotes
            if (input[i] == ' ')
            {
                if (result.Length > 0 && result[result.Length - 1] != ' ')
                {
                    result.Append(' ');
                }
                i++;
                continue;
            }

            // Backslash escape outside quotes
            if (input[i] == '\\' && i + 1 < input.Length)
            {
                i++;
                result.Append(input[i]);
                i++;
                continue;
            }

            // Regular character
            result.Append(input[i]);
            i++;
        }

        return result.ToString().TrimEnd();
    }
}
