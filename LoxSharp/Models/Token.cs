namespace LoxSharp.Models;

/// <summary>
/// Record representing a Token.
/// </summary>
/// <param name="Type">The token type. <see cref="TokenType"/>.</param>
/// <param name="Lexeme">The lexeme the token rpresents.</param>
/// <param name="Literal">The literal represented by the token.</param>
/// <param name="Line">The line the token is on in source code.</param>
internal record struct Token
(
    TokenType Type,
    string Lexeme,
    object? Literal,
    int Line
);
