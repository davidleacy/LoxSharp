namespace LoxSharp.Interpreter;

using LoxSharp.AbstractSyntaxTrees;

/// <summary>
/// Implements <see cref="ILoxSharpCallable"/> so that we can call functions.
/// </summary>
internal class LoxSharpFunction : ILoxSharpCallable
{
    private readonly Stmt.Function Declaration;

    /// <summary>
    /// Used to store the captured environment in order for closures to work correctly.
    /// </summary>
    private readonly Environment.Environment Closure;

    /// <summary>
    /// Initializes a new instance of the <see cref="LoxSharpFunction"/> class.
    /// </summary>
    /// <param name="declaration">Function declaration.</param>
    /// <param name="closure">Capture the enclosing environment at function declaration.</param>
    public LoxSharpFunction(Stmt.Function declaration, Environment.Environment closure)
    {
        Declaration = declaration;
        Closure = closure;
    }

    /// <inheritdoc/>
    public int Arity() => Declaration.parameters.Count;

    /// <inheritdoc/>
    public object? Call(Interpreter interpreter, List<object?> arguments)
    {
        // Each function call gets it's own environment for which to store variables.
        Environment.Environment environment = new Environment.Environment(Closure);
        for (int i = 0; i < Declaration.parameters.Count; i++) {
            environment.Define(Declaration.parameters[i],
                arguments[i]);
        }

        try
        {
            interpreter.ExecuteBlock(Declaration.body, environment);
        }
        catch (ReturnException returnValue)
        {
            return returnValue.Value;
        }

        return null;
    }

    public override string ToString()
    {
        return "<fn " + Declaration.name.Lexeme + ">";
    }
}
