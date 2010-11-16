using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Kurogane.Util {
	public static class ExpressionUtil {

		private static readonly List<Type> _FuncTypeCache = new List<Type>();

		/// <summary>
		/// argCount個数の引数を受け取る関数の型を返します。
		/// 例えばargCountが1の場合、Func<object,object>を返します。
		/// </summary>
		/// <param name="argCount">関数の引数の個数</param>
		/// <returns>Funcを指すType型の値</returns>
		public static Type GetFuncType(int argCount) {
			if (argCount < 0)
				throw new ArgumentOutOfRangeException("argCount");
			int addSize = argCount - _FuncTypeCache.Count + 1;
			for (int i = 0; i < addSize; i++)
				_FuncTypeCache.Add(null);
			var type = _FuncTypeCache[argCount];
			if (type == null) {
				var types = new Type[argCount + 1];
				for (int i = 0; i < types.Length; i++)
					types[i] = typeof(object);
				type = Expression.GetFuncType(types);
				_FuncTypeCache[argCount] = type;
			}
			return type;
		}

		#region BetaReduction

		public static Expression BetaReduction<TResult>(Expression<Func<TResult>> func) {
			return func.Body;
		}

		public static Expression BetaReduction<T1, TResult>(Expression<Func<T1, TResult>> func, Expression arg) {
			var param = func.Parameters[0];
			var lst = new[] { new KeyValuePair<ParameterExpression, Expression>(param, arg) };
			return new Visitor(lst).Visit(func.Body);
		}

		public static Expression BetaReduction<T1, T2, TResult>(Expression<Func<T1, T2, TResult>> func, Expression arg1, Expression arg2) {
			var param1 = func.Parameters[0];
			var param2 = func.Parameters[1];
			var lst = new[] {
				new KeyValuePair<ParameterExpression, Expression>(param1, arg1),
				new KeyValuePair<ParameterExpression, Expression>(param2, arg2),
			};
			return new Visitor(lst).Visit(func.Body);
		}

		public static Expression BetaReduction<T1, T2, T3, TResult>(Expression<Func<T1, T2, T3, TResult>> func, Expression arg1, Expression arg2, Expression arg3) {
			var param = func.Parameters;
			var lst = new[] {
				new KeyValuePair<ParameterExpression, Expression>(param[0], arg1),
				new KeyValuePair<ParameterExpression, Expression>(param[1], arg2),
				new KeyValuePair<ParameterExpression, Expression>(param[2], arg3),
			};
			return new Visitor(lst).Visit(func.Body);
		}

		public static Expression BetaReduction(LambdaExpression lambda, params Expression[] args) {
			if (lambda.Parameters.Count != args.Length)
				throw new ArgumentException("引数の数が異なります。");
			var lst = lambda.Parameters.Zip(args, (k, v) => new KeyValuePair<ParameterExpression, Expression>(k, v)).ToList();
			return new Visitor(lst).Visit(lambda.Body);
		}

		private class Visitor : ExpressionVisitor {

			private readonly IList<KeyValuePair<ParameterExpression, Expression>> _dic;

			public Visitor(IList<KeyValuePair<ParameterExpression, Expression>> dic) {
				_dic = dic;
			}

			protected override Expression VisitParameter(ParameterExpression node) {
				foreach (var pair in _dic)
					if (pair.Key == node)
						return pair.Value;
				return base.VisitParameter(node);
			}
		}

		#endregion

	}
}
