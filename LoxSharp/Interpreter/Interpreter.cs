namespace LoxSharp.Interpreter;

using LoxSharp.AbstractSyntaxTrees;
using LoxSharp.Extensions;
using LoxSharp.Models;
using LoxSharp.NativeFunctions;

/// <summary>
/// The LoxSharp interpreter.
/// </summary>
internal class Interpreter : Expr.IVisitor<object?>, Stmt.IVisitor<object?>
{
    public readonly Environment.Environment Globals;

    // Side table used to store variable resolution information from the VariableResolver pass which is used during interpretation.
    private readonly Dictionary<Expr, int> Locals = new Dictionary<Expr, int>();


    private Environment.Environment Environment;

    public Interpreter()
    {
        this.Globals = new Environment.Environment();
        this.Environment = Globals;

        // Adds to 
        this.Globals.Define(new Token(TokenType.FUN, "Clock", null, 0), new NativeClockFunction());
    }

public void Interpret(List<Stmt> statements)
    {
        try
        {
            foreach (Stmt statement in statements)
            {
                Execute(statement);
            }
        }
        catch (RuntimeErrorException error)
        {
            Program.RuntimeError(error);
        }
    }

    /// <summary>
    /// Execute the given statement.
    /// </summary>
    /// <param name="stmt">Statement to be executed.</param>
    private void Execute(Stmt stmt)
    {
        stmt.Accept(this);
    }

    /// <summary>
    /// Allows the interpreters local side table to be updates with variable resolution information.
    /// </summary>
    /// <param name="expr">The expression to store.</param>
    /// <param name="depth">The depth or hops between the current environment and the environment where the interpreter can find the variables value.</param>
    /// <remarks>Since we are storing expressions we don't need to worry about multiple expressions referencing the same variable as they will be stored seperately.</remarks>
    public void Resolve(Expr expr, int depth)
    {
        Locals.Add(expr, depth);
    }

    /// <summary>
    /// Execute stetements in the context of an a given environment.
    /// </summary>
    /// <param name="statements">Statements to execute.</param>
    /// <param name="environment">Environment from which to execute in.</param>
    public void ExecuteBlock(
        List<Stmt> statements,
        Environment.Environment environment)
    {
        // Keep track of the previos context.
        Environment.Environment previous = this.Environment;
        try
        {
            // Use the passed context.
            this.Environment = environment;

            foreach (Stmt statement in statements)
            {
                Execute(statement);
            }
        }
        finally
        {
            // We restore the environment in a finally block to ensure it is reset even after an exception is thrown.
            this.Environment = previous;
        }
    }

    /// <summary>
    /// Evaluates the given expression by recalling it's own Accept method.
    /// Works due to the visitor pattern implementation.
    /// </summary>
    /// <param name="expr">The expression to be evaluated.</param>
    /// <returns>Result value of evaluation.</returns>
    private object? Evaluate(Expr expr) => expr.Accept(this);

    #region ExpressionVistorMethods

    object? Expr.IVisitor<object?>.VisitBinaryExpr(Expr.Binary expr)
    {
        object? left = Evaluate(expr.left);
        object? right = Evaluate(expr.right);

        switch (expr.op.Type)
        {
            case TokenType.BANG_EQUAL: return !IsEqual(left, right);
            case TokenType.EQUAL_EQUAL: return IsEqual(left, right);
            case TokenType.GREATER:
                CheckNumberOperands(expr.op, left, right);
                return (double)left > (double)right;
            case TokenType.GREATER_EQUAL:
                CheckNumberOperands(expr.op, left, right);
                return (double)left >= (double)right;
            case TokenType.LESS:
                CheckNumberOperands(expr.op, left, right);
                return (double)left < (double)right;
            case TokenType.LESS_EQUAL:
                CheckNumberOperands(expr.op, left, right);
                return (double)left <= (double)right;
            case TokenType.MINUS:
                CheckNumberOperands(expr.op, left, right);
                return (double)left - (double)right;
            case TokenType.PLUS:
                if (left is double && right is double)
                {
                    return (double)left + (double)right;
                }

                if ((left is string && right is string) ||
                    (left is string && right is double) ||
                    (left is double && right is string))
                {
                    return left.ToString() + right.ToString();
                }

                throw new RuntimeErrorException(expr.op, "Operands must be numbers or strings.");
            case TokenType.SLASH:
                CheckNumberOperand(expr.op, left);
                CheckDenominatorIsNoZeroNumberOperand(expr.op, right);
                return (double)left / (double)right;
            case TokenType.STAR:
                CheckNumberOperands(expr.op, left, right);
                return (double)left * (double)right;
        }

