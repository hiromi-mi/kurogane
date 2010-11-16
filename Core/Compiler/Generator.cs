using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;
using Kurogane.RuntimeBinder;
using Kurogane.Util;

namespace Kurogane.Compiler {

	public class Generator {

		// ----- ----- ----- ----- public interface ----- ----- ----- -----
		public static Expression<Func<Scope, object>> Generate(Block block, BinderFactory factory, string fileName) {
			var gen = new Generator(factory);
			if (fileName != null)
				gen.SymbolDocumentInfo = Expression.SymbolDocument(fileName);
			var expr = gen.ConvertBlock(block);
			return Expression.Lambda<Func<Scope, object>>(expr, gen.Global);
		}

		// ----- ----- ----- ----- fields ----- ----- ----- -----
		private readonly BinderFactory _factory;
		private LabelTarget _ReturnTarget = null;

		public ParameterExpression Global { get; set; }
		public virtual SymbolDocumentInfo SymbolDocumentInfo { get; private set; }

		// ----- ----- ----- ----- ctor ----- ----- ----- -----
		protected Generator(BinderFactory factory) {
			_factory = factory;
			this.Global = Expression.Parameter(typeof(Scope), "global");
		}

		// ----- ----- ----- ----- methods ----- ----- ----- -----
		public virtual Expression ConvertBlock(Block block) {
			var list = new List<Expression>();
			foreach (var stmt in block.Statements) {
				list.Add(ConvertStatement(stmt));
			}
			var blockExpr =
				list.Count == 0
				? Expression.Constant(null)
				: Expression.Block(list) as Expression;
			if (_ReturnTarget == null)
				return blockExpr;
			return Expression.Label(_ReturnTarget, blockExpr);
		}

		private Expression ConvertStatement(IStatement stmt) {
			if (stmt is IfStatement)
				return ConvertIf((IfStatement)stmt);
			if (stmt is PhraseChain)
				return ConvertPhraseChain((PhraseChain)stmt);
			if (stmt is Defun)
				return ConvertDefun((Defun)stmt);
			if (stmt is BlockExecute)
				return ConvertBlockExecute((BlockExecute)stmt);
			if (stmt is Return)
				return ConvertReturn((Return)stmt);
			throw new NotImplementedException(stmt.GetType().Name);
		}

		private Expression ConvertBlockExecute(BlockExecute block) {
			var gen = new BlockGenerator(_factory, this, null);
			return gen.ConvertBlock(block.Block);
		}

		private Expression ConvertIf(IfStatement ifStatement) {
			var thens = ifStatement.Thens;
			var ix = thens.Count - 1;
			var lastThen = thens[ix];
			Expression lastExpr;
			if (lastThen.Condition is BoolLiteral && ((BoolLiteral)lastThen.Condition).Value) {
				lastExpr = ConvertStatement(lastThen.Statement);
				ix--;
			}
			else {
				lastExpr = Expression.Constant(null);
			}
			for (; ix >= 0; ix--) {
				var then = thens[ix];
				var execExpr = ConvertStatement(then.Statement);
				if (execExpr.Type != typeof(object))
					execExpr = Expression.Convert(execExpr, typeof(object));
				lastExpr = Expression.Condition(ToBool(ConvertElement(then.Condition)), execExpr, lastExpr);
			}
			return lastExpr;
		}

		private Expression ToBool(Expression expr) {
			return Expression.Dynamic(_factory.ToBoolBinder, typeof(bool), expr);
		}

		public virtual Expression ConvertDefun(Defun defun) {
			var funcType = ExpressionUtil.GetFuncType(defun.Params.Count);
			var sfxFuncType = typeof(SuffixFunc<>).MakeGenericType(funcType);
			var funcVarExpr = Expression.Parameter(sfxFuncType, defun.Name);
			var defunExpr = ConvertDefunCore(defun, funcVarExpr);
			return SetGlobal(defun.Name, Expression.Block(new[] { funcVarExpr }, defunExpr));
		}

