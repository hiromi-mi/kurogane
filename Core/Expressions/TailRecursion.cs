using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Collections.ObjectModel;

namespace Kurogane.Expressions {


	public class TailRecursion : ExpressionVisitor {

		private readonly string _name;
		private readonly ParameterExpression _invokeTarget;
		private readonly LambdaExpression _lambda;
		private readonly ReadOnlyCollection<ParameterExpression> _params;

		/// <summary>末尾再帰した場合nullではなくなる。</summary>
		private LabelTarget tailGoto = null;

		public TailRecursion(string name, ParameterExpression left, LambdaExpression right) {
			_name = name;
			_invokeTarget = left;
			_lambda = right;
			_params = right.Parameters;
		}

		/// <summary>
		/// 処理開始
		/// </summary>
		public LambdaExpression Run(LambdaExpression lambda) {
			var body = lambda.Body;
			var label = body as LabelExpression;
			if (label != null)
				body = label.DefaultValue;
			var result = this.Visit(body); // 内側を検索
			if (result == body || tailGoto == null) {
				// 書き換えが起こらなかった
				return lambda;
			}
			var labelExpr = Expression.Label(tailGoto);
			var block = result as BlockExpression;
			if (block == null) {
				result = Expression.Block(labelExpr, result);
			}
			else {
				result = Expression.Block(
					block.Variables,
					Enumerable.Concat(new Expression[] { labelExpr }, block.Expressions));
			}
			if (label != null)
				result = Expression.Label(label.Target, result);
			return Expression.Lambda(result, lambda.Parameters);
		}

		/// <summary>
		/// 関数呼び出しをJumpに置き換え
		/// </summary>
		private Expression TryReplaceTailJump(InvocationExpression node) {
			if (node.Expression != _invokeTarget)
				return null;
			var size = _params.Count;
			var tmps = new ParameterExpression[size];
			for (int i = 0; i < tmps.Length; i++)
				tmps[i] = Expression.Parameter(_params[i].Type, "tmp_" + _params[i].Name);
			var assigns = new Expression[size * 2 + 1];
			for (int i = 0; i < tmps.Length; i++)
				assigns[i] = Expression.Assign(tmps[i], node.Arguments[i]);
			for (int i = 0; i < size; i++)
				assigns[i + size] = Expression.Assign(_params[i], tmps[i]);
			var target = tailGoto ?? (tailGoto = Expression.Label("#start_of_" + _name));
			assigns[size * 2] = Expression.Goto(target, node.Type);
			return Expression.Block(tmps, assigns);
		}

		#region 走査

		protected override Expression VisitInvocation(InvocationExpression node) {
			// 最後が関数呼び出しの場合、Jump出来るかもしれない。
			var expr = TryReplaceTailJump(node);
			if (expr != null)
				return expr;
			// 関数の内側にチェックは必要ない。
			return node;
		}

		#region 何もしない

		protected override Expression VisitBinary(BinaryExpression node) {
			// 最後が計算なら何もしない。
			return node;
		}

		protected override Expression VisitUnary(UnaryExpression node) {
			// 何もしない
			return node;
		}

		protected override Expression VisitDynamic(DynamicExpression node) {
			// 動的な実行はそのまま
			return node;
		}

		protected override Expression VisitMethodCall(MethodCallExpression node) {
			// そのまま実行
			return node;
		}

		protected override Expression VisitNew(NewExpression node) {
			// そのまま作成する。
			return node;
		}

		#endregion

		#region 最後のみを走査

		protected override Expression VisitBlock(BlockExpression node) {
			if (node.Variables.Count == 0 && node.Expressions.Count == 1) {
				// ローカル変数が無く、式がひとつなら、その式として実行すればよい。
				return Visit(node.Expressions[0]);
			}
			// ブロックの場合、そのブロックの最後のみを走査する。
			Expression nextLastExpr = null;
			var lastExpr = node.Expressions[node.Expressions.Count - 1];
			nextLastExpr = Visit(lastExpr);
			if (nextLastExpr == lastExpr)
				return node;
			var exprs = new Expression[node.Expressions.Count];
			for (int i = 0; i < exprs.Length - 1; i++)
				exprs[i] = node.Expressions[i];
			exprs[exprs.Length - 1] = nextLastExpr;
			return Expression.Block(node.Variables, exprs);
		}

		protected override Expression VisitConditional(ConditionalExpression node) {
			// 条件文の場合、条件式以外（ifTrue, ifFalse)を走査する。
			var ifTrue = Visit(node.IfTrue);
			var ifFalse = Visit(node.IfFalse);
			if (ifTrue == node.IfTrue && ifFalse == node.IfFalse)
				return node;
			else
				return Expression.Condition(node.Test, ifTrue, ifFalse);
		}

		#endregion

		#endregion

	}
}
