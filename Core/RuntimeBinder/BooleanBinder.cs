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

		public override DynamicMetaObject FallbackBinaryOperation(DynamicMetaObject target, DynamicMetaObject arg, DynamicMetaObject errorSuggestion) {
			var expr = Expression.Condition(
				Expression.TypeIs(target.Expression, typeof(bool)),
				Expression.Condition(Expression.Convert(target.Expression, typeof(bool)), arg.Expression, target.Expression),
				Expression.Condition(Expression.Equal(target.Expression, Expression.Constant(null)), target.Expression, arg.Expression));
			return new DynamicMetaObject(expr, BindingRestrictions.Empty);
		}
	}

	/// <summary>
	/// Or演算を行うBinderクラス。
	/// </summary>
	public class OrOperationBinder : BinaryOperationBinder {

		public OrOperationBinder()
			: base(ExpressionType.Or) {
		}

		public override DynamicMetaObject FallbackBinaryOperation(DynamicMetaObject target, DynamicMetaObject arg, DynamicMetaObject errorSuggestion) {
			var expr = Expression.Condition(
				Expression.TypeIs(target.Expression, typeof(bool)),
				Expression.Condition(Expression.Convert(target.Expression, typeof(bool)), target.Expression, arg.Expression),
				Expression.Condition(Expression.Equal(target.Expression, Expression.Constant(null)), arg.Expression, target.Expression));
			return new DynamicMetaObject(expr, BindingRestrictions.Empty);
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
			var nullExpr = Expression.Default(typeof(object));
			var trueExpr = Expression.Constant(true, typeof(object));
			var falseExpr = Expression.Constant(false, typeof(object));

			var expr = Expression.Condition(
				Expression.TypeIs(target.Expression, typeof(bool)),
				Expression.Condition(Expression.Convert(target.Expression, typeof(bool)), falseExpr, trueExpr),
				Expression.Condition(Expression.Equal(target.Expression, nullExpr), trueExpr, falseExpr));
			return new DynamicMetaObject(expr, BindingRestrictions.Empty);
		}
	}
}
