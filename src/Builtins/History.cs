using System;
using System.Collections.Generic;
using System.IO;

public static class History
{
    private static readonly List<string> commandHistory = new List<string>();
    private static int lastSavedIndex = 0;  // Track what's already been saved

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
        // After reading, mark all as "saved" so -a won't duplicate them
        lastSavedIndex = commandHistory.Count;
    }

    /// <summary>Writes ALL history to file (overwrites existing content)</summary>
    public static void WriteToFile(string filePath)
    {
        CreateDirectoryIfNeeded(filePath);
        File.WriteAllLines(filePath, commandHistory);
        lastSavedIndex = commandHistory.Count;
    }

    /// <summary>Appends only NEW commands to file (since last read/write)</summary>
    public static void AppendToFile(string filePath)
    {
        CreateDirectoryIfNeeded(filePath);
        
        // Only append commands that haven't been saved yet
        var newCommands = new List<string>();
        for (int i = lastSavedIndex; i < commandHistory.Count; i++)
        {
            newCommands.Add(commandHistory[i]);
        }
        
        File.AppendAllLines(filePath, newCommands);
        lastSavedIndex = commandHistory.Count;
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