﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using Kurogane.Runtime;
using System.Dynamic;
using System.Diagnostics;
using Kurogane.Compiler.Binders;
using Kurogane.Buildin;

namespace Kurogane.Compiler {

	public class LexicalScope {

		public readonly LexicalScope Parent;
		private Dictionary<string, ParameterExpression> _dic = new Dictionary<string, ParameterExpression>();

		public LexicalScope(LexicalScope parent) {
			Parent = parent;
		}

		/// <summary>
		/// 現在のスコープに変数を作成する。
		/// </summary>
		public ParameterExpression CreateVariable(string name) {
			if (_dic.ContainsKey(name)) throw new SemanticException("同じ変数がすでに定義されています。");
			var expr = Expression.Parameter(typeof(object), name);
			_dic[name] = expr;
			return expr;
		}

		/// <summary>
		/// 変数名からもっとも直近の宣言された変数を取得する。
		/// 存在しない場合、nullを返す。
		/// </summary>
		public ParameterExpression GetVariable(string name) {
			for (var t = this; t != null; t = t.Parent)
				if (t._dic.ContainsKey(name))
					return t._dic[name];
			return null;
		}

		public void AddRange(ParameterExpression[] exprs) {
			foreach (var expr in exprs)
				_dic[expr.Name] = expr;
		}

		public ICollection<ParameterExpression> Variables {
			get { return _dic.Values; }
		}

		public int NestSize {
			get { return Parent == null ? 1 : Parent.NestSize + 1; }
		}
	}

	/// <summary>
	/// AstをExpressionに変換するためのクラス
	/// </summary>
	public class ExpressionGenerator {

		// ----- ----- ----- ----- ----- static ----- ----- ----- ----- -----

		public static Expression<Func<Scope, object>> Generate(ProgramNode program) {
			var gen = new ExpressionGenerator();
			return gen.ConvertProgram(program);
		}

		// ----- ----- ----- ----- ----- fields ----- ----- ----- ----- -----
		ParameterExpression _globalScope = Expression.Parameter(typeof(Scope), "__global__");
		LexicalScope _currentScope = null;

		// ----- ----- ----- ----- ----- ctor ----- ----- ----- ----- -----
		private ExpressionGenerator() {

		}

		// ----- ----- ----- ----- ----- methods ----- ----- ----- ----- -----
		private Expression<Func<Scope, object>> ConvertProgram(ProgramNode program) {
			Debug.Assert(_currentScope == null);
			var stmts = program.Statements;
			var body = new Expression[stmts.Count];
			for (int i = 0; i < body.Length; i++)
				body[i] = ConvertStatement(stmts[i]);
			Debug.Assert(_currentScope == null);
			return Expression.Lambda<Func<Scope, object>>(
				Expression.Convert(Expression.Block(body), typeof(object)), _globalScope);
		}

		private Expression ConvertStatement(AbstractStatementNode stmt) {
			Debug.Assert(stmt != null);

			if (stmt is StatementNode)
				return Convert((StatementNode)stmt);
			if (stmt is IfNode)
				return Convert((IfNode)stmt);
			if (stmt is DefunNode)
				return Convert((DefunNode)stmt);
			throw new NotImplementedException();
		}

		#region 普通の手続き

		private Expression Convert(StatementNode stmt) {
			Debug.Assert(stmt != null, "stmt must not null.");
			Debug.Assert(stmt.Procedures.Count > 0, "num of procs must be positive.");

			Expression expr = null;
			foreach (var proc in stmt.Procedures)
				expr = Convert(proc, expr);
			return expr;
		}

		private Expression Convert(Procedure proc, Expression prevExpr) {
			var procName = proc.Name;
			switch (procName) {

			case "代入":
				return ConvertAssign(proc, prevExpr);

			default:
				return ConvertNormal(proc, prevExpr);
			}
			throw new NotImplementedException();
		}

		private Expression ConvertNormal(Procedure proc, Expression prevExpr) {
			int offset = prevExpr == null ? 1 : 2;
			Expression[] args = new Expression[proc.Arguments.Count + offset];
			args[0] = ConvertName(proc.Name);
			if (prevExpr != null)
				args[1] = prevExpr;
			for (int i = offset; i < args.Length; i++) {
				args[i] = Convert(proc.Arguments[i - offset].Target);
			}
			var pps = new string[proc.Arguments.Count];
			for (int i = 0; i < pps.Length; i++)
				pps[i] = proc.Arguments[i].PostPosition;
			return Expression.Dynamic(
				new KrgnInvokeBinder(new CallInfo(args.Length - 1, pps)),
				typeof(object),
				args);
		}

