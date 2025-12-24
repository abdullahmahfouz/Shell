using System;

class Program
{
    static void Main(string[] args)
    {
        while (true)
        {
            Console.Write("$ ");
            var input = Console.ReadLine();
            if (input != null)
            {
                ProcessCommands.ProcessCommand(input);
            }
        }
    }
}
