using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;
using System.Linq.Expressions;
using Kurogane.Util;
using System.Reflection;
using System.Diagnostics.Contracts;

namespace Kurogane.RuntimeBinder {

	/// <summary>
	/// 比較演算用のBinderクラス。
	/// nullの比較は例外
	/// </summary>
	public class ComparingBinder : BinaryOperationBinder {

		private readonly Expression<Func<int, bool>> _compareExpr;

		public ComparingBinder(ExpressionType operation, string ilMethodName, Expression<Func<int, bool>> compareExpr)
			: base(operation) {

				Contract.Requires<ArgumentException>(
					operation == ExpressionType.LessThan ||
					operation == ExpressionType.GreaterThan ||
					operation == ExpressionType.LessThanOrEqual ||
					operation == ExpressionType.GreaterThanOrEqual);

			_compareExpr = compareExpr;
		}

		public override DynamicMetaObject FallbackBinaryOperation(DynamicMetaObject target, DynamicMetaObject arg, DynamicMetaObject errorSuggestion) {
			if (target.Value == null && arg.Value == null)
				return CompareBotheNull(target, arg);
			if (target.Value == null || arg.Value == null)
				return CompareEatherNull(target, arg);
			try {
				var expr = Expression.MakeBinary(
					this.Operation,
					BinderHelper.Wrap(target.Expression, target.LimitType),
					BinderHelper.Wrap(arg.Expression, arg.LimitType));
				return new DynamicMetaObject(
					BinderHelper.Wrap(expr, this.ReturnType),
					BinderHelper.GetTypeRestriction(target, arg));
			}
			catch (InvalidOperationException) {
				return Search(target, arg);
			}
		}

		/// <summary>
		/// 左辺、右辺ともに、nullだった場合
		/// </summary>
		private DynamicMetaObject CompareBotheNull(DynamicMetaObject left, DynamicMetaObject right) {
			Contract.Requires<ArgumentException>(left.Value == null);
			Contract.Requires<ArgumentException>(right.Value == null);

			Expression value = null;
			switch (this.Operation) {
			case ExpressionType.LessThan:
			case ExpressionType.GreaterThan:
				value = Expression.Constant(false);
				break;
			case ExpressionType.LessThanOrEqual:
			case ExpressionType.GreaterThanOrEqual:
				value = Expression.Constant(true);
				break;
			}
			var expr = BinderHelper.Wrap(value, this.ReturnType);
			var rest = BindingRestrictions.GetExpressionRestriction(
				Expression.And(
					BinderHelper.IsNull(left.Expression),
					BinderHelper.IsNull(right.Expression)));
			return new DynamicMetaObject(expr, rest);
		}

		/// <summary>
		/// 左辺、右辺のいずれかがnullで、もう一方はそうでない場合
		/// </summary>
		private DynamicMetaObject CompareEatherNull(DynamicMetaObject left, DynamicMetaObject right) {
			Contract.Requires<ArgumentException>(
				left.Value == null && right.Value != null ||
				left.Value != null && right.Value == null);

			bool? value = null;
			switch (this.Operation) {
			case ExpressionType.LessThan:
			case ExpressionType.LessThanOrEqual:
				value = false;
				break;
			case ExpressionType.GreaterThan:
			case ExpressionType.GreaterThanOrEqual:
				value = true;
				break;
			}
			Expression rest;
			bool order;
			if (left.Value == null) {
				rest = Expression.AndAlso(
					BinderHelper.IsNull(left.Expression),
					BinderHelper.IsNotNull(right.Expression));
				order = true;
			}
			else {
				rest = Expression.AndAlso(
					BinderHelper.IsNull(right.Expression),
					BinderHelper.IsNotNull(left.Expression));
				order = false;
			}
			var expr = BinderHelper.Wrap(Expression.Constant(value ^ order), this.ReturnType);
			return new DynamicMetaObject(expr, BindingRestrictions.GetExpressionRestriction(rest));
		}

		/// <summary>
		/// 比較方法を検索し、見つからなかったら例外を投げるMetaObjectを返す。
		/// </summary>
		private DynamicMetaObject Search(DynamicMetaObject left, DynamicMetaObject right) {
			return
				TryCalcDefferentNumberType(left, right) ??
				TryImplicitCast(left, right) ??
				TryUseIComparableT(left, right) ??
				BinderHelper.NoResult("比較", this.ReturnType, left, right);
		}

		/// <summary>
		/// 手動で型変換を行い、比較する。
		/// </summary>
		private DynamicMetaObject TryCalcDefferentNumberType(DynamicMetaObject left, DynamicMetaObject right) {
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
		/// 暗黙のキャストで同じ型に変換できるか試す。
		/// </summary>
		private DynamicMetaObject TryImplicitCast(DynamicMetaObject left, DynamicMetaObject right) {
			Expression cmpExpr = null;
			if (BinderHelper.GetImplicitCast(left.LimitType, right.LimitType) != null) {
				try {
					cmpExpr = Expression.MakeBinary(this.Operation,
						BinderHelper.Wrap(left.Expression, left.LimitType, right.LimitType),
						BinderHelper.Wrap(right.Expression, right.LimitType));
				}
				catch (InvalidOperationException) {}
			}
			else if (BinderHelper.GetImplicitCast(right.LimitType, left.LimitType) != null) {
				try {
					cmpExpr = Expression.MakeBinary(this.Operation,
						BinderHelper.Wrap(left.Expression, left.LimitType),
						BinderHelper.Wrap(right.Expression, right.LimitType, left.LimitType));
				}
				catch (InvalidOperationException) { }
			}
			if (cmpExpr == null)
				return null;
			var expr = BinderHelper.Wrap(cmpExpr, this.ReturnType);
			var rest = BinderHelper.GetTypeRestriction(left, right);
			return new DynamicMetaObject(expr, rest);
		}

		/// <summary>
		/// 左辺がIComparable<T>を実装している場合、CompareToメソッドを呼ぶことで比較する。
		/// </summary>
		private DynamicMetaObject TryUseIComparableT(DynamicMetaObject left, DynamicMetaObject right) {
			var types = left.LimitType.GetInterfaces();
			var cmpType = typeof(IComparable<>).MakeGenericType(right.LimitType);
			var hasGeneric = Array.IndexOf(types, cmpType) != -1;
			if (hasGeneric == false)
				return null;
			var callExpr = Expression.Call(
				Expression.Convert(left.Expression, cmpType),
				cmpType.GetMethod("CompareTo", new[] { right.LimitType }),
				Expression.Convert(right.Expression, right.LimitType));
			var expr = BinderHelper.Wrap(ExpressionHelper.BetaReduction(_compareExpr, callExpr), this.ReturnType);
			var rest = BinderHelper.GetTypeRestriction(left, right);
			return new DynamicMetaObject(expr, rest);
		}
	}
}