		public virtual Expression ConvertDefunCore(Defun defun, ParameterExpression funcExpr) {
			var funcType = ExpressionUtil.GetFuncType(defun.Params.Count);
			var sfxFuncType = typeof(SuffixFunc<>).MakeGenericType(funcType);
			var dic = new Dictionary<string, ParameterExpression>();
			dic[defun.Name] = funcExpr;
			foreach (var p in defun.Params)
				dic[p.Name] = Expression.Parameter(typeof(object), p.Name);
			var gen = new BlockGenerator(_factory, this, dic);
			var block = gen.ConvertBlock(defun.Block);
			dic.Remove(defun.Name);
			var lambdaExpr = Expression.Lambda(block, dic.Values);
			var ctorInfo = sfxFuncType.GetConstructor(new[] { funcType, typeof(string[]) });
			var suffixList = new string[defun.Params.Count];
			for (int i = 0; i < suffixList.Length; i++)
				suffixList[i] = defun.Params[i].Suffix;
			var createExpr = Expression.New(ctorInfo, lambdaExpr, Expression.Constant(suffixList));
			return Expression.Assign(funcExpr, createExpr);
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
			if (ph is DefineValue)
				return ConvertDefineValue((DefineValue)ph, ref lastExpr);
			throw new NotImplementedException();
		}

		public virtual Expression ConvertDefineValue(DefineValue defineValue, ref Expression lastExpr) {
			return ConvertAssign(new Assign(defineValue.Name, defineValue.Value), ref lastExpr);
		}

		private GotoExpression ConvertReturn(Return ret) {
			var value = ConvertElement(ret.Value);
			var target = GetReturnTarget();
			if (value.Type != typeof(object))
				value = Expression.Convert(value, typeof(object));
			return Expression.Return(target, value, typeof(object));
		}

		private LabelTarget GetReturnTarget() {
			return _ReturnTarget ?? (_ReturnTarget = Expression.Label(typeof(object), "#return_label"));
		}

		private Expression ConvertCall(Call call, ref Expression lastExpr) {
			if (call is MapCall)
				return ConvertMapCall((MapCall)call, ref lastExpr);
			var func = ConvertSymbol(call.Name);
			var argList = new List<Expression>(call.Arguments.Count + 1);
			argList.Add(func);
			bool hasOptional = false;
			if (lastExpr != null) {
				argList.Add(lastExpr);
				lastExpr = null;
				hasOptional = true;
			}
			var sfxList = new List<string>(call.Arguments.Count);
			foreach (var argPair in call.Arguments) {
				argList.Add(ConvertElement(argPair.Argument));
				sfxList.Add(argPair.Suffix);
			}
			var nArg = call.Arguments.Count + (hasOptional ? 1 : 0);
			var callInfo = new CallInfo(nArg, sfxList);
			if (func is ParameterExpression)
				return Expression.Dynamic(_factory.InvokeBinder(callInfo), typeof(object), argList);
			argList[0] = Global;
			return Expression.Dynamic(_factory.InvokeMemberBinder(call.Name, callInfo), typeof(object), argList);
		}

		private Expression ConvertMapCall(MapCall call, ref Expression lastExpr) {
			Expression listExpr = null;
			// Mapのリストを探す
			if (call.FirstArg != null) {
				listExpr = ConvertElement(call.FirstArg.Argument);
			}
			else if (lastExpr != null) {
				listExpr = lastExpr;
				lastExpr = null;
			}
			else {
				throw new SemanticException("「それぞれ」の対象がありません。");
			}
			// map用の関数を作る。
			Expression mapFunc = null;
			var param = Expression.Parameter(typeof(object), "それぞれ");
			{
				var func = ConvertSymbol(call.Name);
				// 引数
				var argList = new List<Expression>(call.Arguments.Count + 1);
				argList.Add(func);
				argList.Add(param);
				foreach (var pair in call.Arguments)
					argList.Add(ConvertElement(pair.Argument));
				// 助詞
				int argCount = call.Arguments.Count + 1;
				int offset = call.FirstArg == null ? 0 : 1;
				string[] sfxList = new string[call.Arguments.Count + offset];
				if (offset == 1)
					sfxList[0] = call.FirstArg.Suffix;
				for (int i = 0; i < call.Arguments.Count; i++)
					sfxList[i + offset] = call.Arguments[i].Suffix;
				var callInfo = new CallInfo(argCount, sfxList);
				// 関数
				if (func is ParameterExpression) {
					mapFunc = Expression.Dynamic(_factory.InvokeBinder(callInfo), typeof(object), argList);
				}
				else {
					argList[0] = Global;
					mapFunc = Expression.Dynamic(_factory.InvokeMemberBinder(call.Name, callInfo), typeof(object), argList);
				}
			}
			// 返す。
			var lambda = Expression.Lambda(mapFunc, param);
			return Expression.Dynamic(_factory.MapBinder, typeof(object), lambda, listExpr);
		}

