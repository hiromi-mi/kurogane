using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;
using System.Diagnostics;

namespace Kurogane.Compiler {
	public class Generator {
		// ----- ----- ----- ----- public interface ----- ----- ----- -----
		public static Expression<Func<Scope, object>> Generate(Block block, BinderFactory factory) {
			var gen = new Generator(factory);
			var expr = gen.ConvertBlock(block);
			return Expression.Lambda<Func<Scope, object>>(expr, gen._global);
		}

		// ----- ----- ----- ----- fields ----- ----- ----- -----
		private ParameterExpression _global = Expression.Parameter(typeof(Scope));
		private BinderFactory _factory;

		// ----- ----- ----- ----- methods ----- ----- ----- -----
		private Generator(BinderFactory factory) {
			Debug.Assert(factory != null, "factory is null");
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
			throw new NotImplementedException();
		}

		private Expression ConvertSymbol(string name) {
			return Expression.Dynamic(_factory.GetMemberBinder(name), typeof(object), _global);
		}

		private Expression ConvertLiteral(Element lit) {
			if (lit is StringLiteral)
				return Expression.Constant(((StringLiteral)lit).Value);
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
