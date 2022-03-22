namespace LoxSharp;

using System.Text;
using static System.FormattableString;

public class Program
{
    public static bool HadError { get; set; } = false;

    /// <summary>
    /// Main entry point to the LoxSharp interpreter.
    /// </summary>
    /// <param name="args"></param>
    public static void Main(string[] args)
    {
        if (args.Length > 0)
        {
            Console.WriteLine("Usage: LoxSharp [script]");
        }
        else if (args.Length == 1)
        {
            RunFile(args[0]);
        }
        else
        {
            RunPrompt();
        }
    }

    /// <summary>
    /// Run a specfied file through the LoxSharp Interpreter.
    /// </summary>
    /// <param name="path">Path to the loxSharp file. (.lox)</param>
    private static void RunFile(String path)
    {
        byte[] bytes = File.ReadAllBytes(path);

        Run(Encoding.UTF8.GetString(bytes));

        if (HadError)
        {
            // If an error occurs be good command line citizens and return an error status code.
            Environment.Exit(-65);
        }
    }

    /// <summary>
    /// Interactive prompt to run lox code line by line.
    /// </summary>
    private static void RunPrompt()
    {
        while (true)
        {
            Console.Write("> ");
            String? line = Console.ReadLine();
            if (line == null) break;
            Run(line);
            // If the user caused an error reset the erorr flag to allow them to continue.
            HadError = false;
        }

        Console.WriteLine("Prompt exited.");
    }

    /// <summary>
    /// Run the specified source code.
    /// </summary>
    /// <param name="source">The source code to be ran.</param>
    private static void Run(string source)
    {
        Scanner.Scanner scanner = new Scanner.Scanner(source);
        List<Token> tokens = scanner.ScanTokens();

        // For now, just print the tokens.
        foreach (Token token in tokens)
        {
            Console.WriteLine(token);
        }
    }

    public static void Error(int line, string message)
    {
        Report(line, string.Empty, message);
    }

    private static void Report(
        int line,
        string where,
        string message)
    {
        Console.Error.WriteLine(Invariant($"[line {line}] Error {where}: {message}"));

        HadError = true;
    }
}
