using System;
using System.Dynamic;
using System.Linq.Expressions;
using Kurogane.Util;

namespace Kurogane.RuntimeBinder {

	/// <summary>
	/// 算術演算用のBinderクラス。
	/// 型が違う場合の組み込み数値計算はint<->double間でのみ可能。
	/// nullが含まれる場合，エラーを投げる。
	/// </summary>
	public class ArithmeticBinder : BinaryOperationBinder {

		private readonly string _name;

		public ArithmeticBinder(ExpressionType operation, string name)
			: base(operation) {
			_name = name;
		}

		public override DynamicMetaObject FallbackBinaryOperation(DynamicMetaObject target, DynamicMetaObject arg, DynamicMetaObject errorSuggestion) {
			if (target.Value == null || arg.Value == null)
				return FallbackOnNull(target, arg);
			try {
				Expression expr = Expression.MakeBinary(
					this.Operation,
					BinderHelper.Wrap(target.Expression, target.LimitType),
					BinderHelper.Wrap(arg.Expression, arg.LimitType));
				return new DynamicMetaObject(
					BinderHelper.Wrap(expr, this.ReturnType),
					BinderHelper.GetTypeRestriction(target, arg));
			}
			catch (InvalidOperationException) {
				return DefferentType(target, arg);
			}
		}

		/// <summary>
		/// 値のいずれかがnullであった場合の処理。
		/// </summary>
		private DynamicMetaObject FallbackOnNull(DynamicMetaObject left, DynamicMetaObject right) {
			var nullExpr = Expression.Constant(null);
			Expression expr;
			string errorMsg;
			if (left.Value == null && right.Value != null) {
				expr = Expression.AndAlso(
					BinderHelper.IsNull(left.Expression),
					BinderHelper.IsNotNull(right.Expression));
				errorMsg = String.Format("{0}と{1}を{2}できません。",
					ConstantNames.NullText, right.LimitType.Name, _name);

			}
			else if (right.Value == null && left.Value != null) {
				expr = Expression.AndAlso(
					BinderHelper.IsNotNull(left.Expression),
					BinderHelper.IsNull(right.Expression));
				errorMsg = String.Format("{0}と{1}を{2}できません。",
					left.LimitType.Name, ConstantNames.NullText, _name);
			}
			else {
				expr = Expression.And(
					BinderHelper.IsNull(left.Expression),
					BinderHelper.IsNull(right.Expression));
				errorMsg = String.Format("{0}同士を{1}できません。",
					ConstantNames.NullText, _name);
			}
			return RuntimeBinderException.CreateMetaObject(errorMsg, BindingRestrictions.GetExpressionRestriction(expr));
		}

		/// <summary>
		/// 暗黙のキャストを探す。
		/// </summary>
		private DynamicMetaObject DefferentType(DynamicMetaObject left, DynamicMetaObject right) {
			var val = TryCalcNumeric(left, right);
			if (val != null) return val;
			// 左辺のキャスト
			var mInfo = BinderHelper.GetImplicitCast(left.LimitType, right.LimitType);
			if (mInfo != null) {
				Expression expr = Expression.MakeBinary(this.Operation,
						BinderHelper.Wrap(left.Expression, left.LimitType, right.LimitType),
						Expression.Convert(right.Expression, right.LimitType));
				return new DynamicMetaObject(
					BinderHelper.Wrap(expr, this.ReturnType),
					BinderHelper.GetTypeRestriction(left, right));
			}
			mInfo = BinderHelper.GetImplicitCast(right.LimitType, left.LimitType);
			if (mInfo != null) {
				Expression expr = Expression.MakeBinary(this.Operation,
						Expression.Convert(left.Expression, left.LimitType),
						BinderHelper.Wrap(right.Expression, right.LimitType, left.LimitType));
				return new DynamicMetaObject(
					BinderHelper.Wrap(expr, this.ReturnType),
					BinderHelper.GetTypeRestriction(left, right));
			}
			return NoResult(left, right);
		}

		/// <summary>
		/// 整数と少数の演算を行う
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		private DynamicMetaObject TryCalcNumeric(DynamicMetaObject left, DynamicMetaObject right) {
			// 二段キャストしないと，InvalidCastになるので注意
			Expression expr = null;
			if (left.LimitType == typeof(int) && right.LimitType == typeof(double)) {
				expr = Expression.MakeBinary(this.Operation,
						Expression.Convert(Expression.Convert(left.Expression, left.LimitType), typeof(double)),
						Expression.Convert(right.Expression, typeof(double)));
			}
			if (left.LimitType == typeof(double) && right.LimitType == typeof(int)) {
				expr = Expression.MakeBinary(this.Operation,
						Expression.Convert(left.Expression, typeof(double)),
						Expression.Convert(Expression.Convert(right.Expression, right.LimitType), typeof(double)));
			}
			if (expr != null) {
				return new DynamicMetaObject(
					BinderHelper.Wrap(expr, this.ReturnType),
					BinderHelper.GetTypeRestriction(left, right));
			}
			return null;
		}

		/// <summary>
		/// 適切な計算方法が見つからなかった場合。
		/// </summary>
		private DynamicMetaObject NoResult(DynamicMetaObject left, DynamicMetaObject right) {
			var errorMsg = "{0}と{1}を" + _name + "出来ません。";
			var mInfo = typeof(String).GetMethod("Format", new[] { typeof(string), typeof(object), typeof(object) });
			var msgExpr = Expression.Call(mInfo, Expression.Constant(errorMsg), left.Expression, right.Expression);
			var ctorInfo = typeof(RuntimeBinderException).GetConstructor(new[] { typeof(string) });
			var expr = Expression.Throw(Expression.New(ctorInfo, msgExpr), typeof(object));
			return new DynamicMetaObject(expr, BinderHelper.GetTypeRestriction(left, right));
		}
	}
}
