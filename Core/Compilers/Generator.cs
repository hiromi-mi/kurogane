using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;
using System.Diagnostics;
using Kurogane.RuntimeBinder;

namespace Kurogane.Compiler {
	public class Generator {
		// ----- ----- ----- ----- public interface ----- ----- ----- -----
		public static Expression<Func<Scope, object>> Generate(Block block, BinderFactory factory) {
			var gen = new Generator(factory);
			var expr = gen.ConvertBlock(block);
			return Expression.Lambda<Func<Scope, object>>(expr, gen._global);
		}

		// ----- ----- ----- ----- fields ----- ----- ----- -----
		private readonly ParameterExpression _global = Expression.Parameter(typeof(Scope));
		private readonly BinderFactory _factory;

		// ----- ----- ----- ----- methods ----- ----- ----- -----
		private Generator(BinderFactory factory) {
			_factory = factory;
		}

		// ----- ----- ----- ----- methods ----- ----- ----- -----
		private Expression ConvertBlock(Block block) {
			var list = new List<Expression>();
			foreach (var stmt in block.Statements) {
				list.Add(ConvertStatement(stmt));
			}
			return Expression.Block(list);
		}

		private Expression ConvertStatement(IStatement stmt) {
			if (stmt is IfStatement)
				return ConvertIf((IfStatement)stmt);
			if (stmt is PhraseChain)
				return ConvertPhraseChain((PhraseChain)stmt);
			throw new NotImplementedException(stmt.GetType().Name);
		}

		private Expression ConvertIf(IfStatement ifStatement) {
			throw new NotImplementedException();
		}

		private Expression ConvertPhraseChain(PhraseChain chain) {
			var list = new List<Expression>();
			foreach (var ph in chain.Phrases) {
				list.Add(ConvertPhrase(ph));
			}
			return Expression.Block(list);
		}

		private Expression ConvertPhrase(IPhrase ph) {
			if (ph is Call)
				return ConvertCall((Call)ph);

			throw new NotImplementedException();
		}

		private Expression ConvertCall(Call call) {
			var func = ConvertSymbol(call.Name);
			var argList = new List<Expression>(call.Arguments.Count + 1);
			argList.Add(func);
			var sfxList = new List<string>(call.Arguments.Count);
			foreach (var argPair in call.Arguments) {
				argList.Add(ConvertElement(argPair.Argument));
				sfxList.Add(argPair.Suffix);
			}
			var nArg = call.Arguments.Count;
			var info = new CallInfo(nArg, sfxList);
			return Expression.Dynamic(_factory.InvokeBinder(info), typeof(object), argList);
		}

		#region ConvertElement

		private Expression ConvertElement(Element elem) {
			if (elem is Symbol)
				return ConvertSymbol(((Symbol)elem).Name);
			if (elem is Literal)
				return ConvertLiteral((Literal)elem);
			if (elem is BinaryExpr)
				return ConvertBinaryExpr((BinaryExpr)elem);
			throw new NotImplementedException();
		}

		private Expression ConvertSymbol(string name) {
			return Expression.Dynamic(_factory.GetMemberBinder(name), typeof(object), _global);
		}

		private Expression ConvertLiteral(Element lit) {
			if (lit is StringLiteral)
				return Expression.Constant(((StringLiteral)lit).Value);
			if (lit is IntLiteral)
				return Expression.Constant(((IntLiteral)lit).Value);
			throw new NotImplementedException();
		}

		private Expression ConvertBinaryExpr(BinaryExpr expr) {
			var binder = FindBinder(expr.Type);
			var left = ConvertElement(expr.Left);
			var right = ConvertElement(expr.Right);
			if (binder == null)
				throw new NotImplementedException();
			return Expression.Dynamic(binder, typeof(object), left, right);
		}

		private DynamicMetaObjectBinder FindBinder(BinaryOperationType type) {
			switch (type) {
			case BinaryOperationType.Add:
				return _factory.AddBinder;
			case BinaryOperationType.Subtract:
				return _factory.SubBinder;
			case BinaryOperationType.Multiply:
				return _factory.MultBinder;
			case BinaryOperationType.Divide:
				return _factory.DivideBinder;
			case BinaryOperationType.Modulo:
				return _factory.ModBinder;

			case BinaryOperationType.LessThan:
				return _factory.LessThanBinder;
			case BinaryOperationType.LessThanOrEqual:
				return _factory.LessThanOrEqualBinder;
			case BinaryOperationType.GreaterThan:
				return _factory.GreaterThanBinder;
			case BinaryOperationType.GreaterThanOrEqual:
				return _factory.GreaterThanOrEqualBinder;

			case BinaryOperationType.Equal:
				return _factory.EqualBinder;
			case BinaryOperationType.NotEqual:
				return _factory.NotEqualBinder;

			case BinaryOperationType.And:
				return _factory.AndBinder;
			case BinaryOperationType.Or:
				return _factory.OrBinder;
			}
			throw new NotImplementedException();
		}

		#endregion

		#region Util

		private ExprVarsPair<T> MakePair<T>(Expression expr, IList<ParameterExpression> vars) where T : Expression {
			return new ExprVarsPair<T>(expr, vars);
		}
		private struct ExprVarsPair<T> where T : Expression {
			public readonly Expression Expr;
			public readonly IList<ParameterExpression> Vars;

			public ExprVarsPair(Expression expr, IList<ParameterExpression> vars) {
				this.Expr = expr;
				this.Vars = vars;
			}
		}

		#endregion
	}
}
