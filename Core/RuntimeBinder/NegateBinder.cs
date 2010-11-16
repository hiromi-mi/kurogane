using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;
using System.Linq.Expressions;

namespace Kurogane.RuntimeBinder {

	public class NegateBinder : UnaryOperationBinder {
		public NegateBinder()
			: base(ExpressionType.Negate) {
		}

		public override DynamicMetaObject FallbackUnaryOperation(DynamicMetaObject target, DynamicMetaObject errorSuggestion) {
			const string errorMsg = "{0}を符号反転出来ません。";

			if (target.Value == null) {
				var msg = String.Format(errorMsg, ConstantNames.NullText);
				var ctorInfo = typeof(InvalidOperationException).GetConstructor(new[] { typeof(string) });
				var expr = Expression.Throw(Expression.New(ctorInfo, Expression.Constant(errorMsg)), this.ReturnType);
				var rest = BindingRestrictions.GetExpressionRestriction(BinderHelper.IsNull(target.Expression));
				return new DynamicMetaObject(expr, rest);
			}
			try {
				var expr = BinderHelper.Wrap(Expression.Negate(Expression.Convert(target.Expression, target.LimitType)), this.ReturnType);
				var rest = BindingRestrictions.GetTypeRestriction(target.Expression, target.LimitType);
				return new DynamicMetaObject(expr, rest);
			}
			catch (InvalidOperationException) {
				var format = typeof(string).GetMethod("Format", new[] { typeof(string), typeof(object) });
				var msgExpr = Expression.Call(format, Expression.Constant(errorMsg), target.Expression);
				var ctorInfo = typeof(RuntimeBinderException).GetConstructor(new[] { typeof(string) });
				var expr = Expression.Throw(Expression.New(ctorInfo, msgExpr), this.ReturnType);
				var rest = BindingRestrictions.GetTypeRestriction(target.Expression, target.LimitType);
				return new DynamicMetaObject(expr, rest);
			}
		}
	}
}