		private Expression ConvertPropertySet(PropertySet propertySet, ref Expression lastExpr) {
			var elemExpr = ConvertElement(propertySet.Property.Value);
			var valueExpr = ConvertElement(propertySet.Value);
			var binder = _factory.SetMemberBinder(propertySet.Property.Name);
			return Expression.Dynamic(binder, typeof(object), elemExpr, valueExpr);
		}

		private Expression ConvertAssign(Assign assign, ref Expression lastExpr) {
			if (assign.Value != null)
				return SetGlobal(assign.Name, ConvertElement(assign.Value));
			Expression valueExpr = lastExpr;
			lastExpr = null;
			return SetGlobal(assign.Name, valueExpr);
		}

		public Expression SetGlobal(string name, Expression value) {
			var binder = _factory.SetMemberBinder(name);
			return Expression.Dynamic(binder, typeof(object), Global, value);
		}

		public Expression GetGlobal(string name) {
			var binder = _factory.GetMemberBinder(name);
			return Expression.Dynamic(binder, typeof(object), Global);
		}

		#region ConvertElement

		protected Expression ConvertElement(Element elem) {
			if (elem is Symbol)
				return ConvertSymbol(((Symbol)elem).Name);
			if (elem is Literal)
				return ConvertLiteral((Literal)elem);
			if (elem is BinaryExpr)
				return ConvertBinaryExpr((BinaryExpr)elem);
			if (elem is PropertyAccess)
				return ConvertPropertyGet((PropertyAccess)elem);
			if (elem is Lambda)
				return ConvertLambda((Lambda)elem);
			throw new NotImplementedException();
		}

		private Expression ConvertPropertyGet(PropertyAccess propertyAccess) {
			return Expression.Dynamic(
				_factory.GetMemberBinder(propertyAccess.Name),
				typeof(object),
				ConvertElement(propertyAccess.Value));
		}

		protected virtual Expression ConvertLambda(Lambda lambda) {
			var gen = new LambdaGenerator(_factory, this);
			return gen.ConvertLambdaCore(lambda);
		}

		public virtual Expression ConvertSymbol(string name) {
			return Expression.Dynamic(_factory.GetMemberBinder(name), typeof(object), Global);
		}

		protected virtual Expression ConvertLiteral(Element lit) {
			if (lit is StringLiteral)
				return Expression.Constant(((StringLiteral)lit).Value, typeof(string));
			if (lit is IntLiteral)
				return Expression.Constant(((IntLiteral)lit).Value, typeof(int));
			if (lit is FloatLiteral)
				return Expression.Constant(((FloatLiteral)lit).Value, typeof(double));
			if (lit is TupleLiteral)
				return ConvertTuple((TupleLiteral)lit);
			if (lit is ListLiteral)
				return ConvertList((ListLiteral)lit);
			if (lit is NullLiteral)
				return Expression.Constant(null);
			if (lit is BoolLiteral)
				return Expression.Constant(((BoolLiteral)lit).Value, typeof(bool));
			throw new NotImplementedException();
		}

		private Expression ConvertList(ListLiteral listLiteral) {
			var memInfo = typeof(ListCell).GetMethod("Cons", new[] { typeof(object), typeof(object) });
			Expression last = Expression.Constant(null);
			var elems = listLiteral.Elements;
			for (int i = elems.Count - 1; i >= 0; i--) {
				var head = Expression.Convert(ConvertElement(elems[i]), typeof(object));
				last = Expression.Call(memInfo, head, last);
			}
			return last;
		}

		private Expression ConvertTuple(TupleLiteral tuple) {
			var head = ConvertElement(tuple.Head);
			var tail = ConvertElement(tuple.Tail);
			var memInfo = typeof(ListCell).GetMethod("Cons", new[] { typeof(object), typeof(object) });
			return Expression.Call(memInfo,
				Expression.Convert(head, typeof(object)),
				Expression.Convert(tail, typeof(object)));
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
