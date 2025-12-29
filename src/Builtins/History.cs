using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

public static class History
{
    private static readonly List<string> commandHistory = new List<string>();

    public static void Add(string command)
    {
        if (!string.IsNullOrWhiteSpace(command))
        {
            commandHistory.Add(command);
        }
        
    }

    public static void Print(int ? limit = null)
    {
        int startIndex = 0;
        int count = commandHistory.Count;

        // If limit is specified, only show last n commands
        if (limit.HasValue && limit.Value < count)
        {
            startIndex = count - limit.Value;
        }

        for (int i = startIndex; i < count; i++)
        {
            Console.WriteLine($"{i + 1,5}  {commandHistory[i]}");
        }
    }

    public static IReadOnlyList<string> GetHistory() => commandHistory.AsReadOnly();
}