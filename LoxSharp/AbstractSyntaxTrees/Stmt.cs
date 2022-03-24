﻿// <auto-generated />
#nullable enable

namespace LoxSharp.AbstractSyntaxTrees;

using LoxSharp.Models;

internal abstract class Stmt {
    public interface IVisitor<T> {
        public T VisitBlockStmt(Block stmt);
        public T VisitExpressionStmt(Expression stmt);
        public T VisitPrintStmt(Print stmt);
        public T VisitVarStmt(Var stmt);
    }

    public class Block: Stmt {
        public readonly List<Stmt> statements;

        public Block(List<Stmt> statements) {
            this.statements = statements;
        }

        public override T Accept<T>(IVisitor<T> visitor) {
            return visitor.VisitBlockStmt(this);
        }
    }

    public class Expression: Stmt {
        public readonly Expr expression;

        public Expression(Expr expression) {
            this.expression = expression;
        }

        public override T Accept<T>(IVisitor<T> visitor) {
            return visitor.VisitExpressionStmt(this);
        }
    }

    public class Print: Stmt {
        public readonly Expr expression;

        public Print(Expr expression) {
            this.expression = expression;
        }

        public override T Accept<T>(IVisitor<T> visitor) {
            return visitor.VisitPrintStmt(this);
        }
    }

    public class Var: Stmt {
        public readonly Token name;
        public readonly Expr? initializer;

        public Var(Token name, Expr? initializer) {
            this.name = name;
            this.initializer = initializer;
        }

        public override T Accept<T>(IVisitor<T> visitor) {
            return visitor.VisitVarStmt(this);
        }
    }

    public abstract T Accept<T>(IVisitor<T> visitor);
}
