using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;
using System.Linq.Expressions;
using Kurogane.Util;
using Kurogane.Libraries;

namespace Kurogane.RuntimeBinder {

	public class MapBinder : BinaryOperationBinder {

		public MapBinder()
			: base(ExpressionType.Multiply) {
		}

		/// <summary>
		/// Map関数を呼び出す。
		/// </summary>
		/// <param name="target">関数</param>
		/// <param name="arg">リスト</param>
		/// <param name="errorSuggestion"></param>
		/// <returns>リスト</returns>
		public override DynamicMetaObject FallbackBinaryOperation(DynamicMetaObject target, DynamicMetaObject arg, DynamicMetaObject errorSuggestion) {
			Expression expr = null;
			Expression rest = null;
			var funcType = typeof(Func<object, object>);
			if (target.LimitType == funcType) {
				var funcExpr = BinderHelper.Wrap(target.Expression, funcType);
				expr = ExpressionHelper.BetaReduction<Func<object, object>, object, object>(
					(func, lst) => ListLib.Map(func, lst), funcExpr, arg.Expression);
				rest = Expression.TypeIs(target.Expression, funcType);
			}
			funcType = typeof(SuffixFunc<Func<object, object>>);
			if (target.LimitType == funcType) {
				expr = ExpressionHelper.BetaReduction<SuffixFunc<Func<object, object>>, object, object>(
					(sfxFunc, lst) => ListLib.Map(sfxFunc.Func, lst),
					BinderHelper.Wrap(target.Expression, funcType), arg.Expression);
				rest = Expression.TypeIs(target.Expression, funcType);
			}
			// ----- ----- ----- return ----- ----- -----
			if (expr != null && rest != null) {
				return new DynamicMetaObject(
					BinderHelper.Wrap(expr, this.ReturnType),
					BindingRestrictions.GetExpressionRestriction(rest));
			}
			else {
				return ThrowArgumentException(
					target.LimitType + "を用いて射影（それぞれ）できません。",
					BindingRestrictions.GetTypeRestriction(target.Expression, target.LimitType));
			}
		}

		private static DynamicMetaObject ThrowArgumentException(string message, BindingRestrictions restrictions) {
			var ctorInfo = typeof(InvalidOperationException).GetConstructor(new[] { typeof(string) });
			return new DynamicMetaObject(
				Expression.Throw(Expression.New(ctorInfo, Expression.Constant(message)), typeof(object)),
				restrictions);
		}
	}
}
