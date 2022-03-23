namespace LoxSharp.Parser;

using LoxSharp.AbstractSyntaxTrees;
using LoxSharp.Models;

/// <summary>
/// The LoxSharp Parser.
/// </summary>
internal class Parser
{
    /// <summary>
    /// List of tokens to parse.
    /// </summary>
    private readonly List<Token> Tokens;

    /// <summary>
    /// Used to keep track of the current token.
    /// </summary>
    private int CurrentTokenIndex = 0;

    /// <summary>
    /// Initializes a new instance of the <see cref="Parser"/> class.
    /// </summary>
    /// <param name="tokens">Tokens to be parsed.</param>
    public Parser(List<Token> tokens) => this.Tokens = tokens;

    /// <summary>
    /// Parses the provided tokens into a syntax tree.
    /// </summary>
    /// <returns>Returns parsed Syntax tree or null if an error occured. TODO: Will change later.</returns>
    public Expr? Parse()
    {
        try
        {
            return ExpressionRule();
        }
        catch (ParseError error)
        {
            return null;
        }
    }

    /// <summary>
    /// Expression grammer rule.
    /// </summary>
    /// <returns>A syntax tree representing the rule.</returns>
    private Expr ExpressionRule() => EqualityRule();

    /// <summary>
    /// Equality grammer rule.
    /// </summary>
    /// <returns>A syntax tree representing the rule.</returns>
    private Expr EqualityRule() 
        => CreateLeftAssociativeBinaryExpr(
            ComparisonRule,
            new List<TokenType> { TokenType.BANG_EQUAL, TokenType.EQUAL_EQUAL },
            ComparisonRule);

    /// <summary>
    /// Comparison grammer rule.
    /// </summary>
    /// <returns>A syntax tree representing the rule.</returns>
    private Expr ComparisonRule()
        => CreateLeftAssociativeBinaryExpr(
            TermRule,
            new List<TokenType> { TokenType.GREATER, TokenType.GREATER_EQUAL, TokenType.LESS, TokenType.LESS_EQUAL },
            TermRule);

    /// <summary>
    /// Term grammer rule.
    /// </summary>
    /// <returns>A syntax tree representing the rule.</returns>
    private Expr TermRule()
        => CreateLeftAssociativeBinaryExpr(
            FactorRule,
            new List<TokenType> { TokenType.MINUS, TokenType.PLUS },
            FactorRule);

    /// <summary>
    /// Factor grammer rule.
    /// </summary>
    /// <returns>A syntax tree representing the rule.</returns>
    private Expr FactorRule()
        => CreateLeftAssociativeBinaryExpr(
            UnaryRule,
            new List<TokenType> { TokenType.SLASH, TokenType.STAR },
            UnaryRule);

    /// <summary>
    /// Unary grammer rule.
    /// </summary>
    /// <returns>A syntax tree representing the rule.</returns>
    private Expr UnaryRule()
    {
        if (Match(TokenType.BANG, TokenType.MINUS))
        {
            Token op = Previous();
            Expr right = UnaryRule();
            return new Expr.Unary(op, right);
        }

        return PrimaryRule();
    }

    /// <summary>
    /// PrimaryRule grammer rule.
    /// </summary>
    /// <returns>A syntax tree representing the rule.</returns>
    private Expr PrimaryRule()
    {
        if (Match(TokenType.FALSE)) return new Expr.Literal(false);
        if (Match(TokenType.TRUE)) return new Expr.Literal(true);
        if (Match(TokenType.NIL)) return new Expr.Literal(null);

        if (Match(TokenType.NUMBER, TokenType.STRING))
        {
            return new Expr.Literal(Previous().literal);
        }

        if (Match(TokenType.LEFT_PAREN))
        {
            Expr expr = ExpressionRule();
            Consume(TokenType.RIGHT_PAREN, "Expect ')' after expression.");
            return new Expr.Grouping(expr);
        }

        throw Error(Peek(), "Expect expression.");
    }