		#endregion

		#region if文

		private Expression Convert(IfNode stmt) {
			var thens = stmt.Thens;
			var last = thens[thens.Count - 1];
			Expression expr = null;
			if (last.Condition is ElseConditionNode)
				expr = Convert(last.Statement);
			else if (last.Condition is ExpressionConditionNode)
				expr = Expression.Condition(
					Convert(last.Condition),
					Convert(last.Statement),
					Expression.Constant(default(object)));
			for (int i = thens.Count - 2; i >= 0; i--) {
				var then = thens[i];
				var condExpr = Convert(then.Condition);
				expr = Expression.Condition(condExpr, Convert(then.Statement), expr);
			}
			return expr;
		}

		private Expression Convert(ConditionNode cond) {
			Expression expr = null;
			if (cond is ExpressionConditionNode) {
				expr = Convert(((ExpressionConditionNode)cond).Expression);
			}
			// object to bool
			var tmp = Expression.Parameter(typeof(object), "testtmp");
			return Expression.Block(
				new ParameterExpression[] { tmp },
				Expression.Assign(tmp, expr),
				Expression.Condition(
					Expression.TypeIs(tmp, typeof(bool)),
					Expression.Convert(tmp, typeof(bool)),
					Expression.NotEqual(tmp, Expression.Constant(null, typeof(object)))
			));
		}

		#endregion

		#region 関数宣言

		private Expression Convert(DefunNode defun) {
			// param
			var param = defun.Declare.Params;
			ParameterExpression[] paramExpr;
			if (param.All(p => p.Param is NormalParam)) {
				paramExpr = new ParameterExpression[param.Count];
				for (int i = 0; i < paramExpr.Length; i++)
					paramExpr[i] = Expression.Parameter(
						typeof(object),
						((NormalParam)param[i].Param).Name);
			}
			else {
				throw new NotImplementedException("「の」には対応していません。");
			}
			var pps = new string[param.Count];
			for (int i = 0; i < pps.Length; i++)
				pps[i] = param[i].PostPosition;
			Expression body;
			Define(defun.Declare.Name);
			using (Scope()) {
				_currentScope.AddRange(paramExpr);
				body = Convert(defun.Body);
			}
			var lambdaExpr = Expression.Lambda(body, paramExpr);
			Expression makeProcExpr;
			//if (pps.Length > 0)
			makeProcExpr = Expression.New(
				//typeof(KrgnFunc_).GetConstructor(new[] { typeof(Delegate), typeof(string[]) }),
				KrgnFunc.GetConstructorInfo(pps.Length),
				lambdaExpr,
				Expression.Constant(pps));
			//else
			//    makeProcExpr = Expression.New(
			//         typeof(KrgnFunc_0).GetConstructor(new[] { typeof(Delegate) }),
			//         lambdaExpr);
			return AssignToDefined(defun.Declare.Name, makeProcExpr);
		}

		#endregion

		#region ブロック 代入

		private Expression Convert(BlockNode block) {
			using (Scope()) {
				var stmts = block.Statements;
				Expression[] exprs = new Expression[stmts.Count];
				for (int i = 0; i < exprs.Length; i++)
					exprs[i] = ConvertStatement(stmts[i]);
				var variables = _currentScope.Variables;
				if (variables.Count > 0)
					return Expression.Block(variables, exprs);
				else
					return Expression.Block(exprs);
			}
		}

		private Expression ConvertAssign(Procedure proc, Expression prevExpr) {
			Debug.Assert(proc.Name == "代入");
			var valueNode = proc.Arguments.SingleOrDefault(a => a.PostPosition == "を");
			var valueExpr = valueNode != null ? Convert(valueNode.Target) : prevExpr;
			var nameNode = proc.Arguments.Single(a => a.PostPosition == "に").Target;
			var name = ((ReferenceExpression)nameNode).Name;
			return Assign(name, valueExpr);
		}

		private Expression Assign(string name, Expression value) {
			if (_currentScope != null) {
				var param = _currentScope.CreateVariable(name);
				return Expression.Assign(param, value);
			}
			else {
				return Expression.Call(
					_globalScope,
					typeof(Scope).GetMethod("SetVariable"),
					Expression.Constant(name),
					value);
			}
		}

