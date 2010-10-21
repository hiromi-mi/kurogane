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
		private readonly ParameterExpression _global = Expression.Parameter(typeof(Scope), "global");
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
			Expression lastExpr = null;
			foreach (var ph in chain.Phrases) {
				var expr = ConvertPhrase(ph, ref lastExpr);
				if (lastExpr != null)
					list.Add(lastExpr);
				lastExpr = expr;
			}
			if (lastExpr != null)
				list.Add(lastExpr);
			if (list.Count == 1)
				return list[0];
			return Expression.Block(list);
		}

		private Expression ConvertPhrase(IPhrase ph, ref Expression lastExpr) {
			if (ph is Call)
				return ConvertCall((Call)ph, ref lastExpr);
			if (ph is PropertySet)
				return ConvertPropertySet((PropertySet)ph, ref lastExpr);
			if (ph is Assign)
				return ConvertAssign((Assign)ph, ref lastExpr);
			throw new NotImplementedException();
		}

		private Expression ConvertCall(Call call, ref Expression lastExpr) {
			var func = ConvertSymbol(call.Name);
			var argList = new List<Expression>(call.Arguments.Count + 1);
			argList.Add(func);
			var sfxList = new List<string>(call.Arguments.Count);
			foreach (var argPair in call.Arguments) {
				argList.Add(ConvertElement(argPair.Argument));
				sfxList.Add(argPair.Suffix);
			}
			var nArg = call.Arguments.Count;
			var callInfo = new CallInfo(nArg, sfxList);
			if (func is ParameterExpression)
				return Expression.Dynamic(_factory.InvokeBinder(callInfo), typeof(object), argList);
			argList[0] = _global;
			return Expression.Dynamic(_factory.InvokeMemberBinder(call.Name, callInfo), typeof(object), argList);
		}

		private Expression ConvertPropertySet(PropertySet propertySet, ref Expression lastExpr) {
			var elemExpr = ConvertElement(propertySet.Property.Value);
			var valueExpr = ConvertElement(propertySet.Value);
			var binder = _factory.SetMemberBinder(propertySet.Property.Name);
			return Expression.Dynamic(binder, typeof(object), elemExpr, valueExpr);
		}

		private Expression ConvertAssign(Assign assign, ref Expression lastExpr) {
			Expression valueExpr;
			if (assign.Value == null) {
				valueExpr = lastExpr;
				lastExpr = null;
			}
			else {
				valueExpr = ConvertElement(assign.Value);
			}
			var binder = _factory.SetMemberBinder(assign.Name);
			return Expression.Dynamic(binder, typeof(object), _global, valueExpr);
		}

		#region ConvertElement

		private Expression ConvertElement(Element elem) {
			if (elem is Symbol)
				return ConvertSymbol(((Symbol)elem).Name);
			if (elem is Literal)
				return ConvertLiteral((Literal)elem);
			if (elem is BinaryExpr)
				return ConvertBinaryExpr((BinaryExpr)elem);
			if (elem is PropertyAccess)
				return ConvertPropertyGet((PropertyAccess)elem);
			throw new NotImplementedException();
		}

		private Expression ConvertPropertyGet(PropertyAccess propertyAccess) {
			return Expression.Dynamic(
				_factory.GetMemberBinder(propertyAccess.Name),
				typeof(object),
				ConvertElement(propertyAccess.Value));
		}

		private Expression ConvertSymbol(string name) {
			return Expression.Dynamic(_factory.GetMemberBinder(name), typeof(object), _global);
		}

		private Expression ConvertLiteral(Element lit) {
			if (lit is StringLiteral)
				return Expression.Constant(((StringLiteral)lit).Value);
			if (lit is IntLiteral)
				return Expression.Constant(((IntLiteral)lit).Value);
			if (lit is TupleLiteral)
				return ConvertTuple((TupleLiteral)lit);
			throw new NotImplementedException();
		}

		private Expression ConvertTuple(TupleLiteral tuple) {
			var head = ConvertElement(tuple.Head);
			var tail = ConvertElement(tuple.Tail);
			var ctorInfo = typeof(Tuple<object, object>).GetConstructor(new[] { typeof(object), typeof(object) });
			return Expression.New(ctorInfo, head, tail);
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
