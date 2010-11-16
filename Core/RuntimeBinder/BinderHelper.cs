
using System.Linq.Expressions;
using System.Reflection;
using System;
using System.Dynamic;

namespace Kurogane.RuntimeBinder {

	public static class BinderHelper {

		/// <summary>
		/// 値が「nullである」かどうかを判定する式を返す。
		/// </summary>
		public static Expression IsNull(Expression expr) {
			return Expression.Not(IsNotNull(expr));
		}

		/// <summary>
		/// 値が「nullでない」かどうかを判定する式を返す。
		/// </summary>
		public static Expression IsNotNull(Expression expr) {
			return Expression.TypeIs(expr, typeof(object));
		}

		/// <summary>
		/// 暗黙の型変換が存在すれば、そのメソッドを返す。
		/// </summary>
		public static MethodInfo GetImplicitCast(Type from, Type to) {
			const string name = "op_Implicit";
			var types = new[] { from };
			var mInfo = to.GetMethod(name, types);
			if (mInfo != null && mInfo.ReturnType == to)
				return mInfo;
			foreach (var m in from.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod)) {
				if (m.Name == name && m.ReturnType == to) {
					var argInfo = m.GetParameters();
					if (argInfo.Length == 1 && argInfo[1].GetType() == from)
						return m;
				}
			}
			return null;
		}

		/// <summary>
		/// 式木の型をチェックして、必要があればキャストする。
		/// </summary>
		public static Expression Wrap(Expression expr, Type type1) {
			if (expr.Type == type1)
				return expr;
			return Expression.Convert(expr, type1);
		}

		/// <summary>
		/// 式木の型をチェックして、必要があればキャストする。
		/// </summary>
		public static Expression Wrap(Expression expr, Type type1, Type type2) {
			if (expr.Type == type2)
				return expr;
			return Expression.Convert(Wrap(expr, type1), type2);
		}

		/// <summary>
		/// それぞれのLimitTypeからRestrictionを作成する。
		/// </summary>
		public static BindingRestrictions GetTypeRestriction(DynamicMetaObject left, DynamicMetaObject right) {
			return BindingRestrictions.GetExpressionRestriction(
				Expression.AndAlso(
					Expression.TypeIs(left.Expression, left.LimitType),
					Expression.TypeIs(right.Expression, right.LimitType)));
		}

	}
}
