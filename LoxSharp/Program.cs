namespace LoxSharp;

using LoxSharp.AbstractSyntaxTrees;
using LoxSharp.Interpreter;
using LoxSharp.Models;
using System.Text;
using static System.FormattableString;

public class Program
{
    private static readonly Interpreter.Interpreter Interpreter = new Interpreter.Interpreter();

    public static bool HadError { get; set; } = false;
    public static bool HadRuntimeError { get; set; } = false;

    /// <summary>
    /// Main entry point to the LoxSharp interpreter.
    /// </summary>
    /// <param name="args"></param>
    public static void Main(string[] args)
    {
        if (args.Length > 1)
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
    private static void RunFile(string path)
    {
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
        using var sr = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        string source = sr.ReadToEnd();

        Run(source);

        if (HadError)
        {
            // If an error occurs be good command line citizens and return an error status code.
            System.Environment.Exit(-65);
        }
        else if (HadRuntimeError)
        {
            // If an error occurs be good command line citizens and return an error status code.
            System.Environment.Exit(-70);
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

        Parser.Parser parser = new Parser.Parser(tokens);
        List<Stmt> statements = parser.Parse();

        // Stop if there was a syntax error.
        if (HadError) return;

        Interpreter.Interpret(statements);
    }

    private static void Report(
        int line,
        string where,
        string message)
    {
        Console.Error.WriteLine(Invariant($"[line {line}] Error {where}: {message}"));

        HadError = true;
    }
    internal static void Error(int line, string message)
    {
        Report(line, string.Empty, message);
    }

    internal static void Error(Token token, string message)
    {
        if (token.Type == TokenType.EOF)
        {
            Report(token.Line, " at end", message);
        }
        else
        {
            Report(token.Line, " at '" + token.Lexeme + "'", message);
        }
    }

    internal static void RuntimeError(RuntimeErrorException error)
    {
        Console.WriteLine(error.Message +
            "\n[line " + error?.Token?.Line + "]");
        HadRuntimeError = true;
    }
}
