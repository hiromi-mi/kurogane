using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;
using System.Linq.Expressions;
using System.Diagnostics;

namespace Kurogane.RuntimeBinder {
	public class KrgnArithmeticBinaryOperationBinder : BinaryOperationBinder {

		private readonly string _name;

		public KrgnArithmeticBinaryOperationBinder(ExpressionType operation, string name)
			: base(operation) {
			_name = name;
		}


		public override DynamicMetaObject FallbackBinaryOperation(DynamicMetaObject target, DynamicMetaObject arg, DynamicMetaObject errorSuggestion) {
			var leftType = target.LimitType;
			var rightType = target.LimitType;

			if (target.Value == null || arg.Value == null)
				return FallbackOnNull(target, arg);

			if (leftType == rightType)
				return CalcOnSameType(target, arg);
			else
				return CalcOnDefferentType(target, arg);
		}

		private DynamicMetaObject FallbackOnNull(DynamicMetaObject left, DynamicMetaObject right) {
			var nullExpr = Expression.Constant(null);
			if (left.Value == null && right.Value != null) {
				var expr = Expression.AndAlso(
					Expression.Equal(left.Expression, nullExpr),
					Expression.NotEqual(right.Expression, nullExpr));
				return new DynamicMetaObject(right.Expression, BindingRestrictions.GetExpressionRestriction(right.Expression));
			}
			if (right.Value == null && left.Value != null) {
				var expr = Expression.AndAlso(
					Expression.Equal(right.Expression, nullExpr),
					Expression.NotEqual(left.Expression, nullExpr));
				return new DynamicMetaObject(left.Expression, BindingRestrictions.GetExpressionRestriction(left.Expression));
			}
			Debug.Assert(left.Value == null);
			Debug.Assert(right.Value == null);
			{
				var expr = Expression.And(
					Expression.Equal(left.Expression, nullExpr),
					Expression.Equal(right.Expression, nullExpr));
				return new DynamicMetaObject(nullExpr, BindingRestrictions.GetExpressionRestriction(expr));
			}
		}

		private DynamicMetaObject CalcOnSameType(DynamicMetaObject left, DynamicMetaObject right) {
			throw new NotImplementedException();
		}

		private DynamicMetaObject CalcOnDefferentType(DynamicMetaObject left, DynamicMetaObject right) {
			if (left.LimitType == typeof(int) && right.LimitType == typeof(double)) {
				var expr = Expression.Add(Expression.Convert(left.Expression, typeof(double)), right.Expression);
				return new DynamicMetaObject(expr, GetTypeRestriction(left, right));
			}
			if (left.LimitType == typeof(double) && right.LimitType == typeof(int)) {
				var expr = Expression.Add(left.Expression, Expression.Convert(right.Expression, typeof(double)));
				return new DynamicMetaObject(expr, GetTypeRestriction(left, right));
			}
			string errorMsg = String.Format("{0}と{1}を{2}できません。",
				left.LimitType.Name, right.LimitType.Name, "加算");
			return RuntimeBinderException.CreateMetaObject(errorMsg, GetTypeRestriction(left, right));
		}

		private BindingRestrictions GetTypeRestriction(DynamicMetaObject left, DynamicMetaObject right) {
			return BindingRestrictions.GetTypeRestriction(left.Expression, left.LimitType)
				.Merge(BindingRestrictions.GetTypeRestriction(right.Expression, right.LimitType));
		}
	}
}
