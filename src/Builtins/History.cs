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

         if (limit.HasValue && limit.Value < count)
        {
            startIndex = count - limit.Value;
        }
        {
            startIndex = Math.Max(0, commandHistory.Count - limit.Value);
            count = commandHistory.Count - startIndex;
        }
        for (int i = 0; i < commandHistory.Count; i++)
        {
            Console.WriteLine($"{i + 1,5}  {commandHistory[i]}");
        }
    }

    public static IReadOnlyList<string> GetHistory() => commandHistory.AsReadOnly();
}