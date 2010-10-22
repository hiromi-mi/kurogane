using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;
using System.Linq.Expressions;

namespace Kurogane.RuntimeBinder {

	public class ToBoolBinder : ConvertBinder {
		public ToBoolBinder()
			: base(typeof(bool), false) {
		}

		public override DynamicMetaObject FallbackConvert(DynamicMetaObject target, DynamicMetaObject errorSuggestion) {
			if (target.LimitType == typeof(bool)) {
				var expr = Expression.Convert(target.Expression, typeof(bool));
				return new DynamicMetaObject(expr, BindingRestrictions.GetTypeRestriction(target.Expression, typeof(bool)));
			}
			if (target.LimitType == typeof(object)) {
				var expr = Expression.Equal(target.Expression,  Expression.Constant(null));
				return new DynamicMetaObject(expr, BindingRestrictions.GetTypeRestriction(target.Expression, typeof(object)));
			}
			return new DynamicMetaObject(
				Expression.Constant(true),
				BindingRestrictions.GetTypeRestriction(target.Expression, target.LimitType));
		}
	}
}
