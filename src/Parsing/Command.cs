using System;

public class Command
{
    public string Name { get; }
    public string[] Args { get; }
    public string? OutputFile { get; }
    public string? ErrorFile { get; }
    public string? RawInput { get; }

    public Command(string name, string[] args, string? outputFile, string? errorFile, string? rawInput)
    {
        Name = name;
        Args = args;
        OutputFile = outputFile;
        ErrorFile = errorFile;
        RawInput = rawInput;
    }
}
