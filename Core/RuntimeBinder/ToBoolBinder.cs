using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;
using System.Linq.Expressions;
using Kurogane.Util;

namespace Kurogane.RuntimeBinder {

	public class ToBoolBinder : ConvertBinder {
		public ToBoolBinder()
			: base(typeof(bool), false) {
		}

		public override DynamicMetaObject FallbackConvert(DynamicMetaObject target, DynamicMetaObject errorSuggestion) {
			Expression boolExpr;
			BindingRestrictions rest;
			if (target.Value == null) {
				boolExpr = Expression.Constant(false);
				rest = BindingRestrictions.GetExpressionRestriction(BinderHelper.IsNull(target.Expression));
			}
			else if (target.LimitType == typeof(bool)) {
				boolExpr = target.Expression;
				rest = BindingRestrictions.GetTypeRestriction(target.Expression, typeof(bool));
			}
			else {
				boolExpr = Expression.Constant(true);
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
