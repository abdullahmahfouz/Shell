using System.Text;
using System.Collections.Generic;

public class Quoting
{
    // Parse command line into array of arguments, respecting quotes
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

            // Handle quote characters
            if ((ch == '\'' || ch == '\"') && !inQuotes)
            {
                inQuotes = true;
                quoteChar = ch;
                i++;
            }
            else if (inQuotes && ch == quoteChar)
            {
                inQuotes = false;
                quoteChar = '\0';
                i++;
            }
            // Handle spaces - delimit arguments when not in quotes
            else if (ch == ' ' && !inQuotes)
            {
                if (currentArg.Length > 0)
                {
                    args.Add(currentArg.ToString());
                    currentArg.Clear();
                }
                i++;
            }
            // Handle backslash escapes outside quotes
            else if (ch == '\\' && i + 1 < input.Length && !inQuotes)
            {
                i++;
                currentArg.Append(input[i]);
                i++;
            }
            // Handle backslash escapes inside double quotes (but not single quotes)
            // Only escape specific characters: ", \, $, `
            else if (ch == '\\' && i + 1 < input.Length && inQuotes && quoteChar == '\"')
            {
                char nextChar = input[i + 1];
                if (nextChar == '\"' || nextChar == '\\' || nextChar == '$' || nextChar == '`')
                {
                    i++; // Skip backslash
                    currentArg.Append(nextChar);
                    i++;
                }
                else
                {
                    // For other characters, preserve the backslash
                    currentArg.Append(ch);
                    i++;
                }
            }
            // Regular character
            else
            {
                currentArg.Append(ch);
                i++;
            }
        }

        // Add the last argument if any
        if (currentArg.Length > 0)
        {
            args.Add(currentArg.ToString());
        }

        return args.ToArray();
    }

    // Parse a string with mixed quoted and unquoted content for echo command
    // Preserves spacing INSIDE quotes, collapses spaces OUTSIDE quotes
    public static string ParseQuotedString(string input)
    {
        var result = new StringBuilder();
        var i = 0;

        while (i < input.Length)
        {
            // Handle single quotes - preserve everything inside literally INCLUDING spaces
            if (input[i] == '\'')
            {
                i++; // Skip opening quote
                // Find closing quote and copy everything in between
                while (i < input.Length && input[i] != '\'')
                {
                    result.Append(input[i]);
                    i++;
                }
                i++; // Skip closing quote
            }
            // Handle double quotes - preserve spaces, allow backslash escapes
            else if (input[i] == '\"')
            {
                i++; // Skip opening quote
                // Find closing quote, handle backslash escapes
                while (i < input.Length && input[i] != '\"')
                {
                    if (input[i] == '\\' && i + 1 < input.Length)
                    {
                        i++; // Skip backslash and take next character
                    }
                    result.Append(input[i]);
                    i++;
                }
                i++; // Skip closing quote
            }
            // Handle spaces OUTSIDE quotes - collapse multiple spaces to single space
            else if (input[i] == ' ')
            {
                // Add single space only if not at start and previous char wasn't a space
                if (result.Length > 0 && result[result.Length - 1] != ' ')
                {
                    result.Append(' ');
                }
                i++;
            }
            // Handle backslash escapes outside quotes
            else if (input[i] == '\\' && i + 1 < input.Length)
            {
                i++; // Skip backslash
                result.Append(input[i]);
                i++;
            }
            // Regular character - copy as is
            else
            {
                result.Append(input[i]);
                i++;
            }
        }

        // Trim trailing spaces
        return result.ToString().TrimEnd();
    }
}