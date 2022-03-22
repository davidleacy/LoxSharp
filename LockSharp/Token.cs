namespace LoxSharp;

/// <summary>
/// Record representing a Token.
/// </summary>
/// <param name="type">The token type. <see cref="TokenType"/>.</param>
/// <param name="lexeme">The lexeme the token rpresents.</param>
/// <param name="literal">The literal represented by the token.</param>
/// <param name="line">The line the token is on in source code.</param>
internal record struct Token
(
    TokenType type,
    string lexeme,
    object? literal,
    int line
);