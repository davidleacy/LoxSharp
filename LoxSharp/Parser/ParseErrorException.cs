namespace LoxSharp.Parser;

using System.Runtime.Serialization;

internal class ParseErrorException: SystemException
{
    public ParseErrorException()
    {
    }

    public ParseErrorException(string? message) : base(message)
    {
    }

    public ParseErrorException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected ParseErrorException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}

