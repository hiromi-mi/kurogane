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
			if (target.LimitType == typeof(bool)) {
				return new DynamicMetaObject(
					Expression.Convert(target.Expression, typeof(bool)),
					BindingRestrictions.GetTypeRestriction(target.Expression, typeof(bool)));
			}
			else {
				return new DynamicMetaObject(
					Expression.TypeIs(target.Expression, typeof(object)),
					BindingRestrictions.GetExpressionRestriction(Expression.Not(Expression.TypeIs(target.Expression, typeof(bool)))));
			}
		}
	}
}
