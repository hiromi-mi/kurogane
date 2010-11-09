using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace Kurogane.Expressions {

	/// <summary>
	/// ラムダ関数を見つけ、TailRecursionを呼び出すVisitor。
	/// </summary>
	public class LambdaFinder : ExpressionVisitor {

		protected override Expression VisitBinary(BinaryExpression node) {
			var nextNode = base.VisitBinary(node);
			node = nextNode as BinaryExpression;
			if (node == null)
				return nextNode;
			var expr = TryOptimize(node);
			if (expr != null)
				return expr;
			return nextNode;
		}

		private Expression TryOptimize(BinaryExpression node) {
			if (node.NodeType != ExpressionType.Assign)
				return null;
			var right = node.Right as LambdaExpression;
			if (right == null)
				return null;
			var left = node.Left as ParameterExpression;
			if (left == null)
				return null;
			var visitor = new TailRecursion(left.Name, left, right);
			right = visitor.Run(right) as LambdaExpression;
			if (right == null)
				return null;
			return Expression.Assign(left, right);
		}


	}
}
