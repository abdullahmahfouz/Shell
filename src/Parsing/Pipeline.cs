using System;
using System.Collections.Generic;

/// <summary>Represents a pipeline of commands connected by pipes</summary>
public class Pipeline
{
    /// <summary>The list of commands in the pipeline, in execution order</summary>
    public List<Command> Commands { get; }

    /// <summary>Creates a new pipeline with the given commands</summary>
    public Pipeline(List<Command> commands)
    {
        Commands = commands;
    }

    /// <summary>Returns true if this pipeline has more than one command</summary>
    public bool IsPipeline => Commands.Count > 1;
}
