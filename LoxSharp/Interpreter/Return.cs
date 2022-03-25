using LoxSharp.Models;

using System.Runtime.Serialization;

/// <summary>
/// Exception used to bubble up return values.
/// </summary>
namespace LoxSharp.Interpreter
{
    internal class ReturnException : RuntimeErrorException
    {
        public readonly object? Value;

        public ReturnException(object? value) : base(null) => Value = value;

        public ReturnException(object? value, string? message) : base(null, message) => Value = value;

        public ReturnException(object? value, string? message, Exception? innerException) : base(null, message, innerException) => Value = value;

        protected ReturnException(object? value, SerializationInfo info, StreamingContext context) : base(null, info, context) => Value = value;
    }
}
