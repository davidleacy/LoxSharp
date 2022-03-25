namespace LoxSharp.NativeFunctions;

using LoxSharp.Interpreter;
using System.Collections.Generic;

internal class NativeClockFunction : ILoxSharpCallable
{
    public int Arity() => 0;

    public object? Call(Interpreter interpreter, List<object?> arguments) => (double) DateTimeOffset.UtcNow.Second;

    public override string ToString() { return "<native fn>"; }
}
