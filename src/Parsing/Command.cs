using System;
using System.Runtime.CompilerServices;

/// <summary>Represents a parsed shell command with arguments and I/O redirection</summary>
public class Command
{
    public string Name { get; }
    public string[] Args { get; }
    public string? OutputFile { get; }  // stdout redirection target
    public string? ErrorFile { get; }   // stderr redirection target
    public string? RawInput { get; }
    /// <summary>True for >> (append), false for > (truncate)</summary>
    public bool AppendOutput { get; }

    /// <summary>Initializes a new Command with parsed arguments and redirection info</summary>
    public Command(
        string name,
        string[] args,
        string? outputFile,
        string? errorFile,
        string? rawInput,
        bool appendOutput = false)
    {
        Name = name;
        Args = args;
        OutputFile = outputFile;
        ErrorFile = errorFile;
        RawInput = rawInput;
        AppendOutput = appendOutput;
    }
}
