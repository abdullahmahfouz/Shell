using System;
using System.Runtime.CompilerServices;

public class Command
{
    public string Name { get; }
    public string[] Args { get; }
    public string? OutputFile { get; }
    public string? ErrorFile { get; }
    public string? RawInput { get; }
    public bool AppendOutput { get; }

    public Command(string name, string[] args, string? outputFile, string? errorFile, string? rawInput, bool appendOutput = false)
    {
        Name = name;
        Args = args;
        OutputFile = outputFile;
        ErrorFile = errorFile;
        RawInput = rawInput;
        AppendOutput = appendOutput;
    }
}
