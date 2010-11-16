using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;
using System.Linq.Expressions;

namespace Kurogane.RuntimeBinder {

	public class NotBinder : UnaryOperationBinder {

		public NotBinder()
			: base(ExpressionType.Not) {
		}

		public override DynamicMetaObject FallbackUnaryOperation(DynamicMetaObject target, DynamicMetaObject errorSuggestion) {
			if (target.Value == null) {
				return new DynamicMetaObject(
					BinderHelper.Wrap(Expression.Constant(true), this.ReturnType),
					BindingRestrictions.GetExpressionRestriction(BinderHelper.IsNull(target.Expression)));
			}
			if (target.LimitType == typeof(bool)) {
				return new DynamicMetaObject(
					BinderHelper.Wrap(Expression.Not(target.Expression), this.ReturnType),
					BindingRestrictions.GetTypeRestriction(target.Expression, typeof(bool)));
			}
			return new DynamicMetaObject(
				BinderHelper.Wrap(Expression.Constant(false), this.ReturnType),
				BindingRestrictions.GetTypeRestriction(target.Expression, target.LimitType));
		}
	}
}