        // Unreachable.
        return null;
    }

    public object? VisitCallExpr(Expr.Call expr)
    {
        object? callee = Evaluate(expr.callee);

        List<object?> arguments = new List<object?>();
        foreach (Expr argument in expr.arguments)
        {
            arguments.Add(Evaluate(argument));
        }

        if (callee is not ILoxSharpCallable) {
            throw new RuntimeErrorException(expr.paren, "Can only call functions and classes.");
        }
        else
        {
            ILoxSharpCallable function = callee as ILoxSharpCallable;

            if (arguments.Count != function.Arity())
            {
                throw new RuntimeErrorException(expr.paren, "Expected " + function.Arity() + " arguments but got " + arguments.Count + ".");
            }

            return function.Call(this, arguments);
        }
    }

    object? Expr.IVisitor<object?>.VisitGroupingExpr(Expr.Grouping expr) => Evaluate(expr.expression);

    object? Expr.IVisitor<object?>.VisitLiteralExpr(Expr.Literal expr) => expr.value;

    object? Expr.IVisitor<object?>.VisitLogicalExpr(Expr.Logical expr)
    {
        object? left = Evaluate(expr.left);

        if (expr.op.Type == TokenType.OR) {
            if (IsTruthy(left)) return left;
        } else
        {
            if (!IsTruthy(left)) return left;
        }

        return Evaluate(expr.right);
    }

    object? Expr.IVisitor<object?>.VisitUnaryExpr(Expr.Unary expr)
    {
        object? rhs = Evaluate(expr.right);

        switch (expr.op.Type)
        {
            case TokenType.MINUS:
                CheckNumberOperand(expr.op, rhs);
                return -(double)rhs; // Need to handle the case where this cast fails.
            case TokenType.BANG:
                return !IsTruthy(rhs);
        }

        // Unreachable.
        return null;
    }

    public object? VisitAssignExpr(Expr.Assign expr)
    {
        object? value = Evaluate(expr.value);

        if (Locals.TryGetValue(expr, out int distance))
        {
            // There is implicit trust that the variable resolver has correctly calculated the distance
            // and this is called out in the book. It makes me slightly uneasy in any case if there is a bug 
            Environment.AssignAt(distance, expr.name, value);
        }
        else
        {
            Globals.Assign(expr.name, value);
        }

        return value;
    }

    object? Expr.IVisitor<object?>.VisitVariableExpr(Expr.Variable expr)
    {
        return LookUpVariable(expr.name, expr);
    }

    /// <summary>
    /// Using scope information find the correct value for the the variable.
    /// </summary>
    private object? LookUpVariable(Token name, Expr.Variable expr)
    {
        if (Locals.TryGetValue(expr, out int distance))
        {
            // There is implicit trust that the variable resolver has correctly calculated the distance
            // and this is called out in the book. It makes me slightly uneasy in any case if there is a bug 
            return this.Environment.GetAt(distance, name.Lexeme);
        }
        else
        {
            return Globals.Get(name);
        }
    }

    #endregion

    #region StatementVistorMethods

    object? Stmt.IVisitor<object?>.VisitExpressionStmt(Stmt.Expression stmt)
    {
        Evaluate(stmt.expression);
        return null;
    }

    public object? VisitFunctionStmt(Stmt.Function stmt)
    {
        LoxSharpFunction function = new LoxSharpFunction(stmt, Environment);
        Environment.Define(stmt.name, function);
        return null;
    }

