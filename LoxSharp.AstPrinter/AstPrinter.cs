namespace LoxSharp.AstPrinter;

using LoxSharp.AstPrinter.AbstractSyntaxTrees;
using LoxSharp.Models;
using System.Text;

public class AstPrinter
{
    /// <summary>
    /// Main entry point to the LoxSharp AstPrinter program which is used pretty print syntax trees.
    /// For the purposes of the book this is only used in chapter 5 so may I may not keep it up to date.
    /// </summary>
    /// <param name="args">Unused.</param>
    public static void Main(string[] args)
    {
        Expr expression = new Expr.Binary(
        new Expr.Unary(
            new Token(TokenType.MINUS, "-", null, 1),
            new Expr.Literal(123)),
        new Token(TokenType.STAR, "*", null, 1),
        new Expr.Grouping(
            new Expr.Literal(45.67)));

        Console.WriteLine(new AstPrinterInternal().Print(expression));
    }

    /// <summary>
    /// Internal AstPrinterInternal which implements the <see cref="Expr.IVisitor{string}"/> interface.
    /// </summary>
    internal class AstPrinterInternal : Expr.IVisitor<string>
    {
        public string Print(Expr expr)
        {
            return expr.Accept(this);
        }

        public string VisitBinaryExpr(Expr.Binary expr)
        {
            return Parenthesize(expr.op.lexeme,
                                expr.left, expr.right);
        }

        public string VisitGroupingExpr(Expr.Grouping expr)
        {
            return Parenthesize("group", expr.expression);
        }

        public string VisitLiteralExpr(Expr.Literal expr)
        {
            if (expr.value == null)
            {
                return "nil";
            }
            else
            {
                return expr.value.ToString()!;
            }
        }

        public string VisitUnaryExpr(Expr.Unary expr)
        {
            return Parenthesize(expr.op.lexeme, expr.right);
        }

        private string Parenthesize(string name, params Expr[] exprs)
        {
            StringBuilder builder = new StringBuilder();

            builder.Append('(').Append(name);
            foreach (Expr expr in exprs)
            {
                builder.Append(' ');
                builder.Append(expr.Accept(this));
            }
            builder.Append(')');

            return builder.ToString();
        }
    }
}