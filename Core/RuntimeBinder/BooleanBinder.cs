using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;
using System.Linq.Expressions;

namespace Kurogane.RuntimeBinder {

	/// <summary>
	/// And演算を行うBinderクラス。
	/// </summary>
	public class AndOperationBinder : BinaryOperationBinder {

		public AndOperationBinder()
			: base(ExpressionType.And) {
		}

		public override DynamicMetaObject FallbackBinaryOperation(DynamicMetaObject left, DynamicMetaObject right, DynamicMetaObject errorSuggestion) {
			Expression expr;
			BindingRestrictions rest;
			if (left.Value == null) {
				expr = BinderHelper.Wrap(left.Expression, this.ReturnType);
				rest = BindingRestrictions.GetExpressionRestriction(BinderHelper.IsNull(left.Expression));
			}
			else if (left.LimitType == typeof(bool)) {
				expr = BinderHelper.Wrap(
					Expression.Condition(BinderHelper.Wrap(left.Expression, typeof(bool)), right.Expression, left.Expression),
					this.ReturnType);
				rest = BindingRestrictions.GetTypeRestriction(left.Expression, typeof(bool));
			}
			else {
				expr = BinderHelper.Wrap(right.Expression, this.ReturnType);
				rest = BindingRestrictions.GetExpressionRestriction(
					Expression.AndAlso(
						BinderHelper.IsNotNull(left.Expression),
						Expression.Not(Expression.TypeIs(left.Expression, typeof(bool)))));
			}
			return new DynamicMetaObject(expr, rest);
		}
	}

	/// <summary>
	/// Or演算を行うBinderクラス。
	/// </summary>
	public class OrOperationBinder : BinaryOperationBinder {

		public OrOperationBinder()
			: base(ExpressionType.Or) {
		}

		public override DynamicMetaObject FallbackBinaryOperation(DynamicMetaObject left, DynamicMetaObject right, DynamicMetaObject errorSuggestion) {
			Expression expr;
			BindingRestrictions rest;
			if (left.Value == null) {
				expr = BinderHelper.Wrap(right.Expression, this.ReturnType);
				rest = BindingRestrictions.GetExpressionRestriction(BinderHelper.IsNull(left.Expression));
			}
			else if (left.LimitType == typeof(bool)) {
				expr = BinderHelper.Wrap(
					Expression.Condition(BinderHelper.Wrap(left.Expression, typeof(bool)), left.Expression, right.Expression),
					this.ReturnType);
				rest = BindingRestrictions.GetTypeRestriction(left.Expression, typeof(bool));
			}
			else {
				expr = BinderHelper.Wrap(left.Expression, this.ReturnType);
				rest = BindingRestrictions.GetExpressionRestriction(
					Expression.AndAlso(
						BinderHelper.IsNotNull(left.Expression),
						Expression.Not(Expression.TypeIs(left.Expression, typeof(bool)))));
			}
			return new DynamicMetaObject(expr, rest);
		}
	}

	/// <summary>
	/// Not演算を行うBinderクラス。
	/// </summary>
	public class NotOperationBinder : UnaryOperationBinder {

		public NotOperationBinder()
			: base(ExpressionType.Not) {
		}

		public override DynamicMetaObject FallbackUnaryOperation(DynamicMetaObject target, DynamicMetaObject errorSuggestion) {
			Expression boolExpr;
			BindingRestrictions rest;
			if (target.Value == null) {
				boolExpr = Expression.Constant(true);
				rest = BindingRestrictions.GetExpressionRestriction(BinderHelper.IsNull(target.Expression));
			}
			else if (target.LimitType == typeof(bool)) {
				boolExpr = Expression.Not(BinderHelper.Wrap(target.Expression, typeof(bool)));
				rest = BindingRestrictions.GetTypeRestriction(target.Expression, typeof(bool));
			}
			else {
				boolExpr = Expression.Constant(false);
				rest = BindingRestrictions.GetExpressionRestriction(
					Expression.AndAlso(
						BinderHelper.IsNotNull(target.Expression),
						Expression.Not(Expression.TypeIs(target.Expression, typeof(bool)))));
			}
			var expr = BinderHelper.Wrap(boolExpr, this.ReturnType);
			return new DynamicMetaObject(expr, rest);
		}
	}
}
