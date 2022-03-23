namespace LoxSharp.Interpreter;

using LoxSharp.AbstractSyntaxTrees;
using LoxSharp.Extensions;
using LoxSharp.Models;

/// <summary>
/// The LoxSharp interpreter.
/// </summary>
internal class Interpreter : Expr.IVisitor<object?>
{
    public void Interpret(Expr expression)
    {
        try
        {
            object? value = Evaluate(expression);
            Console.WriteLine(Stringify(value));
        }
        catch (RuntimeErrorException error)
        {
            Program.RuntimeError(error);
        }
    }

    object? Expr.IVisitor<object?>.VisitBinaryExpr(Expr.Binary expr)
    {
        object? left = Evaluate(expr.left);
        object? right = Evaluate(expr.right);

        switch (expr.op.type)
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

    object? Expr.IVisitor<object?>.VisitGroupingExpr(Expr.Grouping expr) => Evaluate(expr.expression);

    object? Expr.IVisitor<object?>.VisitLiteralExpr(Expr.Literal expr) => expr.value;

    object? Expr.IVisitor<object?>.VisitUnaryExpr(Expr.Unary expr)
    {
        object? rhs = Evaluate(expr.right);

        switch (expr.op.type)
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

    /// <summary>
    /// Evaluates the given expression by recalling it's own Accept method.
    /// Works due to the visitor pattern implementation.
    /// </summary>
    /// <param name="expr">The expression to be evaluated.</param>
    /// <returns>Result value of evaluation.</returns>
    private object? Evaluate(Expr expr) => expr.Accept(this);

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
            throw new RuntimeErrorException(op, "Dominominator must be a non-zero number.");
        }
    }
}
