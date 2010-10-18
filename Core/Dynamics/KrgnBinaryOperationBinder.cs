using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;
using System.Linq.Expressions;

namespace Kurogane.Dynamics {
	class KrgnBinaryOperationBinder {
	}

	public class KrgnAddOperationBinder : BinaryOperationBinder {

		public KrgnAddOperationBinder()
			: base(ExpressionType.Add) {
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
			string errorMsg = "null参照です。";

			var rest = BindingRestrictions.GetExpressionRestriction(
				Expression.And(
					Expression.NotEqual(left.Expression, Expression.Constant(null)),
					Expression.NotEqual(right.Expression, Expression.Constant(null))));
			var ctorInfo = typeof(ArgumentNullException).GetConstructor(new[] { typeof(string) });
			var expr = Expression.Throw(Expression.New(ctorInfo, Expression.Constant(errorMsg)));
			return new DynamicMetaObject(expr, rest);
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
			return BindingRestrictions.GetExpressionRestriction(
				Expression.And(
					Expression.TypeIs(left.Expression, left.LimitType),
					Expression.TypeIs(right.Expression, right.LimitType)));
		}

	}
}
