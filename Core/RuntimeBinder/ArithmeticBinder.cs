using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;
using System.Linq.Expressions;
using System.Diagnostics;
using System.Reflection;

namespace Kurogane.RuntimeBinder {

	/// <summary>
	/// 算術演算用のBinderクラス。
	/// 型が違う場合の組み込み数値計算はint<->double間でのみ可能。
	/// nullが含まれる場合，エラーを投げる。
	/// </summary>
	public class ArithmeticBinder : BinaryOperationBinder {

		private readonly string _name;
		private readonly string _ilMethodName;

		public ArithmeticBinder(ExpressionType operation, string name, string ilMethodName)
			: base(operation) {
			_name = name;
			_ilMethodName = ilMethodName;
		}

		public override DynamicMetaObject FallbackBinaryOperation(DynamicMetaObject target, DynamicMetaObject arg, DynamicMetaObject errorSuggestion) {
			if (target.Value == null || arg.Value == null)
				return FallbackOnNull(target, arg);
			if (target.LimitType == arg.LimitType)
				return CalcOnSameType(target, arg);
			else
				return CalcOnDefferentType(target, arg);
		}

		private DynamicMetaObject FallbackOnNull(DynamicMetaObject left, DynamicMetaObject right) {
			var nullExpr = Expression.Constant(null);
			Expression expr;
			string errorMsg;
			if (left.Value == null && right.Value != null) {
				expr = Expression.AndAlso(
					Expression.Equal(left.Expression, nullExpr),
					Expression.NotEqual(right.Expression, nullExpr));
				errorMsg = String.Format("{0}と{1}を{2}できません。",
					ConstantNames.NullText, right.LimitType.Name, _name);

			}
			else if (right.Value == null && left.Value != null) {
				expr = Expression.AndAlso(
					Expression.Equal(right.Expression, nullExpr),
					Expression.NotEqual(left.Expression, nullExpr));
				errorMsg = String.Format("{0}と{1}を{2}できません。",
					left.LimitType.Name, ConstantNames.NullText, _name);
			}
			else {
				expr = Expression.And(
					Expression.Equal(left.Expression, nullExpr),
					Expression.Equal(right.Expression, nullExpr));
				errorMsg = String.Format("{0}同士を{1}できません。",
					ConstantNames.NullText, _name);
			}
			return RuntimeBinderException.CreateMetaObject(errorMsg, BindingRestrictions.GetExpressionRestriction(expr));
		}

		private DynamicMetaObject CalcOnSameType(DynamicMetaObject left, DynamicMetaObject right) {
			if (left.LimitType == typeof(int) || left.LimitType == typeof(double))
				return new DynamicMetaObject(
					Expression.Convert(Expression.MakeBinary(this.Operation,
							Expression.Convert(left.Expression, left.LimitType),
							Expression.Convert(right.Expression, right.LimitType)),
						typeof(object)),
					GetTypeRestriction(left, right));
			return FallbackMethodSearch(left, right);
		}

		private DynamicMetaObject CalcOnDefferentType(DynamicMetaObject left, DynamicMetaObject right) {
			// 二段キャストしないと，InvalidCastになるので注意
			if (left.LimitType == typeof(int) && right.LimitType == typeof(double)) {
				var expr = Expression.Convert(Expression.MakeBinary(this.Operation,
						Expression.Convert(Expression.Convert(left.Expression, left.LimitType), typeof(double)),
						Expression.Convert(right.Expression, typeof(double))),
					typeof(object));
				return new DynamicMetaObject(expr, GetTypeRestriction(left, right));
			}
			if (left.LimitType == typeof(double) && right.LimitType == typeof(int)) {
				var expr = Expression.Convert(Expression.MakeBinary(this.Operation,
						Expression.Convert(left.Expression, typeof(double)),
						Expression.Convert(Expression.Convert(right.Expression, right.LimitType), typeof(double))),
					typeof(object));
				return new DynamicMetaObject(expr, GetTypeRestriction(left, right));
			}
			return FallbackMethodSearch(left, right);
		}

		private DynamicMetaObject FallbackMethodSearch(DynamicMetaObject left, DynamicMetaObject right) {
			// operatorがクラスに定義されている。
			var types = new[] { left.LimitType, right.LimitType };
			var mInfo = left.LimitType.GetMethod(_ilMethodName, types) ?? right.LimitType.GetMethod(_ilMethodName, types);
			if (mInfo != null)
				return new DynamicMetaObject(Expression.Call(mInfo,
						Expression.Convert(left.Expression, left.LimitType),
						Expression.Convert(right.Expression, right.LimitType)),
					GetTypeRestriction(left, right));

			// 何も見つからなかった。
			string errorMsg = String.Format("{0}と{1}を{2}できません。",
				left.LimitType.Name, right.LimitType.Name, _name);
			return RuntimeBinderException.CreateMetaObject(errorMsg, GetTypeRestriction(left, right));
		}

		private BindingRestrictions GetTypeRestriction(DynamicMetaObject left, DynamicMetaObject right) {
			var nullExpr = Expression.Constant(null);
			return BindingRestrictions.GetExpressionRestriction(
				Expression.AndAlso(
					Expression.AndAlso(
						Expression.NotEqual(left.Expression, nullExpr),
						Expression.TypeIs(left.Expression, left.LimitType)),
					Expression.AndAlso(
						Expression.NotEqual(left.Expression, nullExpr),
						Expression.TypeIs(right.Expression, right.LimitType))));
		}
	}
}
