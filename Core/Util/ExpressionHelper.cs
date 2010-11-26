using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Diagnostics.Contracts;

namespace Kurogane.Util {
	public static class ExpressionHelper {

		private static readonly List<Type> _FuncTypeCache = new List<Type>();

		/// <summary>
		/// argCount個数の引数を受け取る関数の型を返します。
		/// 例えばargCountが1の場合、Func<object,object>を返します。
		/// </summary>
		/// <param name="argCount">関数の引数の個数</param>
		/// <returns>Funcを指すType型の値</returns>
		public static Type GetFuncType(int argCount) {
			Contract.Requires<ArgumentOutOfRangeException>(argCount >= 0);
			Contract.Ensures(Contract.Result<Type>() != null);

			int addSize = argCount - _FuncTypeCache.Count + 1;
			for (int i = 0; i < addSize; i++)
				_FuncTypeCache.Add(null);
			var type = _FuncTypeCache[argCount];
			if (type == null) {
				var types = new Type[argCount + 1];
				for (int i = 0; i < types.Length; i++)
					types[i] = typeof(object);
				if (Expression.TryGetFuncType(types, out type))
					_FuncTypeCache[argCount] = type;
				else
					_FuncTypeCache[argCount] = Expression.GetDelegateType(types);
			}
			return type;
		}

		public static Expression Wrap(Expression expr, Type type) {
			Contract.Requires<ArgumentNullException>(expr != null);
			Contract.Requires<ArgumentNullException>(type != null);
			Contract.Ensures(Contract.Result<Expression>().Type == type);

			if (expr.Type == type)
				return expr;
			return Expression.Convert(expr, type);
		}

		#region BetaReduction

		public static Expression BetaReduction<TResult>(Expression<Func<TResult>> func) {
			Contract.Requires<ArgumentNullException>(func != null);
			Contract.Ensures(Contract.Result<Expression>() != null);
			return func.Body;
		}

		public static Expression BetaReduction<T1, TResult>(Expression<Func<T1, TResult>> func, Expression arg1) {
			Contract.Requires<ArgumentNullException>(func != null);
			Contract.Requires<ArgumentNullException>(arg1 != null);
			Contract.Requires<ArgumentException>(arg1.Type == typeof(T1));
			Contract.Ensures(Contract.Result<Expression>() != null);
			Contract.Ensures(Contract.Result<Expression>().Type == typeof(TResult));
			var param = func.Parameters[0];
			var lst = new[] { new KeyValuePair<ParameterExpression, Expression>(param, arg1) };
			return new Visitor(lst).Visit(func.Body);
		}

		public static Expression BetaReduction<T1, T2, TResult>(Expression<Func<T1, T2, TResult>> func, Expression arg1, Expression arg2) {
			Contract.Requires<ArgumentNullException>(func != null);
			Contract.Requires<ArgumentNullException>(arg1 != null);
			Contract.Requires<ArgumentNullException>(arg2 != null);
			Contract.Requires<ArgumentException>(arg1.Type == typeof(T1));
			Contract.Requires<ArgumentException>(arg2.Type == typeof(T2));
			Contract.Ensures(Contract.Result<Expression>() != null);
			Contract.Ensures(Contract.Result<Expression>().Type == typeof(TResult));
			var param1 = func.Parameters[0];
			var param2 = func.Parameters[1];
			var lst = new[] {
				new KeyValuePair<ParameterExpression, Expression>(param1, arg1),
				new KeyValuePair<ParameterExpression, Expression>(param2, arg2),
			};
			return new Visitor(lst).Visit(func.Body);
		}

		public static Expression BetaReduction<T1, T2, T3, TResult>(Expression<Func<T1, T2, T3, TResult>> func, Expression arg1, Expression arg2, Expression arg3) {
			Contract.Requires<ArgumentNullException>(func != null);
			Contract.Requires<ArgumentNullException>(arg1 != null);
			Contract.Requires<ArgumentNullException>(arg2 != null);
			Contract.Requires<ArgumentNullException>(arg3 != null);
			Contract.Requires<ArgumentException>(arg1.Type == typeof(T1));
			Contract.Requires<ArgumentException>(arg2.Type == typeof(T2));
			Contract.Requires<ArgumentException>(arg3.Type == typeof(T3));
			Contract.Ensures(Contract.Result<Expression>() != null);
			Contract.Ensures(Contract.Result<Expression>().Type == typeof(TResult));
			var param = func.Parameters;
			var lst = new[] {
				new KeyValuePair<ParameterExpression, Expression>(param[0], arg1),
				new KeyValuePair<ParameterExpression, Expression>(param[1], arg2),
				new KeyValuePair<ParameterExpression, Expression>(param[2], arg3),
			};
			return new Visitor(lst).Visit(func.Body);
		}

		public static Expression BetaReduction(LambdaExpression lambda, params Expression[] args) {
			Contract.Requires<ArgumentNullException>(lambda != null);
			Contract.Requires<ArgumentException>(Contract.ForAll(args, arg => arg != null));
			Contract.Requires<ArgumentException>(lambda.Parameters.Count == args.Length);
			Contract.Ensures(Contract.Result<Expression>() != null);
			var lst = lambda.Parameters.Zip(args, (k, v) => new KeyValuePair<ParameterExpression, Expression>(k, v)).ToList();
			return new Visitor(lst).Visit(lambda.Body);
		}

		private class Visitor : ExpressionVisitor {

			private readonly IList<KeyValuePair<ParameterExpression, Expression>> _dic;

			public Visitor(IList<KeyValuePair<ParameterExpression, Expression>> dic) {
				Contract.Requires<ArgumentNullException>(dic != null);
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
