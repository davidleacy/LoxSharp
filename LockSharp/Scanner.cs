namespace LoxSharp;

internal class Scanner
{
    private readonly string Source;
    private readonly List<Token> Tokens = new List<Token>();
    private int Start = 0;
    private int Current = 0;
    private int Line = 1;

    private readonly Dictionary<string, TokenType> ReservedKeywords = new Dictionary<string, TokenType>
    {
        { "and",    TokenType.AND },
        { "class",  TokenType.CLASS },
        { "else",   TokenType.ELSE },
        { "false",  TokenType.FALSE },
        { "for",    TokenType.FOR },
        { "fun",    TokenType.FUN },
        { "if",     TokenType.IF },
        { "nil",    TokenType.NIL },
        { "or",     TokenType.OR },
        { "print",  TokenType.PRINT },
        { "return", TokenType.RETURN },
        { "super",  TokenType.SUPER },
        { "this",   TokenType.THIS },
        { "true",   TokenType.TRUE },
        { "var",    TokenType.VAR },
        { "while",  TokenType.WHILE },
    };

    public Scanner(string source)
    {
        Source = source;
    }

    /// <summary>
    /// Scans/Lexs source code finding all LoxSharp tokens and returning them.
    /// </summary>
    /// <returns>Scanned and Lexed tokens.</returns>
    public List<Token> ScanTokens()
    {
        while (!IsAtEnd())
        {
            Start = Current;
            ScanToken();
        }

        // When we reach the end of the source code add an EOF identifier for completeness.
        Tokens.Add(new Token(TokenType.EOF, string.Empty, null, Line));
        return Tokens;
    }

    /// <summary>
    /// Scans an individual token.
    /// </summary>
    private void ScanToken()
    {
        char c = Advance();
        switch (c)
        {
            // Single character lexemes
            case '(': AddToken(TokenType.LEFT_PAREN); break;
            case ')': AddToken(TokenType.RIGHT_PAREN); break;
            case '{': AddToken(TokenType.LEFT_BRACE); break;
            case '}': AddToken(TokenType.RIGHT_BRACE); break;
            case ',': AddToken(TokenType.COMMA); break;
            case '.': AddToken(TokenType.DOT); break;
            case '-': AddToken(TokenType.MINUS); break;
            case '+': AddToken(TokenType.PLUS); break;
            case ';': AddToken(TokenType.SEMICOLON); break;
            case '*': AddToken(TokenType.STAR); break;
            // Operators
            case '!':
                AddToken(Match('=') ? TokenType.BANG_EQUAL : TokenType.BANG);
                break;
            case '=':
                AddToken(Match('=') ? TokenType.EQUAL_EQUAL : TokenType.EQUAL);
                break;
            case '<':
                AddToken(Match('=') ? TokenType.LESS_EQUAL : TokenType.LESS);
                break;
            case '>':
                AddToken(Match('=') ? TokenType.GREATER_EQUAL : TokenType.GREATER);
                break;
            // Division and single line comments
            case '/':
                if (Match('/'))
                {
                    // A comment goes until the end of the line.
                    while (Peek() != '\n' && !IsAtEnd()) Advance();
                }
                else
                {
                    AddToken(TokenType.SLASH);
                }
                break;
            // Ignore whitespace
            case ' ':
            case '\r':
            case '\t':
                break;
            // Increment line count on reaching new line.
            case '\n':
                Line++;
                break;
            // Literals
            case '"': StringLiteralScan(); break;
            case '0':
            case '1':
            case '2':
            case '3':
            case '4':
            case '5':
            case '6':
            case '7':
            case '8':
            case '9':
                NumberLiteralScan();
                break;
            // Reserved Words and Identifiers.
            default:
                // This is placed in default as adding a case for each Alpha character is tedious and messy. I.e. this is a hack.
                if (IsAlpha(c))
                {
                    IdentifierAndKeywordScan();
                }
                else
                {
                    LoxSharp.Error(Line, "Unexpected character.");
                }
                break;
        }
    }

