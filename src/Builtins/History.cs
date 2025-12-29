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
}