		private void Define(string name) {
			if (_currentScope != null)
				_currentScope.CreateVariable(name);
		}

		private Expression AssignToDefined(string name, Expression value) {
			if (_currentScope != null) {
				var param = _currentScope.GetVariable(name);
				return Expression.Assign(param, value);
			}
			else {
				return Expression.Call(
					_globalScope,
					typeof(Scope).GetMethod("SetVariable"),
					Expression.Constant(name),
					value);
			}
		}

		#endregion

		#region 式

		private Expression Convert(ExpressionNode node) {
			if (node is ReferenceExpression)
				return Convert((ReferenceExpression)node);
			if (node is LiteralExpression)
				return Convert((LiteralExpression)node);
			if (node is BinaryExpression)
				return Convert((BinaryExpression)node);
			if (node is UnaryExpression)
				return Convert((UnaryExpression)node);
			throw new NotImplementedException();
		}

		private Expression Convert(ReferenceExpression refExpr) {
			return ConvertName(refExpr.Name);
		}

		private Expression ConvertName(string name) {
			if (_currentScope != null) {
				var expr = _currentScope.GetVariable(name);
				if (expr != null) return expr;
			}
			return Expression.Call(
				_globalScope,
				typeof(Scope).GetMethod("GetVariable"),
				Expression.Constant(name));
		}

		private Expression Convert(LiteralExpression literal) {
			return Expression.Constant(literal.Value);
		}

		private Expression Convert(BinaryExpression expr) {
			switch (expr.Operator) {
			case "+":
			case "＋":
				return ConvertSimpleBinary(ExpressionType.Add, expr);
			case "-":
			case "－":
				return ConvertSimpleBinary(ExpressionType.Subtract, expr);
			case "*":
			case "×":
				return ConvertSimpleBinary(ExpressionType.Multiply, expr);
			case "/":
			case "÷":
				return ConvertSimpleBinary(ExpressionType.Divide, expr);
			case "==":
			case "＝":
				return ConvertSimpleBinary(ExpressionType.Equal, expr);
			case "!=":
			case "≠":
				return ConvertSimpleBinary(ExpressionType.NotEqual, expr);
			case "<":
			case "＜":
				return ConvertSimpleBinary(ExpressionType.LessThan, expr);
			case "<=":
			case "≦":
				return ConvertSimpleBinary(ExpressionType.LessThanOrEqual, expr);
			case ">":
			case "＞":
				return ConvertSimpleBinary(ExpressionType.GreaterThan, expr);
			case ">=":
			case "≧":
				return ConvertSimpleBinary(ExpressionType.GreaterThanOrEqual, expr);
			case "&&":
			case "∧":
				return ConvertSimpleBinary(ExpressionType.AndAlso, expr);
			case "||":
			case "∨":
				return ConvertSimpleBinary(ExpressionType.OrElse, expr);
			}

			throw new NotImplementedException();
		}

		private Expression Convert(UnaryExpression expr) {
			switch (expr.Operator) {
			case "￢":
				return ConvertSimpleUnary(ExpressionType.Not, expr);
			}
			throw new NotImplementedException();
		}

		private Expression ConvertSimpleBinary(ExpressionType type, BinaryExpression expr) {
			return Expression.Dynamic(
				KrgnBinaryExpressionBinder.Create(type),
				typeof(object),
				Convert(expr.Left),
				Convert(expr.Right));
		}

		private Expression ConvertSimpleUnary(ExpressionType type, UnaryExpression expr) {
			return Expression.Dynamic(
				KrgnBinaryExpressionBinder.Create(type),
				typeof(object),
				Convert(expr.Expression));
		}

		#endregion

		#region scope

		private IDisposable Scope() {
			_currentScope = new LexicalScope(_currentScope);
			return new AbstractDisposable(() => {
				_currentScope = _currentScope.Parent;
			});
		}

		private class AbstractDisposable : IDisposable {
			private Action _onDispose;
			public AbstractDisposable(Action onDispose) {
				Debug.Assert(onDispose != null);
				_onDispose = onDispose;
			}
			public void Dispose() {
				_onDispose();
				_onDispose = null;
			}
		}

		#endregion
	}

	public class SemanticException : Exception {
		public SemanticException(string message)
			: base(message) {

		}
	}
}