    /// <summary>
    /// Performs a string literal scan and adds the token to the token list.
    /// </summary>
    private void StringLiteralScan ()
    {
        // Continue to advance pointer until we reach double quote or EOF.
        while (Peek() != '"' && !IsAtEnd()) {
            // Lox supports multi-line strings so we must update the line counter.
            if (Peek() == '\n') Line++;
            Advance();
        }

        // Reached EOF in the middle of a string which is a syntax error.
        if (IsAtEnd()) {
          LoxSharp.Error(Line, "Unterminated string.");
          return;
        }

        // The closing ".
        Advance();

        // Trim the surrounding quotes.
        string value = Source.SubstringByIndex(Start + 1, Current - 1);
        AddToken(TokenType.STRING, value);
    }

    /// <summary>
    /// Performs a number literal scan and adds the token to the token list.
    /// </summary>
    private void NumberLiteralScan()
    {
        while (Char.IsDigit(Peek())) Advance();

        // Look for a fractional part.
        // This means that 123.sqrt() is allowed, I am not sure I like like so may revist.
        if (Peek() == '.' && Char.IsDigit(PeekNext()))
        {
            // Consume the "."
            Advance();

            while (Char.IsDigit(Peek())) Advance();
        }

        AddToken(TokenType.NUMBER,
            Double.Parse(Source.SubstringByIndex(Start, Current)));
    }

    /// <summary>
    /// Performs a identifier scan and adds the token to the token list.
    /// </summary>
    private void IdentifierAndKeywordScan()
    {
        while (IsAlphaNumeric(Peek())) Advance();

        // After determining the identifier check if it is a reserved keyword and change the type accordingly. 
        string text = Source.SubstringByIndex(Start, Current);
        TokenType type;
        if (!ReservedKeywords.TryGetValue(text, out type))
        {
            type = TokenType.IDENTIFIER;
        }

        AddToken(type);
    }

    private bool IsAlpha(char c)
    {
        return (c >= 'a' && c <= 'z') ||
               (c >= 'A' && c <= 'Z') ||
                c == '_';
    }

    private bool IsAlphaNumeric(char c)
    {
        return IsAlpha(c) || Char.IsDigit(c);
    }

    /// <summary>
    /// Advances <see cref="Current"/> if the character matches the expected character.
    /// </summary>
    /// <param name="expected">The character we are trying to match against.</param>
    /// <returns>A bool representing whether the expected character was matched against and by extension if <see cref="Current"/> was advanced.</returns>
    private bool Match(char expected)
    {
        if (IsAtEnd())
        {
            return false;
        }
        else if (Source[Current] != expected)
        {
            return false;
        }
        else
        {
            Current++;
            return true;
        }
    }

    /// <summary>
    /// Peeks the current character without advancing.
    /// </summary>
    /// <returns>The peeked character.</returns>
    private char Peek()
    {
        // If at end return the null terminating character.
        if (IsAtEnd())
        {
            return '\0';
        }
        return Source[Current];
    }

    /// <summary>
    /// Peeks the next character without advancing.
    /// </summary>
    /// <returns>The peeked character.</returns>
    private char PeekNext()
    {
        // If at end return the null terminating character.
        if (Current + 1 >= Source.Length)
        {
            return '\0';
        }
        return Source[Current + 1];
    }

    /// <summary>
    /// Returns the current character and advances the <see cref="Current"/> pointer by 1.
    /// </summary>
    /// <returns>The current character.</returns>
    private char Advance()
    {
        return Source[Current++];
    }

    /// <summary>
    /// Add a token to the token list.
    /// </summary>
    /// <param name="type"><see cref="TokenType"/></param>
    private void AddToken(TokenType type)
    {
        AddToken(type, null);
    }

    /// <summary>
    /// Adds a token to the token list.
    /// </summary>
    /// <param name="type"><see cref="TokenType"/></param>
    /// <param name="literal">The literal represented by the token.</param>
    private void AddToken(TokenType type, object? literal)
    {
        string text = Source.SubstringByIndex(Start, Current);
        Tokens.Add(new Token(type, text, literal, Line));
    }

    /// <summary>
    /// Determines if we have reached the end of the source code.
    /// </summary>
    /// <returns>Bool representing if we have reached the end of the source code.</returns>
    private bool IsAtEnd() => Current >= Source.Length;
}
