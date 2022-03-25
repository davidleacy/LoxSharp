namespace LoxSharp.Resolvers;

using LoxSharp.AbstractSyntaxTrees;
using LoxSharp.Models;

/// <summary>
/// Implements an additional variable resolution pass to resolve variables after parsing.
/// </summary>
internal class VariableResolver: Expr.IVisitor<object?>, Stmt.IVisitor<object?>
{
    private readonly Interpreter.Interpreter Interpreter;

    // Stack to used to keep track of variables scopes. Each var maps to a bool determining if the var has been initialized yet.
    private readonly Stack<Dictionary<string, bool>> Scopes = new Stack<Dictionary<string, bool>>();

    /// <summary>
    /// Used to keep track of whether we are in a function context and helps determine when RETURN statements are invalid.
    /// </summary>
    private FunctionType CurrentFunction = FunctionType.NONE;


    /// <summary>
    /// Initializes a new instance of the <see cref="VariableResolver"/> class.
    /// </summary>
    /// <param name="interpreter">Instance of the LoxSharp interpreter.</param>
    public VariableResolver(Interpreter.Interpreter interpreter)
    {
        Interpreter = interpreter;
    }

    public void Resolve(List<Stmt> statements)
    {
        foreach (Stmt statement in statements)
        {
            Resolve(statement);
        }
    }

    #region ExpressionVistorMethods

    object? Expr.IVisitor<object?>.VisitAssignExpr(Expr.Assign expr)
    {
        // Resolve the expression first in case there are other variables being used.
        Resolve(expr.value);
        // Then resovle the variable.
        ResolveLocal(expr, expr.name);
        return null;
    }

    object? Expr.IVisitor<object?>.VisitBinaryExpr(Expr.Binary expr)
    {
        Resolve(expr.left);
        Resolve(expr.right);
        return null;
    }

    object? Expr.IVisitor<object?>.VisitCallExpr(Expr.Call expr)
    {
        Resolve(expr.callee);

        foreach (Expr argument in expr.arguments)
        {
            Resolve(argument);
        }

        return null;
    }

    object? Expr.IVisitor<object?>.VisitGroupingExpr(Expr.Grouping expr)
    {
        Resolve(expr.expression);
        return null;
    }
    object? Expr.IVisitor<object?>.VisitLiteralExpr(Expr.Literal expr) => null;

    object? Expr.IVisitor<object?>.VisitLogicalExpr(Expr.Logical expr)
    {
        Resolve(expr.left);
        Resolve(expr.right);
        return null;
    }


    object? Expr.IVisitor<object?>.VisitUnaryExpr(Expr.Unary expr)
    {
        Resolve(expr.right);
        return null;
    }

    object? Expr.IVisitor<object?>.VisitVariableExpr(Expr.Variable expr)
    {
        // If we are trying to access the variable while it is the process of being initialized we report an error.
        if (Scopes.Any() && Scopes.Peek().TryGetValue(expr.name.Lexeme, out bool varInitialized) && varInitialized == false)
        {
            Program.Error(expr.name,
                "Can't read local variable in its own initializer.");
        }

        ResolveLocal(expr, expr.name);
        return null;
    }

    #endregion

    #region StatementVistorMethods

    object? Stmt.IVisitor<object?>.VisitBlockStmt(Stmt.Block stmt)
    {
        BeginScope();
        Resolve(stmt.statements);
        EndScope();
        return null;
    }

    object? Stmt.IVisitor<object?>.VisitExpressionStmt(Stmt.Expression stmt)
    {
        Resolve(stmt.expression);
        return null;
    }

    object? Stmt.IVisitor<object?>.VisitFunctionStmt(Stmt.Function stmt)
    {
        // The function is bound to the surrounding scope.
        Declare(stmt.name);
        Define(stmt.name);

        // The function body and params are bound to it's own scope.
        ResolveFunction(stmt, FunctionType.FUNCTION);
        return null;
    }

