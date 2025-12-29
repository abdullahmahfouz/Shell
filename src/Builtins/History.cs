using System;
using System.Collections.Generic;
using System.IO;

/// <summary>Stores and manages command history for the shell</summary>
public static class History
{
    private static readonly List<string> commandHistory = new List<string>();

    /// <summary>Adds a command to the history</summary>
    public static void Add(string command)
    {
        if (!string.IsNullOrWhiteSpace(command))
        {
            commandHistory.Add(command);
        }
    }

    /// <summary>Reads history from a file and appends to current history</summary>
    /// <param name="filePath">Path to the history file</param>
    public static void ReadFromFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return;
        }

        var lines = File.ReadAllLines(filePath);
        foreach (var line in lines)
        {
            // Only add non-empty lines
            if (!string.IsNullOrWhiteSpace(line))
            {
                commandHistory.Add(line);
            }
        }
    }

    /// <summary>Prints history with optional limit</summary>
    public static void Print(int? limit = null)
    {
        int startIndex = 0;
        int count = commandHistory.Count;

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