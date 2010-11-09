using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;

namespace Kurogane.Expressions {

	public class InvokeStatic : ExpressionVisitor {
		// ----- ----- ----- ----- ----- inner class ----- ----- ----- ----- -----

		private class SuffixFuncInfo {
			public string[] Suffix { get; set; }
			public ParameterExpression FuncExpr { get; set; }
			public bool IsUsed { get; set; }
		}

		// ----- ----- ----- ----- ----- field ----- ----- ----- ----- -----

		private Dictionary<ParameterExpression, SuffixFuncInfo> _dic = new Dictionary<ParameterExpression, SuffixFuncInfo>();

		// ----- ----- ----- ----- ----- methods ----- ----- ----- ----- -----

		protected override Expression VisitBlock(BlockExpression node) {
			var block = base.VisitBlock(node) as BlockExpression;
			List<ParameterExpression> lst = new List<ParameterExpression>();
			foreach (var param in node.Variables) {
				SuffixFuncInfo info;
				if (_dic.TryGetValue(param, out info)) {
					lst.Add(info.FuncExpr);
				}
			}
			if (lst.Count == 0) {
				return block;
			}
			else {
				lst.AddRange(node.Variables);
				return Expression.Block(lst, block.Expressions);
			}
		}

		protected override Expression VisitBinary(BinaryExpression node) {
			var result = TryRegisterSuffixFunc(node);
			if (result == null)
				return base.VisitBinary(node);
			else
				return result;
		}

		private Expression TryRegisterSuffixFunc(BinaryExpression node) {
			if (node.NodeType != ExpressionType.Assign)
				return null;
			var left = node.Left as ParameterExpression;
			if (left == null)
				return null;
			var right = node.Right as NewExpression;
			if (right == null || right.Arguments.Count == 0)
				return null;
			var sfxFuncType = right.Constructor.DeclaringType;
			if (typeof(SuffixFunc<>) != sfxFuncType.GetGenericTypeDefinition())
				return null;
			//if ((right.Arguments[0] is LambdaExpression) == false)
			//    return null;
			var funcType = sfxFuncType.GetGenericArguments()[0];
			var suffix = ((ConstantExpression)right.Arguments[1]).Value as string[];
			var param = Expression.Parameter(funcType, left.Name + "_Body");
			var info = new SuffixFuncInfo { Suffix = suffix, FuncExpr = param };
			_dic[left] = info;

			var body = this.Visit(right.Arguments[0]) as LambdaExpression;
			if (info.IsUsed)
				body = Expression.Lambda(body.Body, true, body.Parameters);
			return Expression.Assign(left, Expression.New(right.Constructor, Expression.Assign(param, body), right.Arguments[1]));
		}

		protected override Expression VisitDynamic(DynamicExpression node) {
			var expr = CallStatic(node);
			if (expr == null)
				return base.VisitDynamic(node);
			else
				return expr;
		}

		private Expression CallStatic(DynamicExpression node) {
			var binder = node.Binder as InvokeBinder;
			if (binder == null)
				return null;
			var target = node.Arguments[0] as ParameterExpression;
			if (target == null)
				return null;
			SuffixFuncInfo info;
			if (_dic.TryGetValue(target, out info) == false)
				return null;
			var expr = SuffixFunc.SortAndInvoke(info.FuncExpr, info.Suffix, binder.CallInfo, i => node.Arguments[i + 1]);
			if (expr is InvocationExpression)
				info.IsUsed = true;
			return expr;
		}
	}
}
