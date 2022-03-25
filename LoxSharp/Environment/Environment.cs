namespace LoxSharp.Environment;

using LoxSharp.Interpreter;
using LoxSharp.Models;

internal class Environment
{
    private readonly Environment? Enclosing;

    private readonly Dictionary<string, object?> values = new Dictionary<string, object?>();

    /// <summary>
    /// Initializes a new instance of the <see cref="Environment"/> class.
    /// </summary>
    public Environment()
    {
        Enclosing = null;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Environment"/> class with the provided enclosing scope.
    /// </summary>
    /// <param name="enclosing">The enclosing environment/scope.</param>
    public Environment(Environment enclosing)
    {
        this.Enclosing = enclosing;
    }

    public void Define(Token name, object? value)
    {
        try
        {
            values.Add(name.Lexeme, value);
        }
        catch (ArgumentException)
        {
            // I differ from the book here in that I think this should be an error
            throw new RuntimeErrorException(name,
                "Attempted to redeclare variable '" + name.Lexeme + "'.");
        }
    }

    public object? Get(Token name)
    {
        if (values.ContainsKey(name.Lexeme))
        {
            return values[name.Lexeme];
        }

        // If not found in current scope walk up the scope chain.
        if (Enclosing != null)
        {
            return Enclosing.Get(name);
        }

        throw new RuntimeErrorException(name,
            "Undefined variable '" + name.Lexeme + "'.");
    }

    public object? GetAt(int distance, string name)
    {
        return Ancestor(distance).values[name];
    }

    public void Assign(Token name, object? value)
    {
        if (values.ContainsKey(name.Lexeme))
        {
            values[name.Lexeme] = value;
            return;
        }

        // If not found in current scope walk up the scope chain.
        if (Enclosing != null)
        {
            Enclosing.Assign(name, value);
            return;
        }

        throw new RuntimeErrorException(name,
            "Undefined variable '" + name.Lexeme + "'.");
    }

    public void AssignAt(int distance, Token name, object? value) => Ancestor(distance).values.Add(name.Lexeme, value);

    private Environment Ancestor(int distance)
    {
        Environment environment = this;
        for (int i = 0; i < distance; i++)
        {
            environment = environment.Enclosing ?? throw new RuntimeErrorException(null, "Unexpected failure during scope resolution."); ;
        }

        return environment;
    }
}
