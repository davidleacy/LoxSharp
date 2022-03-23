namespace LoxSharp.Interpreter;

using LoxSharp.Models;
using System.Runtime.Serialization;

/// <summary>
/// Custom RuntimeError to  represent runtime errors in the LoxSharp interpreter.
/// </summary>
internal class RuntimeErrorException: SystemException
{
    public readonly Token Token;

    public RuntimeErrorException(Token token) => this.Token = token;

    public RuntimeErrorException(Token token, string? message) : base(message) => this.Token = token;

    public RuntimeErrorException(Token token, string? message, Exception? innerException) : base(message, innerException) => this.Token = token;

    protected RuntimeErrorException(Token token, SerializationInfo info, StreamingContext context) : base(info, context) => this.Token = token;
}
