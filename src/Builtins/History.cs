using System;
using System.Collections.Generic;
using System.IO;

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

    public static void ReadFromFile(string filePath)
    {
        if (!File.Exists(filePath))
            return;

        var lines = File.ReadAllLines(filePath);
        foreach (var line in lines)
        {
            if (!string.IsNullOrWhiteSpace(line))
            {
                commandHistory.Add(line);
            }
        }
    }

    /// <summary>Writes history to file (overwrites existing content)</summary>
    public static void WriteToFile(string filePath)
    {
        CreateDirectoryIfNeeded(filePath);
        // WriteAllLines = OVERWRITE (erase old, write new)
        File.WriteAllLines(filePath, commandHistory);
    }

    /// <summary>Appends history to file (keeps existing content)</summary>
    public static void AppendToFile(string filePath)
    {
        CreateDirectoryIfNeeded(filePath);
        // AppendAllLines = ADD TO END (keep old, add new)
        File.AppendAllLines(filePath, commandHistory);
    }

    private static void CreateDirectoryIfNeeded(string filePath)
    {
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }

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