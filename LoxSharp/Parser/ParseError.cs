namespace LoxSharp.Parser;

using System.Runtime.Serialization;

internal class ParseError: SystemException
{
    public ParseError()
    {
    }

    public ParseError(string? message) : base(message)
    {
    }

    public ParseError(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected ParseError(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}

