
using System.Linq.Expressions;
using System.Reflection;
using System;
using System.Dynamic;
using System.Diagnostics.Contracts;

namespace Kurogane.RuntimeBinder {

	public static class BinderHelper {

		/// <summary>
		/// 値が「nullである」かどうかを判定する式を返す。
		/// </summary>
		public static Expression IsNull(Expression expr) {
			Contract.Requires<ArgumentNullException>(expr != null);
			Contract.Ensures(Contract.Result<Expression>() != null);
			return Expression.Not(IsNotNull(expr));
		}

		/// <summary>
		/// 値が「nullでない」かどうかを判定する式を返す。
		/// </summary>
		public static Expression IsNotNull(Expression expr) {
			Contract.Requires<ArgumentNullException>(expr != null);
			Contract.Ensures(Contract.Result<Expression>() != null);
			return Expression.TypeIs(expr, typeof(object));
		}

		/// <summary>
		/// 暗黙の型変換が存在すれば、そのメソッドを返す。
		/// </summary>
		public static MethodInfo GetImplicitCast(Type from, Type to) {
			Contract.Requires<ArgumentNullException>(from != null);
			Contract.Requires<ArgumentNullException>(to != null);
			const string name = "op_Implicit";
			var types = new[] { from };
			var mInfo = to.GetMethod(name, types);
			if (mInfo != null && mInfo.ReturnType == to)
				return mInfo;
			foreach (var m in from.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod)) {
				if (m.Name == name && m.ReturnType == to) {
					var argInfo = m.GetParameters();
					if (argInfo.Length == 1 && argInfo[0].GetType() == from)
						return m;
				}
			}
			return null;
		}

		/// <summary>
		/// 式木の型をチェックして、必要があればキャストする。
		/// </summary>
		public static Expression Wrap(Expression expr, Type type1) {
			Contract.Requires<ArgumentNullException>(expr != null);
			Contract.Requires<ArgumentNullException>(type1 != null);
			Contract.Ensures(Contract.Result<Expression>().Type == type1);
			if (expr.Type == type1)
				return expr;
			return Expression.Convert(expr, type1);
		}

		/// <summary>
		/// 式木の型をチェックして、必要があればキャストする。
		/// </summary>
		public static Expression Wrap(Expression expr, Type type1, Type type2) {
			Contract.Requires<ArgumentNullException>(expr != null);
			Contract.Requires<ArgumentNullException>(type1 != null);
			Contract.Requires<ArgumentNullException>(type2 != null);
			Contract.Ensures(Contract.Result<Expression>().Type == type2);
			if (expr.Type == type2)
				return expr;
			return Expression.Convert(Wrap(expr, type1), type2);
		}

		/// <summary>
		/// それぞれのLimitTypeからRestrictionを作成する。
		/// </summary>
		public static BindingRestrictions GetTypeRestriction(DynamicMetaObject left, DynamicMetaObject right) {
			Contract.Requires<ArgumentNullException>(left != null);
			Contract.Requires<ArgumentNullException>(right != null);
			Contract.Ensures(Contract.Result<BindingRestrictions>() != null);
			return BindingRestrictions.GetExpressionRestriction(
				Expression.AndAlso(
					Expression.TypeIs(left.Expression, left.LimitType),
					Expression.TypeIs(right.Expression, right.LimitType)));
		}

		/// <summary>
		/// 引数の値を確かめて、いずれかがnullの場合、InvalidOperationExceptionを投げるDynamicMetaObjectを返す。
		/// そうでない場合はnullを返す。
		/// </summary>
		/// <param name="name">Operation名</param>
		/// <param name="type">式が想定する型</param>
		/// <param name="left">左辺</param>
		/// <param name="right">右辺</param>
		/// <returns>Throw式あるいはnull</returns>
		internal static DynamicMetaObject NullErrorOnOperation(string name, Type type, DynamicMetaObject left, DynamicMetaObject right) {
			Contract.Requires<ArgumentException>(String.IsNullOrWhiteSpace(name) == false);
			Contract.Requires<ArgumentException>(left.Value == null || right.Value == null);
			Contract.Ensures(Contract.Result<DynamicMetaObject>() != null);
	
			var ctorInfo = typeof(InvalidOperationException).GetConstructor(new[] { typeof(string) });
			var format = typeof(String).GetMethod("Format", new[] { typeof(string), typeof(object) });
			if (left.Value == null && right.Value == null) {
				var msg = ConstantNames.NullText + "同士を" + name + "できません。";
				var expr = Expression.Throw(Expression.New(ctorInfo, Expression.Constant(msg)), type);
				var rest = BindingRestrictions.GetExpressionRestriction(
						Expression.AndAlso(IsNull(left.Expression), IsNull(right.Expression)));
				return new DynamicMetaObject(expr, rest);
			}
			if (left.Value != null && right.Value == null) {
				var msg = "{0}と" + ConstantNames.NullText + "を" + name + "できません。";
				var formatTxt = Expression.Constant(msg);
				var msgExpr = Expression.Call(format, formatTxt, Expression.Convert(left.Expression, typeof(object)));
				var expr = Expression.Throw(Expression.New(ctorInfo, msgExpr), type);
				var rest = BindingRestrictions.GetExpressionRestriction(Expression.AndAlso(IsNull(right.Expression), IsNotNull(left.Expression)));
				return new DynamicMetaObject(expr, rest);
			}
			if (left.Value == null && right.Value != null) {
				var msg = ConstantNames.NullText + "と{0}を" + name + "できません。";
				var formatTxt = Expression.Constant(msg);
				var msgExpr = Expression.Call(format, formatTxt, Expression.Convert(right.Expression, typeof(object)));
				var expr = Expression.Throw(Expression.New(ctorInfo, msgExpr), type);
				var rest = BindingRestrictions.GetExpressionRestriction(Expression.AndAlso(IsNull(left.Expression), IsNotNull(right.Expression)));
				return new DynamicMetaObject(expr, rest);
			}
			throw new InvalidOperationException();
		}

		/// <summary>
		/// 適切な計算方法が見つからなかった場合、RuntimeBinderExceptionを投げる。
		/// </summary>
		public static DynamicMetaObject NoResult(string name, Type type, DynamicMetaObject left, DynamicMetaObject right) {
			var ctorInfo = typeof(InvalidOperationException).GetConstructor(new[] { typeof(string) });
			var errorMsg = "{0}と{1}を" + name + "出来ません。";
			var mInfo = typeof(String).GetMethod("Format", new[] { typeof(string), typeof(object), typeof(object) });
			var msgExpr = Expression.Call(mInfo,
				Expression.Constant(errorMsg),
				Expression.Convert(left.Expression, typeof(object)),
				Expression.Convert(right.Expression, typeof(object)));
			var expr = Expression.Throw(Expression.New(ctorInfo, msgExpr), typeof(object));
			return new DynamicMetaObject(expr, BinderHelper.GetTypeRestriction(left, right));
		}
	}
}
