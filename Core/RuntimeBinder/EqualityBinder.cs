using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;
using System.Linq.Expressions;
using System.Diagnostics;
using Kurogane.Util;
using System.Diagnostics.Contracts;

namespace Kurogane.RuntimeBinder {

	/// <summary>
	/// 等価比較を行うBinderクラス。
	/// </summary>
	public class EqualityBinder : BinaryOperationBinder {

		private readonly string _ilMethodName;

		public EqualityBinder(ExpressionType operation, string ilMethodName)
			: base(operation) {
			Contract.Requires<ArgumentException>(
				operation == ExpressionType.Equal ||
				operation == ExpressionType.NotEqual);

			_ilMethodName = ilMethodName;
		}

		public override DynamicMetaObject FallbackBinaryOperation(DynamicMetaObject left, DynamicMetaObject right, DynamicMetaObject errorSuggestion) {
			if (left.Value == null || right.Value == null)
				return FallbackOnNull(left, right);
			return
				TryCalcOnDefferentNumericType(left, right) ??
				TryImplicitCast(left, right) ??
				TryUseIEquatableT(left, right) ??
				TryReferenceEqual(left, right) ??
				Default(left, right);
		}

		/// <summary>
		/// 左辺か右辺のいずれかがnullの場合の比較
		/// </summary>
		private DynamicMetaObject FallbackOnNull(DynamicMetaObject left, DynamicMetaObject right) {
			bool value = this.Operation == ExpressionType.Equal;
			Expression restExpr = null;
			if (left.Value == null && right.Value == null) {
				restExpr = Expression.And(
					BinderHelper.IsNull(left.Expression),
					BinderHelper.IsNull(right.Expression));
			}
			else if (left.Value == null) {
				value ^= true;
				restExpr = Expression.AndAlso(
					BinderHelper.IsNull(left.Expression),
					BinderHelper.IsNotNull(right.Expression));
			}
			else {
				value ^= true;
				restExpr = Expression.AndAlso(
					BinderHelper.IsNull(right.Expression),
					BinderHelper.IsNotNull(left.Expression));
			}
			var expr = BinderHelper.Wrap(Expression.Constant(value), this.ReturnType);
			var rest = BindingRestrictions.GetExpressionRestriction(restExpr);
			return new DynamicMetaObject(expr, rest);
		}

		/// <summary>
		/// 整数と小数の比較
		/// </summary>
		private DynamicMetaObject TryCalcOnDefferentNumericType(DynamicMetaObject left, DynamicMetaObject right) {
			Expression leftExpr = BinderHelper.LimitTypeConvert(left);
			Expression rightExpr = BinderHelper.LimitTypeConvert(right);
			Expression expr = null;
			foreach (var pattern in BinderHelper.CastPatterns) {
				if (leftExpr.Type == pattern.Narrow && rightExpr.Type == pattern.Wide) {
					var upperLeft = Expression.Convert(leftExpr, pattern.Wide);
					expr = Expression.MakeBinary(this.Operation, upperLeft, rightExpr);
					break;
				}
				if (leftExpr.Type == pattern.Wide && rightExpr.Type == pattern.Narrow) {
					var upperRight = Expression.Convert(rightExpr, pattern.Wide);
					expr = Expression.MakeBinary(this.Operation, leftExpr, upperRight);
					break;
				}
			}
			if (expr == null)
				return null;
			return new DynamicMetaObject(
				BinderHelper.Wrap(expr, this.ReturnType),
				BinderHelper.GetTypeRestriction(left, right));
		}

		/// <summary>
		/// 暗黙の型変換が存在する場合，それを利用する。
		/// </summary>
		private DynamicMetaObject TryImplicitCast(DynamicMetaObject left, DynamicMetaObject right) {
			Expression cmpExpr = null;
			if (BinderHelper.GetImplicitCast(left.LimitType, right.LimitType) != null) {
				try {
					cmpExpr = Expression.MakeBinary(this.Operation,
						BinderHelper.Wrap(left.Expression, left.LimitType, right.LimitType),
						BinderHelper.Wrap(right.Expression, right.LimitType));
				}
				catch (InvalidCastException) { }
			}
			if (BinderHelper.GetImplicitCast(right.LimitType, left.LimitType) != null) {
				try {
					cmpExpr = Expression.MakeBinary(this.Operation,
						BinderHelper.Wrap(left.Expression, left.LimitType),
						BinderHelper.Wrap(right.Expression, right.LimitType, left.LimitType));
				}
				catch (InvalidCastException) { }
			}
			if (cmpExpr == null)
				return null;
			var expr = BinderHelper.Wrap(cmpExpr, this.ReturnType);
			var rest = BinderHelper.GetTypeRestriction(left, right);
			return new DynamicMetaObject(expr, rest);
		}

		/// <summary>
		/// 左辺がIEquatable<T>を継承している場合，それを利用する。
		/// </summary>
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
			var result = Expression.Constant(this.Operation == ExpressionType.Equal);
			var expr = BinderHelper.Wrap(Expression.Equal(callExpr, result), this.ReturnType);
			var rest = BinderHelper.GetTypeRestriction(left, right);
			return new DynamicMetaObject(expr, rest);
		}

		/// <summary>
		/// 左辺と右辺を参照比較する。
		/// </summary>
		private DynamicMetaObject TryReferenceEqual(DynamicMetaObject left, DynamicMetaObject right) {
			if (left.LimitType.IsValueType || right.LimitType.IsValueType)
				return null;
			var boolResult = Expression.Constant(this.Operation == ExpressionType.Equal);
			Expression expr = ExpressionHelper.BetaReduction(
				(object objA, object objB, bool result) => (object)(Object.ReferenceEquals(objA, objB) == result),
				left.Expression, right.Expression, boolResult);
			expr = BinderHelper.Wrap(expr, this.ReturnType);
			var rest = BinderHelper.GetTypeRestriction(left, right);
			return new DynamicMetaObject(expr, rest);
		}

		/// <summary>
		/// 標準の比較を用いる。
		/// 失敗した場合，右辺と左辺は等しくないものとする。
		/// </summary>
		private DynamicMetaObject Default(DynamicMetaObject left, DynamicMetaObject right) {
			Expression eqExpr = null;
			try {
				eqExpr = Expression.MakeBinary(this.Operation,
						BinderHelper.Wrap(left.Expression, left.LimitType),
						BinderHelper.Wrap(right.Expression, right.LimitType));
			}
			catch (InvalidOperationException) {
				eqExpr = Expression.Constant(this.Operation != ExpressionType.Equal);
			}
			var expr = BinderHelper.Wrap(eqExpr, this.ReturnType);
			var rest = BinderHelper.GetTypeRestriction(left, right);
			return new DynamicMetaObject(expr, rest);
		}
	}
}