    object? Stmt.IVisitor<object?>.VisitIfStmt(Stmt.If stmt)
    {
        // Here we see how we differ from an interpreter as we evaluate all branches regardless of which may be ran.
        Resolve(stmt.condition);
        Resolve(stmt.thenBranch);
        if (stmt.elseBranch != null) Resolve(stmt.elseBranch);
        return null;
    }

    object? Stmt.IVisitor<object?>.VisitPrintStmt(Stmt.Print stmt)
    {
        Resolve(stmt.expression);
        return null;
    }

    object? Stmt.IVisitor<object?>.VisitReturnStmt(Stmt.Return stmt)
    {
        if (CurrentFunction == FunctionType.NONE)
        {
            Program.Error(stmt.keyword, "Can't return from top-level code.");
        }

        if (stmt.value != null)
        {
            Resolve(stmt.value);
        }

        return null;
    }

    object? Stmt.IVisitor<object?>.VisitVarStmt(Stmt.Var stmt)
    {
        Declare(stmt.name);
        if (stmt.initializer != null)
        {
            Resolve(stmt.initializer);
        }
        Define(stmt.name);
        return null;
    }

    object? Stmt.IVisitor<object?>.VisitWhileStmt(Stmt.While stmt)
    {
        // Same as with if the condition and body are resolved once regardless of if the body is executed at runtime.
        Resolve(stmt.condition);
        Resolve(stmt.body);
        return null;
    }
    #endregion

    #region Helpers

    private void Resolve(Stmt stmt)
    {
        stmt.Accept(this);
    }

    private void Resolve(Expr expr)
    {
        expr.Accept(this);
    }

    /// <summary>
    /// Creates a new variable scope.
    /// </summary>
    private void BeginScope()
    {
        Scopes.Push(new Dictionary<string, bool>());
    }

    /// <summary>
    /// Removes a variable scope.
    /// </summary>
    private void EndScope()
    {
        Scopes.Pop();
    }

    /// <summary>
    /// Helper to declare a variable.
    /// </summary>
    /// <param name="name">Variable to declare.</param>
    private void Declare(Token name)
    {
        if (!Scopes.Any())
        {
            return;
        }

        Dictionary<string, bool> scope = Scopes.Peek();

        // Additional checking to ensure variables are not redeclared in the same scope since this is almost certainly a mistake.
        if (scope.ContainsKey(name.Lexeme))
        {
            Program.Error(name, "Already a variable with this name in this scope.");
        }

        scope.Add(name.Lexeme, false);
    }

    /// <summary>
    /// Helper to define a variable.
    /// </summary>
    /// <param name="name">Variable to define.</param>
    private void Define(Token name)
    {
        if (!Scopes.Any())
        {
            return;
        }

        Scopes
            .Peek()[name.Lexeme] = true;
    }

    /// <summary>
    /// Similiar to <see cref="Environment"/> this helper resolves a local variable in the scope stack.
    /// </summary>
    /// <param name="expr">Expression to be resolved.</param>
    /// <param name="name">Token to be resolved.</param>
    private void ResolveLocal(Expr expr, Token name)
    {
        // Starting from the top of the stack work our way down looking for the variable.
        // N.b. c# places new items at index zero rather than in Java where it is at the end.
        for (int i = 0; i < Scopes.Count; i++)
        {
            if (Scopes.ElementAt(i).ContainsKey(name.Lexeme))
            {
                Interpreter.Resolve(expr, Scopes.Count - 1 - i);
                return;
            }
        }
    }

    private void ResolveFunction(Stmt.Function function, FunctionType type)
    {
        // Use the internal stack to keep a track of when we are in a function.
        FunctionType enclosingFunction = CurrentFunction;
        CurrentFunction = type;

        BeginScope();
        foreach (Token param in function.parameters)
        {
            Declare(param);
            Define(param);
        }
        Resolve(function.body);
        EndScope();

        // Restore to previos value.
        CurrentFunction = enclosingFunction;
    }

    #endregion
}