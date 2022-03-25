namespace LoxSharp.Interpreter;

/// <summary>
/// Class representing and Lox object which is callable i.e. a function.
/// </summary>
internal interface ILoxSharpCallable
{
    /// <summary>
    /// Provides the Arity of the function.
    /// </summary>
    /// <returns>The function arity.</returns>
    public int Arity();

    /// <summary>
    /// Execute the callable logic on the object/function.
    /// </summary>
    /// <param name="interpreter">The interpreter in case needed by the function.</param>
    /// <param name="arguments">The evaluated arguments to be used by the function.</param>
    /// <returns>The result of calling the function.</returns>
    public object? Call(Interpreter interpreter, List<object?> arguments);
}
