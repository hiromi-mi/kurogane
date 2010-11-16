using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;
using System.Linq.Expressions;
using Kurogane.Util;
using System.Reflection;

namespace Kurogane.RuntimeBinder {

	/// <summary>
	/// 比較演算用のBinderクラス。
	/// nullの比較は例外
	/// </summary>
	public class ComparingBinder : BinaryOperationBinder {

		private readonly string _ilMethodName;
		private readonly Expression<Func<int, bool>> _compareExpr;

		public ComparingBinder(ExpressionType operation, string ilMethodName, Expression<Func<int, bool>> compareExpr)
			: base(operation) {
			_ilMethodName = ilMethodName;
			_compareExpr = compareExpr;
		}

		public override DynamicMetaObject FallbackBinaryOperation(DynamicMetaObject target, DynamicMetaObject arg, DynamicMetaObject errorSuggestion) {
			if (target.Value == null || arg.Value == null)
				return BinderHelper.NullErrorOnOperation("比較", this.ReturnType, target, arg);
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
				throw new NotImplementedException();
			}


			//if (target.LimitType == arg.LimitType)
			//    return CalcOnSameType(target, arg);
			//else
			//    return CalcOnDefferentType(target, arg);
		}

		private DynamicMetaObject FallbackOnNull(DynamicMetaObject left, DynamicMetaObject right) {
			if (left.Value == null && right.Value == null) {

			}



			var nullExpr = Expression.Constant(null);
			return new DynamicMetaObject(
				Expression.Constant(false, typeof(object)),
				BindingRestrictions.GetExpressionRestriction(
					Expression.OrElse(
						Expression.Equal(left.Expression, nullExpr),
						Expression.Equal(right.Expression, nullExpr))));
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

			return TryCallOperator(left, right)
				?? TryUseIComparableT(left, right)
				?? TryUseComparerDefault(left, right)
				?? RuntimeBinderException.CreateMetaObject(
					String.Format("{0}と{1}を比較できません。", left.LimitType.Name, right.LimitType.Name),
					GetTypeRestriction(left, right));
		}

		private DynamicMetaObject TryCallOperator(DynamicMetaObject left, DynamicMetaObject right) {
			var types = new[] { left.LimitType, right.LimitType };
			var mInfo = left.LimitType.GetMethod(_ilMethodName, types) ?? right.LimitType.GetMethod(_ilMethodName, types);
			if (mInfo == null)
				return null;
			return new DynamicMetaObject(
				Expression.Convert(
					Expression.Call(mInfo,
						Expression.Convert(left.Expression, left.LimitType),
						Expression.Convert(right.Expression, right.LimitType)),
					typeof(object)),
				GetTypeRestriction(left, right));
		}

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
			var expr = Expression.Convert(ExpressionUtil.BetaReduction(_compareExpr, callExpr), typeof(object));
			return new DynamicMetaObject(expr, GetTypeRestriction(left, right));
		}

		private DynamicMetaObject TryUseComparerDefault(DynamicMetaObject left, DynamicMetaObject right) {
			if (left.LimitType != right.LimitType)
				return null;
			var comType = typeof(Comparer<>).MakeGenericType(left.LimitType);
			var propInfo = comType.GetProperty("Default", BindingFlags.Static);
			var callExpr = Expression.Call(
				Expression.Property((Expression)null, propInfo),
				comType.GetMethod("Compare", new []{left.LimitType, right.LimitType}),
				left.Expression,
				right.Expression);
			var expr = Expression.Convert(ExpressionUtil.BetaReduction(_compareExpr, callExpr), typeof(object));
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