    /// <summary>
    /// Common helper method for representing a left associative binary expressions.
    /// </summary>
    /// <param name="leftOperandResolution">Rule for resolving the lhs of the binary expression.</param>
    /// <param name="operatorsToMatch">Binary operators to match against.</param>
    /// <param name="rightOperandResolution">Rule for resolving the rhs of the binary expression.</param>
    /// <returns></returns>
    private Expr CreateLeftAssociativeBinaryExpr(Func<Expr> leftOperandResolution, List<TokenType> operatorsToMatch, Func<Expr> rightOperandResolution)
    {
        // Parse lhs operand.
        Expr expr = leftOperandResolution();

        while (Match(operatorsToMatch.ToArray()))
        {
            // Retrieve != or == operator.
            Token op = Previous();
            // Parse rhs operand.
            Expr right = rightOperandResolution();
            // Create new Binary syntax tree node.
            // By re-assigning to expr we are creating a left associative rule.
            expr = new Expr.Binary(expr, op, right);
        }

        return expr;
    }

    /// <summary>
    /// Synchronizes the Parser after an error based on best effort statement boundaries i.e. ";".
    /// This is not foolproff such as a ";" in a for seperating clauses but is best effort.
    /// </summary>
    private void Synchronize()
    {
        Advance();

        while (!IsAtEnd())
        {
            if (Previous().type == TokenType.SEMICOLON) return;

            switch (Peek().type)
            {
                case TokenType.CLASS:
                case TokenType.FUN:
                case TokenType.VAR:
                case TokenType.FOR:
                case TokenType.IF:
                case TokenType.WHILE:
                case TokenType.PRINT:
                case TokenType.RETURN:
                    return;
            }

            Advance();
        }
    }

    /// <summary>
    /// Matches the current token against to passed token types and if a match is found advances the <see cref="CurrentTokenIndex"/> index.
    /// </summary>
    /// <param name="types"></param>
    /// <returns><see cref="true"/> if match is found (and advances <see cref="CurrentTokenIndex"/>) or <see cref="false"/> if not.</returns>
    private bool Match(params TokenType[] types)
    {
        foreach (TokenType type in types)
        {
            if (Check(type))
            {
                Advance();
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Consumes the current token returns it if it matches the expectedType otherwise throws an error.
    /// </summary>
    /// <param name="expectedType">The type to check against.</param>
    /// <param name="errorMessage">The error to used in the Error if thrown.</param>
    /// <returns>The checked token.</returns>
    private Token Consume(TokenType expectedType, string errorMessage)
    {
        // If correct type advance.
        if (Check(expectedType))
        {
            return Advance();
        }
        // otherwise throw a ParseError.
        throw Error(Peek(), errorMessage);
    }

    /// <summary>
    /// When a syntax error is encountered we return a <see cref="ParseError"/> that can be dealt with by the caller.
    /// </summary>
    /// <param name="token">The current token.</param>
    /// <param name="errorMessage">Associated error message.</param>
    /// <returns>An instance of <see cref="ParseError"/>.</returns>
    private ParseError Error(Token token, string errorMessage)
    {
        Program.Error(token, errorMessage);
        return new ParseError(errorMessage);
    }

    /// <summary>
    /// Checks the current token.
    /// </summary>
    /// <param name="type">Type to check against.</param>
    /// <returns><see cref="true"/> if match is found or <see cref="false"/> if not.</returns>
    private bool Check(TokenType type)
    {
        if (IsAtEnd()) return false;
        return Peek().type == type;
    }

    /// <summary>
    /// Advances the <see cref="CurrentTokenIndex"/>.
    /// </summary>
    /// <returns>The current token before advancing.</returns>
    private Token Advance()
    {
        if (!IsAtEnd())
        {
            CurrentTokenIndex++;
        }

        return Previous();
    }

    /// <summary>
    /// Checks if at end of file/token. I.e. ran out of tokens to consume.
    /// </summary>
    /// <returns><see cref="true"/> if at end of file or <see cref="false"/> if not.</returns>
    private bool IsAtEnd() => Peek().type == TokenType.EOF;

    /// <summary>
    /// Peeks the current token we have yet to consume without advancing the <see cref="CurrentTokenIndex"/>.
    /// </summary>
    /// <returns>The token being peeked.</returns>
    private Token Peek() => Tokens[CurrentTokenIndex];

    /// <summary>
    /// Returns previous token.
    /// </summary>
    /// <returns>The token before the current token pointed to by <see cref="CurrentTokenIndex"/>.</returns>
    private Token Previous() => Tokens[CurrentTokenIndex - 1];
}
