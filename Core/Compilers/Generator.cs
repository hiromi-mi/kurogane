using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using Kurogane.Dynamics;
using System.Dynamic;

namespace Kurogane.Compilers {
	public class Generator {
		// ----- ----- ----- ----- public interface ----- ----- ----- -----
		public static Expression<Func<Scope, object>> Generate(Block block) {
			var gen = new Generator();
			var expr = gen.ConvertBlock(block);
			return Expression.Lambda<Func<Scope, object>>(expr, gen._global);
		}

		// ----- ----- ----- ----- fields ----- ----- ----- -----
		private ParameterExpression _global = Expression.Parameter(typeof(Scope));

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
			var argList = new List<Expression>();
			foreach (var argPair in call.Arguments) {
				argList.Add(ConvertElement(argPair.Argument));
			}
			var info = new CallInfo(0);
			return Expression.Dynamic(new KrgnInvokeBinder(info), typeof(object), func);
		}
		private Expression ConvertElement(Element elem) {
			throw new NotImplementedException();
		}
		private Expression ConvertSymbol(string name) {
			return Expression.Dynamic(KrgnGetMemberBinder.Create(name), typeof(object), _global);
		}

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
