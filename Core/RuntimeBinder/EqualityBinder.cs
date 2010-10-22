using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;
using System.Linq.Expressions;
using System.Diagnostics;
using Kurogane.Util;

namespace Kurogane.RuntimeBinder {

	/// <summary>
	/// 等価比較を行うBinderクラス。
	/// </summary>
	public class EqualityBinder : BinaryOperationBinder {

		private readonly string _ilMethodName;

		public EqualityBinder(ExpressionType operation, string ilMethodName)
			: base(operation) {
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
			return new DynamicMetaObject(
				Expression.Convert(
					Expression.MakeBinary(this.Operation, left.Expression, right.Expression),
					typeof(object)),
				BindingRestrictions.GetExpressionRestriction(
					Expression.OrElse(
						Expression.Equal(left.Expression, nullExpr),
						Expression.Equal(right.Expression, nullExpr))));
		}

		private DynamicMetaObject CalcOnDefferentType(DynamicMetaObject left, DynamicMetaObject right) {
			if (left.LimitType == typeof(int) && right.LimitType == typeof(double)) {
				return new DynamicMetaObject(
					Expression.Convert(
						Expression.MakeBinary(this.Operation,
							Expression.Convert(Expression.Convert(left.Expression, typeof(int)), typeof(double)),
							Expression.Convert(right.Expression, typeof(double))),
						typeof(object)),
					BindingRestrictions.GetExpressionRestriction(
						Expression.AndAlso(
						Expression.TypeIs(left.Expression, typeof(int)),
						Expression.TypeIs(right.Expression, typeof(double)))));
			}
			if (left.LimitType == typeof(double) && right.LimitType == typeof(int)) {
				return new DynamicMetaObject(
					Expression.Convert(
						Expression.MakeBinary(this.Operation,
							Expression.Convert(left.Expression, typeof(double)),
							Expression.Convert(Expression.Convert(right.Expression, typeof(int)), typeof(double))),
						typeof(object)),
					BindingRestrictions.GetExpressionRestriction(
						Expression.AndAlso(
						Expression.TypeIs(left.Expression, typeof(double)),
						Expression.TypeIs(right.Expression, typeof(int)))));
			}
			return new DynamicMetaObject(
				Expression.Constant(this.Operation != ExpressionType.Equal, typeof(object)),
				BindingRestrictions.GetExpressionRestriction(
					Expression.AndAlso(
						Expression.TypeIs(left.Expression, left.LimitType),
						Expression.TypeIs(right.Expression, right.LimitType))));
		}

		private DynamicMetaObject CalcOnSameType(DynamicMetaObject left, DynamicMetaObject right) {
			return TryCallOperator(left, right)
				?? TryUseIEquatableT(left, right)
				?? ReferenceEqual(left, right);
		}

		private DynamicMetaObject TryCallOperator(DynamicMetaObject left, DynamicMetaObject right) {
			var types = new[] { left.LimitType, right.LimitType };
			var mInfo = left.LimitType.GetMethod(_ilMethodName, types) ?? right.LimitType.GetMethod(_ilMethodName, types);
			if (mInfo == null)
				return null;
			return new DynamicMetaObject(
				Expression.Convert(Expression.Call(mInfo,
						Expression.Convert(left.Expression, left.LimitType),
						Expression.Convert(right.Expression, right.LimitType)),
					typeof(object)),
				GetTypeRestriction(left, right));
		}

		private DynamicMetaObject TryUseIEquatableT(DynamicMetaObject left, DynamicMetaObject right) {
			var types = left.LimitType.GetInterfaces();
			var cmpType = typeof(IEquatable<>).MakeGenericType(right.LimitType);
			var hasGeneric = Array.IndexOf(types, cmpType) != -1;
			if (hasGeneric == false)
				return null;
			var callExpr = Expression.Call(
				Expression.Convert(left.Expression, cmpType),
				cmpType.GetMethod("Equals", new[] { right.LimitType }),
				Expression.Convert(right.Expression, right.LimitType));
			var result = Expression.Constant(this.Operation == ExpressionType.Equal ? true : false, typeof(bool));
			var expr = Expression.Convert(Expression.Equal(callExpr, result), typeof(object));
			return new DynamicMetaObject(expr, GetTypeRestriction(left, right));
		}

		private DynamicMetaObject ReferenceEqual(DynamicMetaObject left, DynamicMetaObject right) {
			var boolResult = Expression.Constant(this.Operation == ExpressionType.Equal ? true : false, typeof(bool));
			var expr = ExpressionUtil.BetaReduction(
				(object objA, object objB, bool result) => (object)(Object.ReferenceEquals(objA, objB) == result),
				left.Expression, right.Expression, boolResult);
			return new DynamicMetaObject(expr, GetTypeRestriction(left, right));
		}

		private BindingRestrictions GetTypeRestriction(DynamicMetaObject left, DynamicMetaObject right) {
			var nullExpr = Expression.Constant(null);
			return BindingRestrictions.GetExpressionRestriction(
				Expression.AndAlso(
					Expression.TypeIs(left.Expression, left.LimitType),
					Expression.TypeIs(right.Expression, right.LimitType)));
		}
	}
}