    object? Stmt.IVisitor<object?>.VisitPrintStmt(Stmt.Print stmt)
    {
        object? value = Evaluate(stmt.expression);
        Console.WriteLine(Stringify(value));
        return null;
    }

    object? Stmt.IVisitor<object?>.VisitReturnStmt(Stmt.Return stmt)
    {
        object? value = null;
        if (stmt.value != null)
        {
            value = Evaluate(stmt.value);
        }

        throw new ReturnException(value);
    }

    object? Stmt.IVisitor<object?>.VisitVarStmt(Stmt.Var stmt)
    {
        object? value = stmt.initializer != null ? Evaluate(stmt.initializer) : null;
        Environment.Define(stmt.name, value);
        return null;
    }

    public object? VisitIfStmt(Stmt.If stmt)
    {
        if (IsTruthy(Evaluate(stmt.condition)))
        {
            Execute(stmt.thenBranch);
        }
        else if (stmt.elseBranch != null)
        {
            Execute(stmt.elseBranch);
        }
        return null;
    }

    public object? VisitWhileStmt(Stmt.While stmt)
    {
        while (IsTruthy(Evaluate(stmt.condition)))
        {
            Execute(stmt.body);
        }
        return null;
    }

    public object? VisitBlockStmt(Stmt.Block stmt)
    {
        ExecuteBlock(stmt.statements, new Environment.Environment(Environment));
        return null;
    }

    #endregion

    #region Helpers

    /// <summary>
    /// Determines whether a given object is truthy or falsey.
    /// </summary>
    /// <param name="ob">The object to be tested for truthiness.</param>
    /// <returns>Bool indicating whether the passed object was truthy.</returns>
    private bool IsTruthy(object? ob)
        => ob switch
        {
            null => false,
            bool => (bool)ob,
            _ => true,
        };

    /// <summary>
    /// Determines if two objects are equal.
    /// </summary>
    /// <param name="a">first object</param>
    /// <param name="b">second object</param>
    /// <returns>Bool indicating whether the objects were equal.</returns>
    private bool IsEqual(object? a, object? b)
    {
        if (a == null && b == null)
        {
            return true;
        }
        else if (a == null)
        {
            return false;
        }
        else
        {
            return a.Equals(b);
        }
    }

    /// <summary>
    /// Stringifies the result of interpreteration.
    /// </summary>
    /// <param name="ob">The object to be stringified.</param>
    /// <returns>Stringified result.</returns>
    private string Stringify(object? ob)
    {
        if (ob == null)
        {
            return "nil";
        }
        else if (ob is double)
        {
            string text = ob.ToString()!;
            if (text.EndsWith(".0"))
            {
                text = text.SubstringByIndex(0, text.Length - 2);
            }
            return text;
        }
        else
        {
            return ob.ToString()!;
        }
    }

    /// <summary>
    /// Checks the provided operand is of type double. Throws a <see cref="RuntimeErrorException"/> if not.
    /// </summary>
    private void CheckNumberOperand(Token op, object? operand)
    {
        if (operand is double)
        {
            return;
        }
        else
        {
            throw new RuntimeErrorException(op, "Operand must be a number.");
        }
    }

    /// <summary>
    /// Checks the provided operands are of type double. Throws a <see cref="RuntimeErrorException"/> if not.
    /// </summary>
    private void CheckNumberOperands(Token op, object? left, object? right)
    {
        if (left is double && right is double)
        {
            return;
        }
        else
        {
            throw new RuntimeErrorException(op, "Operands must be numbers.");
        }
    }

    /// <summary>
    /// Checks the provided operand is of type double. Throws a <see cref="RuntimeErrorException"/> if not.
    /// </summary>
    private void CheckDenominatorIsNoZeroNumberOperand(Token op, object? operand)
    {
        if (operand is double && (double)operand != 0)
        {
            return;
        }
        else
        {
            // TODO: Create DevideByZeroError
            throw new RuntimeErrorException(op, "Dominominator must be a non-zero number.");
        }
    }
    #endregion
}
