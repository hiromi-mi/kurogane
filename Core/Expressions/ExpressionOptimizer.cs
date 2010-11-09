using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using Kurogane.RuntimeBinder;

namespace Kurogane.Expressions {

	public class ExpressionOptimizer {

		// ----- ----- ----- ----- ----- static ----- ----- ----- ----- -----

		public static Expression<T> Analyze<T>(Expression<T> expr) {
			return Expression.Lambda<T>(AnalyzeCore(expr.Body), expr.Parameters);
		}

		private static Expression AnalyzeCore(Expression expr) {
			// 意味解析後、同じ関数なら静的呼び出しに変更。
			var opt1 = new InvokeStatic();
			expr = opt1.Visit(expr);
			// 静的解析後、可能なら末尾再帰最適化。
			var opt2 = new LambdaFinder();
			expr = opt2.Visit(expr);
			// 終わり
			return expr;
		}
	}
}
