using System;
using System.Collections.Generic;

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

    public static void Print()
    {
        for (int i = 0; i < commandHistory.Count; i++)
        {
            Console.WriteLine($"{i + 1,5}  {commandHistory[i]}");
        }
    }

    public static IReadOnlyList<string> GetHistory() => commandHistory.AsReadOnly();
}